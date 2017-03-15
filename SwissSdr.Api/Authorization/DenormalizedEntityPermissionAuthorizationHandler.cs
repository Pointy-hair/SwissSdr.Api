using Microsoft.AspNetCore.Authorization;
using Raven.Client.Linq;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwissSdr.Api.Authorization
{
	public class DenormalizedEntityPermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement, IDenormalizedEntity>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement, IDenormalizedEntity resource)
		{
			var userId = context.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
			var userPermissions = Enumerable.Empty<ObjectPermission>();

			if (!string.IsNullOrEmpty(userId) && resource.Permissions.TryGetValue(userId, out userPermissions))
			{
				if (requirement.RequiredPermissions.All(p => userPermissions.Contains(p)))
				{
					context.Succeed(requirement);
				}
			}

			return Task.CompletedTask;
		}
	}
}
