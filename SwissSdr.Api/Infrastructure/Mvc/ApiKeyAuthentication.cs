using Microsoft.AspNetCore.Builder;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public static class ApiKeyAuthenticationExtensions
    {
		public static void UseApiKeyAuthentication(this IApplicationBuilder app, string apiKey)
		{
			app.Use(async (context, next) =>
			{
				string requestKey = context.Request.Query["api_key"];

				if (string.IsNullOrEmpty(requestKey))
				{
					requestKey = context.Request.Headers["X-Api-Key"];
				}

				if (!string.IsNullOrEmpty(requestKey))
				{
					if (requestKey == apiKey)
					{
						var claims = new Claim[] {
							new Claim("sub", "Users/9999"),
							new Claim(Datamodel.ClaimTypes.AdministerUsers, ""),
							new Claim(Datamodel.ClaimTypes.BypassObjectPermissions, ""),
							new Claim(Datamodel.ClaimTypes.CreateEntityOfType, EntityTypeNames.Person),
							new Claim(Datamodel.ClaimTypes.CreateEntityOfType, EntityTypeNames.Project),
							new Claim(Datamodel.ClaimTypes.CreateEntityOfType, EntityTypeNames.Organisation),
							new Claim(Datamodel.ClaimTypes.CreateEntityOfType, EntityTypeNames.Event),
							new Claim(Datamodel.ClaimTypes.CreateEntityOfType, EntityTypeNames.Topic)
							};
						context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "api_key"));
					}
				}

				await next.Invoke();
			});
		}
	}
}
