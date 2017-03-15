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
using SwissSdr.Api.QueryModels;
using Microsoft.AspNetCore.Authorization;
using SwissSdr.Api.Authorization;
using SwissSdr.Api.Transformers;
using Halcyon.HAL;
using SwissSdr.Api.Endpoints;

namespace SwissSdr.Api.Controllers
{
	[Route(ApiConstants.Routes.Organisations)]
	public class OrganisationsController : ControllerBase, IHasImagesEndpoint, IHasAssociationsEndpoint, IHasLibraryEndpoint, IHasJobsEndpoint, IHasPermissionsEndpoint
	{
		private readonly IMapper _mapper;
		private readonly IAsyncDocumentSession _session;
		private readonly ResourceFactory _resourceFactory;
		private readonly AssociationsEndpoint<OrganisationsController> _assocationsEndpoint;
		private readonly LibraryEndpoint<OrganisationsController> _libraryEndpoint;
		private readonly ImagesEndpoint<OrganisationsController> _imagesEndpoint;
		private readonly JobsEndpoint<OrganisationsController> _jobsEndpoint;
		private readonly PermissionsEndpoint<OrganisationsController> _permissionsEndpoint;
		private readonly PermissionService _permissionService;
		private readonly TagService _tagService;
		private readonly IAuthorizationService _authService;
		private readonly GeocodingService _geocodingService;

		public IImagesEndpoint ImagesEndpoint => _imagesEndpoint;
		public IAssociationsEndpoint AssociationsEndpoint => _assocationsEndpoint;
		public ILibraryEndpoint LibraryEndpoint => _libraryEndpoint;
		public IJobsEndpoint JobsEndpoint => _jobsEndpoint;
		public IPermissionsEndpoint PermissionsEndpoint => _permissionsEndpoint;

		public OrganisationsController(
			IAsyncDocumentSession session,
			IMapper mapper,
			ResourceFactory resourceFactory,
			AssociationsEndpoint<OrganisationsController> assocationsEndpoint,
			LibraryEndpoint<OrganisationsController> libraryEndpoint,
			ImagesEndpoint<OrganisationsController> imagesEndpoint,
			JobsEndpoint<OrganisationsController> jobsEndpoint,
			PermissionsEndpoint<OrganisationsController> permissionsEndpoint,
			PermissionService permissionService,
			TagService tagService,
			IAuthorizationService authService,
			GeocodingService geocodingService)
		{
			_session = session;
			_mapper = mapper;
			_resourceFactory = resourceFactory;

			_assocationsEndpoint = assocationsEndpoint;
			_libraryEndpoint = libraryEndpoint;
			_imagesEndpoint = imagesEndpoint;
			_jobsEndpoint = jobsEndpoint;
			_permissionsEndpoint = permissionsEndpoint;

			_permissionService = permissionService;
			_tagService = tagService;
			_authService = authService;
			_geocodingService = geocodingService;
		}

		[HttpGet]
		[ProducesResponse(typeof(PagedCollectionResource), HttpStatusCode.OK)]
		public async Task<IActionResult> GetOrganisations(int? skip, int? take, [FromQuery]OrganisationsFilterInputModel filter)
		{
			if (filter == null)
			{
				filter = new OrganisationsFilterInputModel();
			}

			RavenQueryStatistics statistics;
			var organisations = await _session.QueryFrom(filter)
				.Statistics(out statistics)
				.Paged(skip, take)
				.TransformWith<Organisations_Summary, DenormalizedOrganisationSummary>()
				.ToListAsync();

			var representation = _resourceFactory.CreatePagedSummaryCollection<OrganisationsController>(
				organisations, skip, take, statistics.TotalResults,
				(s, t) => _ => GetOrganisations(s, t, filter));

			return Ok(representation);
		}

