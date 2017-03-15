using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Raven.Client;
using Raven.Client.Linq;
using AutoMapper;
using SwissSdr.Datamodel;
using SwissSdr.Api.Resources;
using SwissSdr.Api.InputModels;
using SwissSdr.Api.Indexes;
using System.Collections.Generic;
using RestApiHelpers.Validation;
using SwissSdr.Api;
using SwissSdr.Api.Services;
using System.Linq.Expressions;
using SwissSdr.Api.Infrastructure;
using System.Collections.ObjectModel;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using SwissSdr.Api.Authorization;
using SwissSdr.Api.Transformers;
using SwissSdr.Api.QueryModels;
using Halcyon.HAL;
using SwissSdr.Api.Endpoints;

namespace SwissSdr.Api.Controllers
{
	[Route(ApiConstants.Routes.EventSessions)]
	public class EventSessionsController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IAsyncDocumentSession _session;
		private readonly ResourceFactory _resourceFactory;
		private readonly PermissionService _permissionService;
		private readonly IAuthorizationService _authService;

		public EventSessionsController(
			IAsyncDocumentSession session,
			IMapper mapper,
			ResourceFactory resourceFactory,
			PermissionService permissionService,
			IAuthorizationService authService)
		{
			_session = session;
			_mapper = mapper;
			_resourceFactory = resourceFactory;
			_permissionService = permissionService;
			_authService = authService;
		}

		[HttpGet]
		[ProducesResponse(HttpStatusCode.OK)]
		public async Task<IActionResult> GetEventSessions(int eventId)
		{
			var sessions = await _session.Advanced.LoadStartingWithAsync<EventSession>(EventSession.GetPartialId(eventId), pageSize: 512);

			var sessionRepresentations = sessions
				.Select(x => CreateEventSessionRepresentation(eventId, x));

			var representation = new HALResponse(new CollectionResource())
				.AddLinks(this.CreateSelfLink(_ => GetEventSessions(eventId)))
				.AddEmbeddedCollection(ApiConstants.Embedded.Items, sessionRepresentations);

			return Ok(representation);
		}

		[HttpGet(ApiConstants.Routes.Item)]
		[ProducesResponse(typeof(EventSessionResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetEventSession(int eventId, int id)
		{
			var data = await _session.LoadAsyncAndThrowIfNull<EventSession>(EventSession.GetId(eventId, id));

			var representation = CreateEventSessionRepresentation(eventId, data);
			return Ok(representation);
		}

		[HttpPost]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(EventSessionResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> CreateEventSession(int eventId, [FromBody]EventSessionUpdateInputModel updateModel)
		{
			var evnt = await _session.LoadAsyncAndThrowIfNull<Event>(eventId);

			if (await _authService.AuthorizeEditAsync(User, evnt))
			{
				var session = new EventSession()
				{
					Id = EventSession.GetPartialId(eventId)
				};
				_mapper.Map(updateModel, session);
				await _session.StoreAsync(session);
				await _session.SaveChangesAsync();

				var representation = CreateEventSessionRepresentation(eventId, session);
				return this.CreatedAtAction(_ => GetEventSession(eventId, _session.GetIdValuePart(session.Id)), representation);
			}

			return Forbid();
		}

		[HttpPut(ApiConstants.Routes.Item)]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(EventResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdateEventSession(int eventId, int id, [FromBody]EventSessionUpdateInputModel updateModel)
		{
			var evnt = await _session.LoadAsyncAndThrowIfNull<Event>(eventId);
			var session = await _session.LoadAsyncAndThrowIfNull<EventSession>(EventSession.GetId(eventId, id));

			if (await _authService.AuthorizeEditAsync(User, evnt))
			{
				_mapper.Map(updateModel, session);
				await _session.SaveChangesAsync();

				return await GetEventSession(eventId, id);
			}

			return Forbid();
		}

		[HttpDelete(ApiConstants.Routes.Item)]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(HttpStatusCode.NoContent)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> DeleteEventSession(int eventId, int id)
		{
			var evnt = await _session.LoadAsyncAndThrowIfNull<Event>(eventId);
			var session = await _session.LoadAsyncAndThrowIfNull<EventSession>(EventSession.GetId(eventId, id));

			if (await _authService.AuthorizeFullControlAsync(User, evnt))
			{
				_session.Delete(session);
				await _session.SaveChangesAsync();

				return NoContent();
			}

			return Forbid();
		}

		protected HALResponse CreateEventSessionRepresentation(int eventId, EventSession model)
		{
			var id = _session.GetIdValuePart(model.Id);
			var resource = _mapper.Map<EventSessionResource>(model);

			var representation = resource
				.CreateRepresentation(this, _ => GetEventSession(eventId, id));

			return representation;
		}
	}
}
