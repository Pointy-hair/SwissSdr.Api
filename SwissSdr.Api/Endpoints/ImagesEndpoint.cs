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
using SwissSdr.Shared;
using Raven.Abstractions.Data;
using Raven.Json.Linq;

namespace SwissSdr.Api.Endpoints
{
	public class ImagesEndpoint<TController> : IImagesEndpoint
		where TController : ControllerBase, IHasImagesEndpoint
	{
		private readonly IAuthorizationService _authService;
		private readonly IAsyncDocumentSession _session;

		public ImagesEndpoint(IAsyncDocumentSession session,
			IAuthorizationService authService)
		{
			_session = session;
			_authService = authService;
		}

		public async Task<IActionResult> GetImages<TTransformer, TDenormalized>(TController controller, int id)
			where TTransformer : AbstractTransformerCreationTask, new()
			where TDenormalized : IHasDenormalizedImages
		{
			if (controller == null)
			{
				throw new ArgumentNullException(nameof(controller));
			}

			var model = await _session.LoadAsyncAndThrowIfNull<TTransformer, TDenormalized>(id);
			var resource = CreateImageResource(model);

			var representation = resource
				.CreateRepresentation(controller, _ => _.GetImages(id));

			return controller.Ok(representation);
		}

		public async Task<IActionResult> UpdateImages<T>(TController controller, int id, ImagesUpdateInputModel updateModel)
			where T : EntityBase, IHasImages
		{
			var model = await _session.LoadAsyncAndThrowIfNull<T>(id);

			if (await _authService.AuthorizeEditAsync(controller.User, model))
			{
				await EnsureFilesExist(updateModel);

				var patches = new[] {
					new PatchRequest()
					{
						Type = PatchCommandType.Set,
						Name = nameof(IHasImages.ImageIds),
						Value = RavenJToken.FromObject(updateModel.FileIds.ToArray())
					}
				};

				await _session.Advanced.DocumentStore.AsyncDatabaseCommands.PatchAsync(model.Id, patches);
				return await controller.GetImages(id);
			}

			return controller.Forbid();
		}

		private async Task EnsureFilesExist(ImagesUpdateInputModel updateModel)
		{
			var files = await _session.LoadAsync<File>(updateModel.FileIds);

			// don't do change tracking on targets, provokes conflicts with patching
			foreach (var file in files)
			{
				_session.Advanced.Evict(file);
			}

			var filesNotFound = files.Zip(updateModel.FileIds, (f, id) => new { Id = id, Exists = f != null }).Where(x => !x.Exists);
			if (filesNotFound.Any())
			{
				throw new ApiException($"Could not find image file(s) {string.Join(", ", filesNotFound.Select(x => $"'{x.Id}'"))}.");
			}
		}

		public ItemsResource<ImageResourceItem> CreateImageResource(IHasDenormalizedImages hasImages)
		{
			var items = hasImages.Entity.ImageIds.Select(imageId =>
			{
				var file = hasImages.ImageFiles.FirstOrDefault(f => f.Id == imageId);
				return new ImageResourceItem()
				{
					FileId = imageId,
					Name = file?.Name,
					Description = file?.Description,
					Urls = new Dictionary<ImageSize, string>()
					{
						{ ImageSize.Thumbnail, file?.GetImageUrl(ImageSize.Thumbnail) },
						{ ImageSize.Large, file?.GetImageUrl(ImageSize.Large) },
						{ ImageSize.Original, file?.Url }
					}
				};
			});

			return new ItemsResource<ImageResourceItem>()
			{
				Items = items
			};
		}
	}
}
