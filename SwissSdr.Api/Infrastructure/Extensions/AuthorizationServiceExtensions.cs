using Microsoft.AspNetCore.Authorization;
using SwissSdr.Api.Authorization;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public static class AuthorizationServiceExtensions
    {
		public async static Task<bool> AuthorizeEditAsync(this IAuthorizationService authService, ClaimsPrincipal user, EntityBase entity)
		{
			return await authService.AuthorizeAsync(user, entity, new PermissionAuthorizationRequirement(ObjectPermission.EditContent));
		}
		public async static Task<bool> AuthorizeEditAsync(this IAuthorizationService authService, ClaimsPrincipal user, IDenormalizedEntity entity)
		{
			return await authService.AuthorizeAsync(user, entity, new PermissionAuthorizationRequirement(ObjectPermission.EditContent));
		}

		public async static Task<bool> AuthorizeFullControlAsync(this IAuthorizationService authService, ClaimsPrincipal user, EntityBase entity)
		{
			return await authService.AuthorizeAsync(user, entity, new PermissionAuthorizationRequirement(ObjectPermission.FullControl));
		}
		public async static Task<bool> AuthorizeFullControlAsync(this IAuthorizationService authService, ClaimsPrincipal user, IDenormalizedEntity entity)
		{
			return await authService.AuthorizeAsync(user, entity, new PermissionAuthorizationRequirement(ObjectPermission.FullControl));
		}
	}
}
