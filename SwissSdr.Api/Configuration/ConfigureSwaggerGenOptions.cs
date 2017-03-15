using System;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;
using SwissSdr.Shared;
using System.Collections.Generic;

namespace SwissSdr.Api.Configuration
{
	public class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
	{
		public void Configure(SwaggerGenOptions options)
		{
			options.DescribeAllEnumsAsStrings();
			options.DescribeStringEnumsInCamelCase();
			options.DescribeAllParametersInCamelCase();
			// we can't yet use xml comments with 1.0.0-rc3 because of https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/316
			// which should be fixed in next release
			//options.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "SwissSdr.Api.xml"));

			options.AddSecurityDefinition("oauth2", new OAuth2Scheme()
			{
				Type = "oauth2",
				Flow = "implicit",
				AuthorizationUrl = $"{SwissSdrConstants.Authority}/connect/authorize",
				Scopes = new Dictionary<string, string>
				{
					{ "swisssdr-api", "Access swiss-sdr.ch API" }
				}
			});
			options.CustomSchemaIds(t => t.FullName);

			options.SwaggerDoc("v1", new Info()
			{
				Title = "swiss-sdr.ch API",
				Version = "v1",
			});
		}
	}
}
