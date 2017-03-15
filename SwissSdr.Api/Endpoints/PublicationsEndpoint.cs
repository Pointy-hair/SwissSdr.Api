using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client;
using SwissSdr.Api.InputModels;
using SwissSdr.Api.Services;
using Raven.Client.Indexes;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using SwissSdr.Api.Resources;

namespace SwissSdr.Api.Endpoints
{
	public class PublicationsEndpoint<TController> : IPublicationsEndpoint
		where TController : ControllerBase, IHasPublicationsEndpoint
	{
		private readonly IAuthorizationService _authService;
		private readonly IAsyncDocumentSession _session;

		public PublicationsEndpoint(IAsyncDocumentSession session,
			IAuthorizationService authService)
		{
			_session = session;
			_authService = authService;
		}

		public async Task<IActionResult> GetPublications<TTransformer, TDenormalized>(TController controller, int id)
			where TTransformer : AbstractTransformerCreationTask, new()
			where TDenormalized : IHasDenormalizedPublications
		{
			if (controller == null)
			{
				throw new ArgumentNullException(nameof(controller));
			}

			var model = await _session.LoadAsyncAndThrowIfNull<TTransformer, TDenormalized>(id);
			var resource = CreatePublicationsResource(model);

			var representation = resource
				.CreateRepresentation(controller, _ => _.GetPublications(id));

			return controller.Ok(representation);
		}

		public async Task<IActionResult> UpdatePublications<T>(TController controller, int id, LibraryUpdateInputModel updateModel)
			where T : EntityBase, IHasPublications
		{
			var model = await _session.LoadAsyncAndThrowIfNull<T>(id);

			if (await _authService.AuthorizeEditAsync(controller.User, model))
			{
				model.Publications = updateModel.CreateLibraryItems();
				await _session.SaveChangesAsync();

				return await controller.GetPublications(id);
			}

			return controller.Forbid();
		}

		public ItemsResource<LibraryResourceItem> CreatePublicationsResource(IHasDenormalizedPublications hasPublications)
		{
			return new ItemsResource<LibraryResourceItem>()
			{
				Items = hasPublications.Entity.Publications.Select(i =>
				{
					return LibraryResourceItem.Create(i, hasPublications.PublicationFiles);
				})
			};
		}
	}
}
