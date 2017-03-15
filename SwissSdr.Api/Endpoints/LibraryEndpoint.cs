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
	public class LibraryEndpoint<TController> : ILibraryEndpoint
		where TController : ControllerBase, IHasLibraryEndpoint
	{
		private readonly IAuthorizationService _authService;
		private readonly IAsyncDocumentSession _session;

		public LibraryEndpoint(IAsyncDocumentSession session,
			IAuthorizationService authService)
		{
			_session = session;
			_authService = authService;
		}

		public async Task<IActionResult> GetLibrary<TTransformer, TDenormalized>(TController controller, int id)
			where TTransformer : AbstractTransformerCreationTask, new()
			where TDenormalized : IHasDenormalizedLibrary
		{
			if (controller == null)
			{
				throw new ArgumentNullException(nameof(controller));
			}

			var model = await _session.LoadAsyncAndThrowIfNull<TTransformer, TDenormalized>(id);
			var resource = CreateLibraryResource(model);

			var representation = resource
				.CreateRepresentation(controller, _ => _.GetLibrary(id));

			return controller.Ok(representation);
		}

		public async Task<IActionResult> UpdateLibrary<T>(TController controller, int id, LibraryUpdateInputModel updateModel)
			where T : EntityBase, IHasLibrary
		{
			var model = await _session.LoadAsyncAndThrowIfNull<T>(id);

			if (await _authService.AuthorizeEditAsync(controller.User, model))
			{
				model.Library = updateModel.CreateLibraryItems();
				await _session.SaveChangesAsync();

				return await controller.GetLibrary(id);
			}

			return controller.Forbid();
		}

		public ItemsResource<LibraryResourceItem> CreateLibraryResource(IHasDenormalizedLibrary hasLibrary)
		{
			return new ItemsResource<LibraryResourceItem>()
			{
				Items = hasLibrary.Entity.Library.Select(i =>
				{
					return LibraryResourceItem.Create(i, hasLibrary.LibraryFiles);
				})
			};
		}
	}
}
