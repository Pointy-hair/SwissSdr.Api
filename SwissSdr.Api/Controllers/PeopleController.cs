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
using System.Net;
using SwissSdr.Api.QueryModels;
using Microsoft.AspNetCore.Authorization;
using SwissSdr.Datamodel.Authorization;
using SwissSdr.Api.Authorization;
using SwissSdr.Api.Transformers;
using SwissSdr.Api.Endpoints;
using Halcyon.HAL;

namespace SwissSdr.Api.Controllers
{
	[Route(ApiConstants.Routes.People)]
	public class PeopleController : ControllerBase, IHasAssociationsEndpoint, IHasLibraryEndpoint, IHasPublicationsEndpoint, IHasPermissionsEndpoint
	{
		private readonly IMapper _mapper;
		private readonly IAsyncDocumentSession _session;
		private readonly ResourceFactory _resourceFactory;
		private readonly AssociationsEndpoint<PeopleController> _associationsEndpoint;
		private readonly LibraryEndpoint<PeopleController> _libraryEndpoint;
		private readonly PublicationsEndpoint<PeopleController> _publicationsEndpoint;
		private readonly PermissionsEndpoint<PeopleController> _permissionsEndpoint;
		private readonly PermissionService _permissionService;
		private readonly TagService _tagService;
		private readonly IAuthorizationService _authorizationService;
		private readonly GeocodingService _geocodingService;

		public IAssociationsEndpoint AssociationsEndpoint => _associationsEndpoint;
		public ILibraryEndpoint LibraryEndpoint => _libraryEndpoint;
		public IPublicationsEndpoint PublicationsEndpoint => _publicationsEndpoint;
		public IPermissionsEndpoint PermissionsEndpoint => _permissionsEndpoint;

		public PeopleController(
			IAsyncDocumentSession session,
			IMapper mapper,
			ResourceFactory resourceFactory,
			AssociationsEndpoint<PeopleController> associationsEndpoint,
			LibraryEndpoint<PeopleController> libraryEndpoint,
			PublicationsEndpoint<PeopleController> publicationsEndpoint,
			PermissionsEndpoint<PeopleController> permissionsEndpoint,
			PermissionService permissionService,
			TagService tagService,
			IAuthorizationService authorizationService,
			GeocodingService geocodingService)
		{
			_session = session;
			_mapper = mapper;
			_resourceFactory = resourceFactory;
			_associationsEndpoint = associationsEndpoint;
			_libraryEndpoint = libraryEndpoint;
			_publicationsEndpoint = publicationsEndpoint;
			_permissionsEndpoint = permissionsEndpoint;
			_permissionService = permissionService;
			_tagService = tagService;
			_authorizationService = authorizationService;
			_geocodingService = geocodingService;
		}

		[HttpGet]
		[ProducesResponse(typeof(PagedCollectionResource), HttpStatusCode.OK)]
		public async Task<IActionResult> GetPersons(int? skip, int? take, [FromQuery]PeopleFilterInputModel filter)
		{
			if (filter == null)
			{
				filter = new PeopleFilterInputModel();
			}

			RavenQueryStatistics statistics;
			var people = await _session.QueryFrom(filter)
				.Statistics(out statistics)
				.Paged(skip, take)
				.TransformWith<People_Summary, DenormalizedPersonSummary>()
				.ToListAsync();

			var representation = _resourceFactory.CreatePagedSummaryCollection<PeopleController>(
				people, skip, take, statistics.TotalResults,
				(s, t) => (_) => GetPersons(s, t, filter));

			return Ok(representation);
		}

