using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using SwissSdr.Api.InputModels;
using Microsoft.AspNetCore.Authorization;
using SwissSdr.Api.Authorization;

namespace SwissSdr.Api.Services
{
	public class PermissionService
	{
		private readonly IHttpContextAccessor _contextAccessor;

		public PermissionService(IHttpContextAccessor contextAccessor)
		{
			_contextAccessor = contextAccessor;
		}

		public IEnumerable<ObjectPermission> GetPermissionsForCurrentUser(IDictionary<string, IEnumerable<ObjectPermission>> permissions)
		{
			if (permissions == null)
			{
				return Enumerable.Empty<ObjectPermission>();
			}

			var user = _contextAccessor.HttpContext.User;
			if (!user.Identity.IsAuthenticated)
			{
				return Enumerable.Empty<ObjectPermission>();
			}

			if (user.HasClaim(c => c.Type == ClaimTypes.BypassObjectPermissions))
			{
				return new[] { ObjectPermission.EditContent, ObjectPermission.FullControl, ObjectPermission.Impersonate, ObjectPermission.ModerateComments };
			}

			var userPermissions = Enumerable.Empty<ObjectPermission>();
			permissions.TryGetValue(user.GetSubject(), out userPermissions);
			return userPermissions;
		}

		public EntityBase ReplacePermissions(EntityBase entity, ObjectPermissionsUpdateInputModel updateModel)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}
			if (updateModel == null)
			{
				throw new ArgumentNullException(nameof(updateModel));
			}

			entity.Permissions = updateModel.Items.ToDictionary(i => i.UserId, i => i.Permissions);

			return entity;
		}

		public EntityBase UpdatePermissions(EntityBase entity, ObjectPermissionsUpdateInputModel updateModel)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}
			if (updateModel == null)
			{
				throw new ArgumentNullException(nameof(updateModel));
			}

			foreach (var item in updateModel.Items)
			{
				if (item.Permissions.Any())
				{
					entity.Permissions[item.UserId] = item.Permissions;
				}
				else
				{
					entity.Permissions.Remove(item.UserId);
				}
			}

			return entity;
		}

		public void AddCreatorPermissions(System.Security.Claims.ClaimsPrincipal user, EntityBase entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			entity.Permissions.Add(user.GetSubject(), new[] {
				ObjectPermission.Impersonate,
				ObjectPermission.ModerateComments,
				ObjectPermission.EditContent,
				ObjectPermission.FullControl
			});
		}
	}
}
