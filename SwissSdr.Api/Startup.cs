using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using SwissSdr.Api.Configuration;
using SwissSdr.Api.Infrastructure;
using SwissSdr.Api.Authorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using SwissSdr.Datamodel;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using SwissSdr.Api.Mapping;
using SwissSdr.Shared;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SwissSdr.Api
{
	public class Startup
	{
		private readonly IList<CultureInfo> _supportedCultures = new[]
		{
			new CultureInfo("en"),
			new CultureInfo("de"),
			new CultureInfo("fr"),
			new CultureInfo("it"),
		};

		private IConfigurationRoot _configuration;
		private ILoggerFactory _loggerFactory;

		public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory;

			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName.ToLowerInvariant()}.json", optional: true)
				.AddEnvironmentVariables();

			if (env.IsDevelopment())
			{
				builder.AddUserSecrets<Startup>();
			}

			_configuration = builder.Build();
		}

		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services.AddOptions();

			services.Configure<RavenDbOptions>(_configuration.GetSection(RavenDbOptions.Name));
			services.Configure<AzureStorageOptions>(_configuration.GetSection(AzureStorageOptions.Name));
			services.Configure<GeocodingOptions>(_configuration.GetSection(GeocodingOptions.Name));
			services.Configure<AzureFunctionsOptions>(_configuration.GetSection(AzureFunctionsOptions.Name));

			services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();
			services.AddSingleton<IConfigureOptions<MvcJsonOptions>, ConfigureMvcJsonOptions>();
			services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>();

			services.AddCors();
			services.AddAuthorization(Policies.CreatePolicies);
			services.AddMvc()
				.AddTypedRouting()
				.AddFluentValidation(options =>
				{
					options.RegisterValidatorsFromAssemblyContaining<Startup>();
				});
			services.AddSwaggerGen(null);

			services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddSingleton<IAuthorizationHandler, EntityBasePermissionAuthorizationHandler>();
			services.AddSingleton<IAuthorizationHandler, BypassPermissionAuthorizationHandler>();
			services.AddSingleton<IAuthorizationHandler, CreateEntityAuthorizationHandler>();

			// autofac
			var builder = new ContainerBuilder();

			builder.RegisterModule(new ApiModule(_configuration, _loggerFactory));
			builder.RegisterModule(new MapperModule());
			builder.Populate(services);
			
			return new AutofacServiceProvider(builder.Build());
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			_loggerFactory.AddAzureWebAppDiagnostics();

			if (env.IsDevelopment())
			{
				_loggerFactory.AddConsole(_configuration.GetSection("Logging"));
				_loggerFactory.AddDebug();
				app.UseDeveloperExceptionPage();
			}

			if (env.IsDevelopment() || env.IsStaging())
			{
				app.UseApiKeyAuthentication(_configuration.GetValue<string>("ApiAuthKey"));
			}

			app.UseCors(options => options
				.WithOrigins("https://swiss-sdr.ch", 
					"https://www.swiss-sdr.ch", 
					"https://app.swiss-sdr.ch", 
					"https://swisssdr-development.azurewebsites.net", 
					"https://swisssdr.novu.io:3000", 
					"https://swisssdr.novu.io")
				.WithExposedHeaders("Location")
				.AllowAnyMethod()
				.AllowAnyHeader()
				.SetPreflightMaxAge(TimeSpan.FromHours(1)));

			app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions()
			{
				Authority = SwissSdrConstants.Authority,
				AllowedScopes = { ApiConstants.ApiScope },

				AutomaticAuthenticate = true,
				AutomaticChallenge = true
			});

			app.UseRequestLocalization(new RequestLocalizationOptions
			{
				DefaultRequestCulture = new RequestCulture("en"),
				SupportedCultures = _supportedCultures,
				SupportedUICultures = _supportedCultures
			});

			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "swiss-sdr.ch API");
				c.ConfigureOAuth2("swaggerui", "secret", "swagger-ui-realm", "Swagger UI");
			});

			app.UseMvc();

			var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
			DenormalizedFileSummaryExtensions.HttpContextAccessor = httpContextAccessor;
		}
	}
}
