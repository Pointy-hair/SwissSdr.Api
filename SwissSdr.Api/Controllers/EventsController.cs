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
	[Route(ApiConstants.Routes.Events)]
	public class EventsController : ControllerBase, IHasImagesEndpoint, IHasAssociationsEndpoint, IHasLibraryEndpoint, IHasPermissionsEndpoint
	{
		private readonly IMapper _mapper;
		private readonly IAsyncDocumentSession _session;
		private readonly ResourceFactory _resourceFactory;
		private readonly PermissionService _permissionService;
		private readonly TagService _tagService;
		private readonly IAuthorizationService _authService;
		private readonly ImagesEndpoint<EventsController> _imagesEndpoint;
		private readonly AssociationsEndpoint<EventsController> _associationsEndpoint;
		private readonly LibraryEndpoint<EventsController> _libraryEndpoint;
		private readonly PermissionsEndpoint<EventsController> _permissionsEndpoint;
		private readonly GeocodingService _geocodingService;

		public IImagesEndpoint ImagesEndpoint => _imagesEndpoint;
		public IAssociationsEndpoint AssociationsEndpoint => _associationsEndpoint;
		public ILibraryEndpoint LibraryEndpoint => _libraryEndpoint;
		public IPermissionsEndpoint PermissionsEndpoint => _permissionsEndpoint;

		public EventsController(
			IAsyncDocumentSession session,
			IMapper mapper,
			ResourceFactory resourceFactory,
			ImagesEndpoint<EventsController> imagesEndpoint,
			AssociationsEndpoint<EventsController> associationsEndpoint,
			LibraryEndpoint<EventsController> libraryEndpoint,
			PermissionsEndpoint<EventsController> permissionsEndpoint,
			PermissionService permissionService,
			TagService tagService,
			IAuthorizationService authService,
			GeocodingService geocodingService)
		{
			_session = session;
			_mapper = mapper;
			_resourceFactory = resourceFactory;
			_permissionService = permissionService;
			_tagService = tagService;
			_authService = authService;
			_imagesEndpoint = imagesEndpoint;
			_associationsEndpoint = associationsEndpoint;
			_libraryEndpoint = libraryEndpoint;
			_permissionsEndpoint = permissionsEndpoint;
			_geocodingService = geocodingService;
		}

		[HttpGet]
		[ProducesResponse(HttpStatusCode.OK)]
		public async Task<IActionResult> GetEvents(int? skip, int? take, [FromQuery]EventsFilterInputModel filter)
		{
			if (filter == null)
			{
				filter = new EventsFilterInputModel();
			}

			RavenQueryStatistics statistics;
			var events = await _session.QueryFrom(filter)
				.Statistics(out statistics)
				.OrderBy(x => x.Begin)
				.Paged(skip, take)
				.TransformWith<Events_Summary, DenormalizedEventSummary>()
				.ToListAsync();

			var representation = _resourceFactory.CreatePagedSummaryCollection<EventsController>(
				events, skip, take, statistics.TotalResults,
				(s, t) => _ => GetEvents(s, t, filter));

			return Ok(representation);
		}

		[HttpGet(ApiConstants.Routes.Item)]
		[ProducesResponse(typeof(EventResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetEvent(int id)
		{
			var evnt = await _session.LoadAsyncAndThrowIfNull<Events_Denormalized, DenormalizedEvent>(id);
			var sessions = await _session.Advanced.LoadStartingWithAsync<EventSession>(EventSession.GetPartialId(id), pageSize: 512);

			var representation = CreateEventRepresentation(evnt, sessions);
			return Ok(representation);
		}

		[HttpPost]
		[Authorize(Policies.CreateEvent)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(EventResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> CreateEvent([FromBody]EventUpdateInputModel updateModel)
		{
			var evnt = new Event()
			{
				CreatedAt = DateTime.UtcNow
			};
			_permissionService.AddCreatorPermissions(User, evnt);
			await UpdateModel(updateModel, evnt);

			await _session.StoreAsync(evnt);
			await _session.SaveChangesAsync();

			var data = await _session.LoadAsyncAndThrowIfNull<Events_Denormalized, DenormalizedEvent>(evnt.Id);
			var representation = CreateEventRepresentation(data, Enumerable.Empty<EventSession>());
			return this.CreatedAtAction(_ => GetEvent(_session.GetIdValuePart(evnt.Id)), representation);
		}

		[HttpPut(ApiConstants.Routes.Item)]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(EventResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdateEvent(int id, [FromBody]EventUpdateInputModel updateModel)
		{
			var evnt = await _session.LoadAsyncAndThrowIfNull<Event>(id);

			if (await _authService.AuthorizeEditAsync(User, evnt))
			{
				await UpdateModel(updateModel, evnt);
				await _session.SaveChangesAsync();

				return await GetEvent(id);
			}

			return Forbid();
		}

		[HttpDelete(ApiConstants.Routes.Item)]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(HttpStatusCode.NoContent)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> DeleteEvent(int id)
		{
			var evnt = await _session.LoadAsyncAndThrowIfNull<Event>(id);

			if (await _authService.AuthorizeFullControlAsync(User, evnt))
			{
				_session.Delete(evnt);
				await _session.SaveChangesAsync();

				return NoContent();
			}

			return Forbid();
		}

		[HttpGet(ApiConstants.Routes.Associations)]
		[ProducesResponse(typeof(ItemsResource<AssociationResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetAssociations(int id)
		{
			return await _associationsEndpoint.GetAssociations<Events_Denormalized, DenormalizedEvent>(this, id);
		}

		[HttpPut(ApiConstants.Routes.Associations)]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(ItemsResource<AssociationResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdateAssociations(int id, [FromBody]AssociationUpdateInputModel updateModel)
		{
			return await _associationsEndpoint.UpdateAssociations<Event>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Images)]
		[ProducesResponse(typeof(ItemsResource<ImageResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetImages(int id)
		{
			return await _imagesEndpoint.GetImages<Events_Denormalized, DenormalizedEvent>(this, id);
		}

		[HttpPut(ApiConstants.Routes.Images)]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(ItemsResource<ImageResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdateImages(int id, [FromBody]ImagesUpdateInputModel updateModel)
		{
			return await _imagesEndpoint.UpdateImages<Event>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Library)]
		[ProducesResponse(typeof(ItemsResource<LibraryResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetLibrary(int id)
		{
			return await _libraryEndpoint.GetLibrary<Events_Denormalized, DenormalizedEvent>(this, id);
		}

		[HttpPut(ApiConstants.Routes.Library)]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(ItemsResource<LibraryResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdateLibrary(int id, [FromBody]LibraryUpdateInputModel updateModel)
		{
			return await _libraryEndpoint.UpdateLibrary<Event>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Permissions)]
		[ProducesResponse(typeof(ItemsResource<ObjectPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> GetPermissions(int id)
		{
			return await _permissionsEndpoint.GetPermissions<Event>(this, id);
		}

		[HttpPatch(ApiConstants.Routes.Permissions)]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(ItemsResource<ObjectPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		[ProducesResponse(HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> UpdatePermissions(int id, [FromBody]ObjectPermissionsUpdateInputModel updateModel)
		{
			return await _permissionsEndpoint.UpdatePermissions<Event>(this, id, updateModel);
		}

		[HttpPut(ApiConstants.Routes.Permissions)]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(ItemsResource<ObjectPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		[ProducesResponse(HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> ReplacePermissions(int id, [FromBody]ObjectPermissionsUpdateInputModel updateModel)
		{
			return await _permissionsEndpoint.ReplacePermissions<Event>(this, id, updateModel);
		}

		private async Task UpdateModel(EventUpdateInputModel updateModel, Event evnt)
		{
			_mapper.Map(updateModel, evnt);
			await _geocodingService.Geocode(evnt.ContactInfo);
			await _tagService.CreateOrUpdateClusters(updateModel.Tags);
		}

		protected HALResponse CreateEventRepresentation(DenormalizedEvent model, IEnumerable<EventSession> sessions)
		{
			var id = _session.GetIdValuePart(model.Id);
			var resource = _mapper.Map<EventResource>(model.Entity);

			var sessionResources = sessions.Select(_mapper.Map<EventSessionResource>);

			var representation = resource
				.CreateRepresentation(this, _ => GetEvent(id))
				.AddImages(this, model, id)
				.AddAssociations(this, model, id)
				.AddLibrary(this, model, id)
				.AddEventSessions(this, sessionResources, id);

			return representation;
		}
	}
}
