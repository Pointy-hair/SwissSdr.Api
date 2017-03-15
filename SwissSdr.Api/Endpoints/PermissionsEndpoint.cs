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
using AutoMapper;
using SwissSdr.Api.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Halcyon.HAL;
using SwissSdr.Api.Controllers;
using System.Collections.ObjectModel;

namespace SwissSdr.Api.Endpoints
{
	public class PermissionsEndpoint<TController> : IPermissionsEndpoint
		where TController : ControllerBase, IHasPermissionsEndpoint
	{
		private readonly IAuthorizationService _authService;
		private readonly IMapper _mapper;
		private readonly PermissionService _permissionService;
		private readonly ResourceFactory _resourceFactory;
		private readonly IAsyncDocumentSession _session;
		private readonly IUrlHelperFactory _urlHelperFactory;
		private readonly IActionContextAccessor _actionContextAccessor;

		public PermissionsEndpoint(IAsyncDocumentSession session,
			IMapper mapper,
			PermissionService permissionService,
			ResourceFactory resourceFactory,
			IUrlHelperFactory urlHelperFactory,
			IActionContextAccessor actionContextAccessor,
			IAuthorizationService authService)
		{
			_session = session;
			_mapper = mapper;
			_permissionService = permissionService;
			_resourceFactory = resourceFactory;
			_urlHelperFactory = urlHelperFactory;
			_actionContextAccessor = actionContextAccessor;
			_authService = authService;
		}

		public async Task<IActionResult> GetPermissions<T>(TController controller, int id)
			where T : EntityBase
		{
			var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

			var model = await _session
				.Include<T, User>(e => e.Permissions.Select(p => p.Key))
				.LoadAsyncAndThrowIfNull(id);

			if (await _authService.AuthorizeAsync(controller.User, Policies.UserAdministration) || await _authService.AuthorizeFullControlAsync(controller.User, model))
			{
				var embeddedItems = new Collection<HALResponse>();
				foreach (var kv in model.Permissions)
				{
					var user = await _session.LoadAsync<User>(kv.Key);
					if (user != null)
					{
						var permission = new ObjectPermissionResourceItem()
						{
							UserId = kv.Key,
							Permissions = kv.Value
						};

						var permissionRepresentation = new HALResponse(permission);

						var userId = _session.GetIdValuePart(user.Id);

						var userRepresentation = _mapper.Map<UserSummaryResource>(user)
							.CreateRepresentation()
							.AddLinks(urlHelper.CreateSelfLink<UsersController>(_ => _.GetUser(userId)));

						permissionRepresentation.AddEmbeddedResource(ApiConstants.Embedded.User, userRepresentation);
						embeddedItems.Add(permissionRepresentation);
					}
				}

				var representation = new HALResponse(new CollectionResource())
					.AddSelfLink(controller, _ => _.GetPermissions(id))
					.AddEmbeddedCollection(ApiConstants.Embedded.Items, embeddedItems);

				return controller.Ok(representation);
			}

			return controller.Forbid();
		}

		public async Task<IActionResult> UpdatePermissions<T>(TController controller, int id, ObjectPermissionsUpdateInputModel updateModel)
			where T : EntityBase
		{
			var model = await _session.LoadAsyncAndThrowIfNull<T>(id);

			if (await _authService.AuthorizeAsync(controller.User, Policies.UserAdministration) || await _authService.AuthorizeFullControlAsync(controller.User, model))
			{
				_permissionService.UpdatePermissions(model, updateModel);
				await _session.SaveChangesAsync();

				return await controller.GetPermissions(id);
			}

			return controller.Forbid();
		}

		public async Task<IActionResult> ReplacePermissions<T>(TController controller, int id, ObjectPermissionsUpdateInputModel updateModel)
			where T : EntityBase
		{
			var model = await _session.LoadAsyncAndThrowIfNull<T>(id);

			if (await _authService.AuthorizeAsync(controller.User, Policies.UserAdministration) || await _authService.AuthorizeFullControlAsync(controller.User, model))
			{
				_permissionService.ReplacePermissions(model, updateModel);
				await _session.SaveChangesAsync();

				return await controller.GetPermissions(id);
			}

			return controller.Forbid();
		}
	}
}
