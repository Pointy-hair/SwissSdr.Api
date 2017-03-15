using Halcyon.Web.HAL.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Converters;
using SwissSdr.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Configuration
{
	public class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
	{
		private readonly IHostingEnvironment _environment;
		private readonly IOptions<MvcJsonOptions> _jsonOptionsAccessor;
		private readonly ILoggerFactory _loggerFactory;

		public ConfigureMvcOptions(ILoggerFactory loggerFactory, IOptions<MvcJsonOptions> jsonOptionsAccessor, IHostingEnvironment environment)
		{
			_loggerFactory = loggerFactory;
			_jsonOptionsAccessor = jsonOptionsAccessor;
			_environment = environment;
		}

		public void Configure(MvcOptions options)
		{
			options.Filters.Add(new AppExceptionFilter());

			var settings = JsonSerializerSettingsProvider.CreateSerializerSettings();
			settings.ContractResolver = new CustomContractResolver();

			settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
			settings.Converters.Add(new MultilingualJsonConverter());
			settings.Converters.Add(new MoneyJsonConverter());
			settings.Converters.Add(new GeoCoordinateJsonConverter());
			settings.Converters.Add(new RichTextJsonConverter());

			options.OutputFormatters.Add(new JsonHalOutputFormatter(settings));

			options.RespectBrowserAcceptHeader = true;
		}
	}

	public class ConfigureMvcJsonOptions : IConfigureOptions<MvcJsonOptions>
	{
		public void Configure(MvcJsonOptions options)
		{
			options.SerializerSettings.ContractResolver = new CustomContractResolver();

			options.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
			options.SerializerSettings.Converters.Add(new MultilingualJsonConverter());
			options.SerializerSettings.Converters.Add(new MoneyJsonConverter());
			options.SerializerSettings.Converters.Add(new GeoCoordinateJsonConverter());
			options.SerializerSettings.Converters.Add(new RichTextJsonConverter());
		}
	}
}
