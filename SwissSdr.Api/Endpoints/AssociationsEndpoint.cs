using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client;
using SwissSdr.Api.InputModels;
using Raven.Client.Indexes;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using SwissSdr.Api.Resources;
using Raven.Abstractions.Data;
using Raven.Abstractions.Commands;
using Raven.Json.Linq;
using MoreLinq;

namespace SwissSdr.Api.Endpoints
{
	public class AssociationsEndpoint<TController> : IAssociationsEndpoint
		where TController : ControllerBase, IHasAssociationsEndpoint
	{
		private readonly IAuthorizationService _authService;
		private readonly IAsyncDocumentSession _session;
		private readonly ResourceFactory _resourceFactory;

		private static string AssociationUpdatePatch = $@"
			var currentAssociations = typeof(this.{nameof(IHasAssociations.Associations)}['$values']) !== 'undefined' ? this.{nameof(IHasAssociations.Associations)}.$values : this.{nameof(IHasAssociations.Associations)};
			this.{nameof(IHasAssociations.Associations)} = currentAssociations.RemoveWhere(function(a) {{ return a.{nameof(Association.TargetId)} === targetId && a.{nameof(Association.TargetStub)} === null; }}).concat(newAssociations);
		";

		public AssociationsEndpoint(IAsyncDocumentSession session,
			IAuthorizationService authService,
			ResourceFactory resourceFactory)
		{
			_session = session;
			_authService = authService;
			_resourceFactory = resourceFactory;
		}

		public async Task<IActionResult> GetAssociations<TTransformer, TDenormalized>(TController controller, int id)
			where TTransformer : AbstractTransformerCreationTask, new()
			where TDenormalized : IHasDenormalizedAssociations
		{
			if (controller == null)
			{
				throw new ArgumentNullException(nameof(controller));
			}

			var model = await _session.LoadAsyncAndThrowIfNull<TTransformer, TDenormalized>(id);

			var resource = CreateAssociationResource(model, EntityTypeControllerExtensions.GetEntityTypeFromControllerType(typeof(TController)));

			var representation = resource
				.CreateRepresentation(controller, _ => _.GetAssociations(id));

			return controller.Ok(representation);
		}

		public async Task<IActionResult> UpdateAssociations<T>(TController controller, int id, AssociationUpdateInputModel updateModel)
			where T : EntityBase, IHasAssociations
		{
			var model = await _session.LoadAsyncAndThrowIfNull<T>(id);

			if (await _authService.AuthorizeEditAsync(controller.User, model))
			{
				await EnsureTargetsExist(updateModel);
				await PatchAssociations(updateModel, model);
				return await controller.GetAssociations(id);
			}

			return controller.Forbid();
		}

		private async Task EnsureTargetsExist(AssociationUpdateInputModel updateModel)
		{
			var targets = await _session.LoadAsync<EntityBase>(updateModel.Items
											.Where(i => i.AssociationType == AssociationResourceItemType.Entity)
											.Select(i => i.TargetId));

			// don't do change tracking on targets, provokes conflicts with patching
			foreach (var target in targets)
			{
				_session.Advanced.Evict(target);
			}

			var nonExistingTarget = targets.Zip(updateModel.Items, (e, i) => new { TargetId = i.TargetId, Exists = e != null }).Where(x => !x.Exists);
			if (nonExistingTarget.Any())
			{
				throw new ApiException($"Could not find association target(s) {string.Join(", ", nonExistingTarget.Select(x => $"'{x.TargetId}'"))}.");
			}
		}

		private async Task PatchAssociations<T>(AssociationUpdateInputModel updateModel, T model) where T : EntityBase, IHasAssociations
		{
			// create associations on this entity
			var newAssociations = updateModel.Items
				.Select(CreateAssociations)
				.ToArray();

			ICommandData thisPatch = new PatchCommandData()
			{
				Key = model.Id,
				Patches = new PatchRequest[]
				{
						new PatchRequest()
						{
							Type = PatchCommandType.Set,
							Name = nameof(model.Associations),
							Value = RavenJToken.FromObject(newAssociations)
						}
				}
			};

			// create patches to remove mirror associations that no longer exist
			IEnumerable<ICommandData> deletePatches = model.Associations
				.Where(a => !string.IsNullOrEmpty(a.TargetId) && a.TargetStub == null)
				.Select(a => a.TargetId)
				.Except(newAssociations.Select(a => a.TargetId))
				.Select(tid => CreateScriptedPatch(tid, model.Id, Enumerable.Empty<string>()));

			// create patches for all targeted entities to create mirror associations
			IEnumerable<ICommandData> createPatches = newAssociations
				.Where(a => !string.IsNullOrEmpty(a.TargetId) && a.TargetStub == null)
				.GroupBy(a => a.TargetId)
				.Select(g => CreateScriptedPatch(g.Key, model.Id, g.Select(a => a.Description)));

			await _session.Advanced.DocumentStore.AsyncDatabaseCommands.BatchAsync(thisPatch.Concat(deletePatches).Concat(createPatches));
		}

		private ScriptedPatchCommandData CreateScriptedPatch(string documentId, string targetId, IEnumerable<string> associationDescriptions)
		{
			return new ScriptedPatchCommandData()
			{
				Key = documentId,
				Patch = new ScriptedPatchRequest()
				{
					Script = AssociationUpdatePatch,
					Values = new Dictionary<string, object>()
					{
						{ "targetId", targetId },
						{ "newAssociations", associationDescriptions.Select(s => new Association(targetId, s)) }
					}
				}
			};
		}

		private Association CreateAssociations(AssociationUpdateInputModel.Item item)
		{
			switch (item.AssociationType)
			{
				case AssociationResourceItemType.Entity:
					return new Association(item.TargetId, item.AssociationDescription);

				case AssociationResourceItemType.Stub:
					return new Association(new EntityStub()
					{
						Type = item.TargetType,
						Name = item.Name,
						Description = item.Description,
						Url = item.Url
					}, item.AssociationDescription);

				default:
					throw new InvalidOperationException($"Can not create an association with type {item.AssociationType}");
			}
		}

		public ItemsResource<AssociationResourceItem> CreateAssociationResource(IHasDenormalizedAssociations hasAssociations, EntityType sourceType)
		{
			var resource = new ItemsResource<AssociationResourceItem>()
			{
				Items = hasAssociations.Entity.Associations
					.Select(a =>
					{
						return CreateAssociationResourceItem(a, sourceType, hasAssociations.AssociationTargets);
					})
					.WhereNotNull()
			};

			return resource;
		}

		private AssociationResourceItem CreateAssociationResourceItem(Association assocation, EntityType sourceType, IEnumerable<DenormalizedEntitySummary> targets)
		{
			if (!string.IsNullOrEmpty(assocation.TargetId))
			{
				var target = targets.FirstOrDefault(e => e.Id == assocation.TargetId);
				if (target == null)
				{
					//throw new ApiException($"Can not find assocation target with id '{assocation.TargetId}'.");
					return null;
				}

				return new AssociationResourceItem()
				{
					AssociationType = AssociationResourceItemType.Entity,
					AssociationDescription = assocation.Description,
					SourceType = sourceType,
					TargetId = assocation.TargetId,
					TargetType = target.GetEntityType(),
					Target = _resourceFactory.CreateSummaryResource(target)
				};
			}
			else if (assocation.TargetStub != null)
			{
				return new AssociationResourceItem()
				{
					AssociationType = AssociationResourceItemType.Stub,
					AssociationDescription = assocation.Description,
					SourceType = sourceType,
					TargetId = assocation.TargetId,
					TargetType = assocation.TargetStub.Type,
					Target = new StubResource()
					{
						Type = assocation.TargetStub.Type,
						Name = assocation.TargetStub.Name,
						Description = assocation.TargetStub.Description,
						Url = assocation.TargetStub.Url
					}
				};
			}

			return null;
		}
	}
}
