using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwissSdr.Datamodel;

namespace SwissSdr.Api.Authorization
{
	public class CreateEntityAuthorizationHandler : AuthorizationHandler<CreateEntityRequirement>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CreateEntityRequirement requirement)
		{
			var hasRequiredClaim = context.User.Claims
				.Where(c => c.Type == ClaimTypes.CreateEntityOfType)
				.Select(c => EntityTypeNames.Parse(c.Value))
				.Any(c => c == requirement.Type);

			if (hasRequiredClaim)
			{
				context.Succeed(requirement);
			}

			return Task.CompletedTask;
		}
	}
}
