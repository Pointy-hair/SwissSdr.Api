using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client;
using RestApiHelpers.Validation;
using SwissSdr.Api;
using SwissSdr.Api.Authorization;
using SwissSdr.Api.Indexes;
using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;
using SwissSdr.Api.Resources;
using SwissSdr.Api.Transformers;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Raven.Client.Linq;
using SwissSdr.Datamodel.Settings;
using SwissSdr.Datamodel.Authorization;
using SwissSdr.Api.Infrastructure;
using Halcyon.HAL;
using SwissSdr.Shared;

namespace SwissSdr.Api.Controllers
{
	[Route(ApiConstants.Routes.Users)]
	[Authorize(Policies.Authenticated)]
	public class UsersController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IAsyncDocumentSession _session;
		private readonly ResourceFactory _resourceFactory;

		public UsersController(
			IAsyncDocumentSession session,
			IMapper mapper,
			ResourceFactory resourceFactory)
		{
			_session = session;
			_mapper = mapper;
			_resourceFactory = resourceFactory;
		}
		
		[HttpGet]
		[ProducesResponse(typeof(ItemsResource<UserSummaryResource>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> GetUsers(int? skip, int? take, [FromQuery]UsersFilterInputModel filter)
		{
			if (filter == null)
			{
				filter = new UsersFilterInputModel();
			}

			RavenQueryStatistics statistics;
			var users = await _session.QueryFrom(filter)
				.Statistics(out statistics)
				.Paged(skip, take)
				.As<User>()
				.ToListAsync();

			var representation = _resourceFactory.CreatePagedCollectionResource<UserSummaryResource, User, UsersController>(
				users, skip, take, statistics.TotalResults,
				(s, t) => _ => GetUsers(s, t, filter),
				u => _ => GetUser(_session.GetIdValuePart(u.Id)));

			return Ok(representation);
		}

		[Authorize(Policies.UserAdministration)]
		[HttpGet(ApiConstants.Routes.Item)]
		[ProducesResponse(typeof(UserResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> GetUser(int id)
		{
			var user = await _session.LoadAsyncAndThrowIfNull<User>(id);

			var representation = (await CreateUserResource(user))
				.AddLinks(this.CreateSelfLink(_ => GetUser(id),
						  this.CreateLink(ApiConstants.Rels.Permissions, _ => GetUserPermissions(id))));

			return Ok(representation);
		}

		[Authorize(Policies.UserAdministration)]
		[HttpPut(ApiConstants.Routes.Item)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(UserResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdateUser(int id, [FromBody]UserUpdateInputModel updateModel)
		{
			var user = await _session.LoadAsyncAndThrowIfNull<User>(id);
			await UpdateUser(user, updateModel);

			await _session.SaveChangesAsync();

			return await GetUser(id);
		}

		[Authorize(Policies.UserAdministration)]
		[HttpDelete(ApiConstants.Routes.Item)]
		[ProducesResponse(HttpStatusCode.NoContent)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> DeleteUser(int id)
		{
			var user = await _session.LoadAsyncAndThrowIfNull<User>(id);

			_session.Delete(user);
			await _session.SaveChangesAsync();

			return NoContent();
		}

		[Authorize(Policies.UserAdministration)]
		[HttpGet(ApiConstants.Routes.Permissions)]
		[ProducesResponse(typeof(ItemsResource<UserPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> GetUserPermissions(int id)
		{
			var representation = (await CreateUserPermissionResource(id))
				.AddSelfLink(this, _ => GetUserPermissions(id));

			return Ok(representation);
		}

		[Authorize(Policies.UserAdministration)]
		[HttpPatch(ApiConstants.Routes.Permissions)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(ItemsResource<UserPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdateUserPermissions(int id, [FromBody]UserPermissionsUpdateInputModel updateModel)
		{
			await UpdatePermissions(_session.GetFullId<User>(id), updateModel);

			return await GetUserPermissions(id);
		}

		[Authorize(Policies.UserAdministration)]
		[HttpPut(ApiConstants.Routes.Permissions)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(ItemsResource<UserPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> ReplaceUserPermissions(int id, [FromBody]UserPermissionsUpdateInputModel updateModel)
		{
			await ReplacePermissions(_session.GetFullId<User>(id), updateModel);

			return await GetUserPermissions(id);
		}

		/*
		 * CURRENTLY LOGGED IN USER
		 */

		[HttpGet("me")]
		[ProducesResponse(typeof(UserResource), HttpStatusCode.OK)]
		public async Task<IActionResult> GetCurrentUser()
		{
			var currentUserId = _session.GetIdValuePart(User.GetSubject());
			var user = await _session.LoadAsyncAndThrowIfNull<User>(currentUserId);

			var representation = (await CreateUserResource(user))
				.AddLinks(this.CreateSelfLink(_ => GetCurrentUser(),
						  this.CreateLink(ApiConstants.Rels.Permissions, _ => GetCurrentUserPermissions())));

			return Ok(representation);
		}

		[HttpPut("me")]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(UserResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		public async Task<IActionResult> UpdateCurrentUser([FromBody]UserUpdateInputModel updateModel)
		{
			var currentUserId = _session.GetIdValuePart(User.GetSubject());
			var user = await _session.LoadAsyncAndThrowIfNull<User>(currentUserId);

			await UpdateUser(user, updateModel);
			await _session.SaveChangesAsync();

			return await GetCurrentUser();
		}

		[HttpDelete("me")]
		[ProducesResponse(HttpStatusCode.OK)]
		public async Task<IActionResult> DeleteCurrentUser()
		{
			var currentUserId = _session.GetIdValuePart(User.GetSubject());
			var user = await _session.LoadAsyncAndThrowIfNull<User>(currentUserId);

			_session.Delete(user);
			await _session.SaveChangesAsync();

			return NoContent();
		}

		[HttpGet("me/permissions")]
		[ProducesResponse(typeof(ItemsResource<UserPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> GetCurrentUserPermissions()
		{
			var currentUserId = _session.GetIdValuePart(User.GetSubject());
			var representation = (await CreateUserPermissionResource(currentUserId))
				.AddSelfLink(this, _ => GetCurrentUserPermissions());

			return Ok(representation);
		}

		[HttpPatch("me/permissions")]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(ItemsResource<UserPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> UpdateCurrentUserPermissions(int id, [FromBody]UserPermissionsUpdateInputModel updateModel)
		{
			var currentUserId = _session.GetIdValuePart(User.GetSubject());

			await UpdatePermissions(User.GetSubject(), updateModel);

			return await GetCurrentUserPermissions();
		}

		[HttpPut("me/permissions")]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(ItemsResource<UserPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> ReplaceCurrentUserPermissions(int id, [FromBody]UserPermissionsUpdateInputModel updateModel)
		{
			var currentUserId = _session.GetIdValuePart(User.GetSubject());

			await ReplacePermissions(User.GetSubject(), updateModel);

			return await GetCurrentUserPermissions();
		}

		private async Task<HALResponse> CreateUserPermissionResource(int userId)
		{
			var permissionQueryResult = await _session.Query<EntityBase_Permissions.Result, EntityBase_Permissions>()
							.Where(x => x.UserIdNumerical == userId)
							.SingleOrDefaultAsync();

			if (permissionQueryResult == null)
			{
				return new CollectionResource()
					.CreateRepresentation();
			}

			var itemRepresentations = permissionQueryResult.EntityPermissions.Select(x =>
			{
				var permissions = x.Permissions?.Values.FirstOrDefault() ?? Enumerable.Empty<ObjectPermission>();
				var item = new UserPermissionResourceItem()
				{
					EntityId = x.Id,
					Type = x.GetEntityType(),
					Permissions = permissions
				};

				return new HALResponse(item)
					.AddEmbeddedResource(ApiConstants.Embedded.User, _resourceFactory.CreateSummaryResource(x));
			});

			var representation = new CollectionResource()
				.CreateRepresentation()
				.AddEmbeddedCollection(ApiConstants.Embedded.Items, itemRepresentations);

			return representation;
		}

		private async Task<User> UpdateUser(User user, UserUpdateInputModel updateModel)
		{
			_mapper.Map(updateModel, user);

			if (!string.IsNullOrEmpty(updateModel.ProfileImageId))
			{
				var file = await _session.LoadAsync<File>(updateModel.ProfileImageId);
				if (file == null)
				{
					throw new ApiException($"Could not find profile image '{updateModel.ProfileImageId}'.");
				}

				user.ProfileImageUrl = file.GetImageUrl(ImageSize.Thumbnail);
			}

			return user;
		}

		private async Task UpdatePermissions(string userId, UserPermissionsUpdateInputModel updateModel)
		{
			if (updateModel.Items.Any())
			{
				var entities = await _session.LoadAsyncAndThrowIfNull<EntityBase>(updateModel.Items.Select(p => p.EntityId));
				foreach (var item in entities.Zip(updateModel.Items, (e, i) => new { Entity = e, UpdateModel = i }))
				{
					if (item.UpdateModel.Permissions.Any())
					{
						item.Entity.Permissions[userId] = item.UpdateModel.Permissions;
					}
					else
					{
						item.Entity.Permissions.Remove(userId);
					}
				}
			}

			await _session.SaveChangesAsync();
		}

		private async Task ReplacePermissions(string userId, UserPermissionsUpdateInputModel updateModel)
		{
			var userIdValuePart = _session.GetIdValuePart(userId);

			var permissionQueryResult = await _session.Query<EntityBase_Permissions.Result, EntityBase_Permissions>()
				.Where(x => x.UserIdNumerical == userIdValuePart)
				.SingleOrDefaultAsync();

			var entityIds = updateModel.Items.Select(p => p.EntityId)
				.Concat(permissionQueryResult.EntityPermissions.Select(e => e.Id))
				.Distinct();

			var entities = await _session.LoadAsyncAndThrowIfNull<EntityBase>(entityIds);
			if (entities.AnyNull())
			{
				throw new ApiException("Could not find all of the objects for which permissions were to be set.");
			}
			
			foreach (var entity in entities)
			{
				var updatedPermissions = updateModel.Items.SingleOrDefault(i => i.EntityId == entity.Id);
				if (updatedPermissions != null)
				{
					// entity appears in update, so either add or update permissions for this entity
					entity.Permissions[userId] = updatedPermissions.Permissions;
				}
				else
				{
					// entity currently has permissions for this user, but update is missing this entity so remove permissions
					entity.Permissions.Remove(userId);
				}
			}

			await _session.SaveChangesAsync();
		}

		private async Task<HALResponse> CreateUserResource(User model)
		{
			var id = _session.GetIdValuePart(model.Id);
			var resource = _mapper.Map<UserResource>(model);
			var representation = new HALResponse(resource);

			if (!string.IsNullOrEmpty(model.ProfileImageId))
			{
				var file = await _session.LoadAsync<Files_Summary, DenormalizedFileSummary>(model.ProfileImageId);
				if (file != null)
				{
					representation.AddEmbeddedResource(ApiConstants.Embedded.ProfileImage, _resourceFactory.CreateSummaryResource(file));
				}
			}

			var appSettings = await _session.LoadAsync<AppSettings>(AppSettings.AppSettingsId);
			foreach (var login in model.Logins)
			{
				resource.Logins.Add(CreateUserLoginResourceItem(login, appSettings.LoginSettings.Providers));
			}

			return representation;
		}

		private UserLoginResourceItem CreateUserLoginResourceItem(UserLogin login, IEnumerable<LoginProvider> providers)
		{
			var matchingProvider = providers.SingleOrDefault(p => p.Provider == login.Provider || p.Provider == login.AuthenticateVia);
			if (matchingProvider == null)
			{
				throw new InvalidOperationException($"Could not find login provider '{login.Provider}' in AppSettings.");
			}

			return new UserLoginResourceItem()
			{
				Provider = login.Provider,
				UserId = login.UserId,
				FriendlyName = login.GetFriendlyName(matchingProvider),
				RemovalUrlTemplate = login.GetRemovalUrl(SwissSdrConstants.Authority)
			};
		}
	}
}