		[HttpGet(ApiConstants.Routes.Item)]
		[ProducesResponse(typeof(PersonResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetPerson(int id)
		{
			var person = await _session.LoadAsyncAndThrowIfNull<People_Denormalized, DenormalizedPerson>(id);

			var representation = CreatePersonRepresentation(person);
			return Ok(representation);
		}

		[HttpPost]
		[Authorize(Policies.CreatePerson)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(PersonResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> CreatePerson([FromBody]PersonUpdateInputModel updateModel)
		{
			var person = new Person()
			{
				CreatedAt = DateTime.UtcNow
			};
			_permissionService.AddCreatorPermissions(User, person);

			person = await UpdateModel(updateModel, person);
			
			await _session.StoreAsync(person);
			await _session.SaveChangesAsync();

			var denormalized = await _session.LoadAsync<People_Denormalized, DenormalizedPerson>(person.Id);
			var representation = CreatePersonRepresentation(denormalized);
			return this.CreatedAtAction(c => c.GetPerson(_session.GetIdValuePart(person.Id)), representation);
		}

		[HttpPut(ApiConstants.Routes.Item)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(typeof(PersonResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdatePerson(int id, [FromBody]PersonUpdateInputModel updateModel)
		{
			var person = await _session.LoadAsyncAndThrowIfNull<Person>(id);

			if (await _authorizationService.AuthorizeEditAsync(User, person))
			{
				person = await UpdateModel(updateModel, person);
				await _session.SaveChangesAsync();

				return await GetPerson(id);
			}

			return Forbid();
		}

		[HttpDelete(ApiConstants.Routes.Item)]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(HttpStatusCode.NoContent)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> DeletePerson(int id)
		{
			var person = await _session.LoadAsyncAndThrowIfNull<Person>(id);

			if (await _authorizationService.AuthorizeFullControlAsync(User, person))
			{
				_session.Delete(person);
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
			return await _associationsEndpoint.GetAssociations<People_Denormalized, DenormalizedPerson>(this, id);
		}

		[HttpPut(ApiConstants.Routes.Associations)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(typeof(ItemsResource<AssociationResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdateAssociations(int id, [FromBody]AssociationUpdateInputModel updateModel)
		{
			return await _associationsEndpoint.UpdateAssociations<Person>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Library)]
		[ProducesResponse(typeof(ItemsResource<LibraryResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetLibrary(int id)
		{
			return await _libraryEndpoint.GetLibrary<People_Denormalized, DenormalizedPerson>(this, id);
		}

		[HttpPut(ApiConstants.Routes.Library)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(typeof(ItemsResource<LibraryResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdateLibrary(int id, [FromBody]LibraryUpdateInputModel updateModel)
		{
			return await _libraryEndpoint.UpdateLibrary<Person>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Publications)]
		[ProducesResponse(typeof(ItemsResource<LibraryResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetPublications(int id)
		{
			return await _publicationsEndpoint.GetPublications<People_Denormalized, DenormalizedPerson>(this, id);
		}

		[HttpPut(ApiConstants.Routes.Publications)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(typeof(ItemsResource<LibraryResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdatePublications(int id, [FromBody]LibraryUpdateInputModel updateModel)
		{
			return await _publicationsEndpoint.UpdatePublications<Person>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Permissions)]
		[ProducesResponse(typeof(ItemsResource<ObjectPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> GetPermissions(int id)
		{
			return await _permissionsEndpoint.GetPermissions<Person>(this, id);
		}

		[HttpPatch(ApiConstants.Routes.Permissions)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(typeof(ItemsResource<ObjectPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		[ProducesResponse(HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> UpdatePermissions(int id, [FromBody]ObjectPermissionsUpdateInputModel updateModel)
		{
			return await _permissionsEndpoint.UpdatePermissions<Person>(this, id, updateModel);
		}

		[HttpPut(ApiConstants.Routes.Permissions)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(typeof(ItemsResource<ObjectPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		[ProducesResponse(HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> ReplacePermissions(int id, [FromBody]ObjectPermissionsUpdateInputModel updateModel)
		{
			return await _permissionsEndpoint.ReplacePermissions<Person>(this, id, updateModel);
		}

		private async Task<Person> UpdateModel(PersonUpdateInputModel updateModel, Person person)
		{
			_mapper.Map(updateModel, person);

			await _geocodingService.Geocode(person.ContactInfo);
			await _tagService.CreateOrUpdateClusters(updateModel.InterestAreas);

			if (!string.IsNullOrEmpty(updateModel.ProfileImageId))
			{
				var file = await _session.LoadAsync<File>(updateModel.ProfileImageId);
				if (file == null)
				{
					throw new ApiException($"Could not find profile image '{updateModel.ProfileImageId}'.");
				}
			}

			return person;
		}

		protected HALResponse CreatePersonRepresentation(DenormalizedPerson model)
		{
			var id = _session.GetIdValuePart(model.Entity.Id);

			var resource = _mapper.Map<PersonResource>(model.Entity);

			var representation = resource
				.CreateRepresentation(this, _ => GetPerson(id))
				.AddProfileImage(_resourceFactory, model.ProfileImage)
				.AddLibrary(this, model, id)
				.AddPublications(this, model, id)
				.AddAssociations(this, model, id);

			return representation;
		}
	}
}
