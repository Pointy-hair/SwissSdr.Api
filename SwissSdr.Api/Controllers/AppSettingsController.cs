using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raven.Client;
using Raven.Client.Linq;
using RestApiHelpers.Validation;
using SwissSdr.Api.QueryModels;
using SwissSdr.Api.Resources;
using SwissSdr.Api.Transformers;
using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SwissSdr.Api.Controllers
{
    [Route("v1/appsettings")]
    public class AppSettingsController : ControllerBase
    {
        private readonly ILogger<AppSettingsController> _logger;
        private readonly IMapper _mapper;
        private readonly IAsyncDocumentSession _session;

        public AppSettingsController(IAsyncDocumentSession session, IMapper mapper, ILogger<AppSettingsController> logger)
        {
            _session = session;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
		[ProducesResponse(typeof(AppSettingsResource), HttpStatusCode.OK)]
        public async Task<IActionResult> GetSettings()
        {
            var appSettings = await _session.LoadAsyncAndThrowIfNull<AppSettings>(AppSettings.AppSettingsId);

            var resource = _mapper.Map<AppSettingsResource>(appSettings);
			resource.EntityTemplates = new Dictionary<string, EntityResourceBase>()
			{
				{ "people", new PersonResource() },
				{ "projects", new ProjectResource() },
				{ "organisations", new OrganisationResource() },
				{ "events", EventResource.CreateTemplate() },
				{ "topics", new TopicResource() }
			};

			var representation = resource
				.CreateRepresentation(this, _ => GetSettings());

            return Ok(representation);
        }
    }
}