		[HttpGet(ApiConstants.Routes.Item)]
		[ProducesResponse(typeof(OrganisationResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetOrganisation(int id)
		{
			var organisation = await _session.LoadAsyncAndThrowIfNull<Organisations_Denormalized, DenormalizedOrganisation>(id);

			var resource = CreateOrganisationRepresentation(organisation);
			return Ok(resource);
		}

		[HttpPost]
		[Authorize(Policies.CreateOrganisation)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(OrganisationResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		public async Task<IActionResult> CreateOrganisation([FromBody]OrganisationUpdateInputModel updateModel)
		{
			var organisation = new Organisation()
			{
				CreatedAt = DateTime.UtcNow
			};
			_permissionService.AddCreatorPermissions(User, organisation);

			organisation = await UpdateModel(updateModel, organisation);

			await _session.StoreAsync(organisation);
			await _session.SaveChangesAsync();

			var denormalized = await _session.LoadAsyncAndThrowIfNull<Organisations_Denormalized, DenormalizedOrganisation>(organisation.Id);
			var resource = CreateOrganisationRepresentation(denormalized);
			return this.CreatedAtAction(c => c.GetOrganisation(_session.GetIdValuePart(organisation.Id)), resource);
		}

		[HttpPut(ApiConstants.Routes.Item)]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(OrganisationResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdateOrganisation(int id, [FromBody]OrganisationUpdateInputModel updateModel)
		{
			var organisation = await _session.LoadAsync<Organisation>(id);

			if (await _authService.AuthorizeEditAsync(User, organisation))
			{
				organisation = await UpdateModel(updateModel, organisation);
				await _session.SaveChangesAsync();

				return await GetOrganisation(id);
			}

			return Forbid();
		}

		[HttpDelete(ApiConstants.Routes.Item)]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(HttpStatusCode.NoContent)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> DeleteOrganisation(int id)
		{
			var organisation = await _session.LoadAsyncAndThrowIfNull<Organisation>(id);

			if (await _authService.AuthorizeFullControlAsync(User, organisation))
			{
				_session.Delete(organisation);
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
			return await _assocationsEndpoint.GetAssociations<Organisations_Denormalized, DenormalizedOrganisation>(this, id);
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
			return await _assocationsEndpoint.UpdateAssociations<Organisation>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Images)]
		[ProducesResponse(typeof(ItemsResource<ImageResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetImages(int id)
		{
			return await _imagesEndpoint.GetImages<Organisations_Denormalized, DenormalizedOrganisation>(this, id);
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
			return await _imagesEndpoint.UpdateImages<Organisation>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Jobs)]
		[ProducesResponse(typeof(ItemsResource<JobResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetJobs(int id)
		{
			return await _jobsEndpoint.GetJobs<Organisation>(this, id);
		}

		[HttpPut(ApiConstants.Routes.Jobs)]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(ItemsResource<JobResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdateJobs(int id, [FromBody]JobsUpdateInputModel updateModel)
		{
			return await _jobsEndpoint.UpdateJobs<Organisation>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Library)]
		[ProducesResponse(typeof(ItemsResource<LibraryResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetLibrary(int id)
		{
			return await _libraryEndpoint.GetLibrary<Organisations_Denormalized, DenormalizedOrganisation>(this, id);
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
			return await _libraryEndpoint.UpdateLibrary<Organisation>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Permissions)]
		[ProducesResponse(typeof(ItemsResource<ObjectPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> GetPermissions(int id)
		{
			return await _permissionsEndpoint.GetPermissions<Organisation>(this, id);
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
			return await _permissionsEndpoint.UpdatePermissions<Organisation>(this, id, updateModel);
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
			return await _permissionsEndpoint.ReplacePermissions<Organisation>(this, id, updateModel);
		}

		private async Task<Organisation> UpdateModel(OrganisationUpdateInputModel updateModel, Organisation organisation)
		{
			_mapper.Map(updateModel, organisation);

			await _geocodingService.Geocode(organisation.ContactInfo);
			await _tagService.CreateOrUpdateClusters(updateModel.Tags);

			if (!string.IsNullOrEmpty(updateModel.ProfileImageId))
			{
				var profileImage = await _session.LoadAsync<File>(updateModel.ProfileImageId);
				if (profileImage == null)
				{
					throw new ApiException($"Could not find profile image '{updateModel.ProfileImageId}'.");
				}
			}

			return organisation;
		}

		protected HALResponse CreateOrganisationRepresentation(DenormalizedOrganisation model)
		{
			var id = _session.GetIdValuePart(model.Id);

			var resource = _mapper.Map<OrganisationResource>(model.Entity);

			var representation = resource
				.CreateRepresentation(this, _ => GetOrganisation(id))
				.AddAssociations(this, model, id)
				.AddImages(this, model, id)
				.AddLibrary(this, model, id)
				.AddJobs(this, model.Entity, id)
				.AddProfileImage(_resourceFactory, model.ProfileImageFile);

			return representation;
		}
	}
}
