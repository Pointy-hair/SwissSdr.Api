using Microsoft.AspNetCore.Authorization;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwissSdr.Datamodel.Authorization;

namespace SwissSdr.Api.Authorization
{
	public class BypassPermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement, EntityBase>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement, EntityBase resource)
		{
			if (context.User.HasClaim(c => c.Type == ClaimTypes.BypassObjectPermissions))
			{
				context.Succeed(requirement);
			}

			return Task.CompletedTask;
		}
	}
}
