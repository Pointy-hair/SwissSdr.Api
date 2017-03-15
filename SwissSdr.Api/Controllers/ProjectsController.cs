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
using SwissSdr.Api.Endpoints;
using Halcyon.HAL;

namespace SwissSdr.Api.Controllers
{
	[Route(ApiConstants.Routes.Projects)]
	public class ProjectsController : ControllerBase, IHasAssociationsEndpoint, IHasImagesEndpoint, IHasLibraryEndpoint, IHasPublicationsEndpoint, IHasJobsEndpoint, IHasPermissionsEndpoint
	{
		private readonly IMapper _mapper;
		private readonly IAsyncDocumentSession _session;
		private readonly ResourceFactory _resourceFactory;
		private readonly AssociationsEndpoint<ProjectsController> _associationsEndpoint;
		private readonly ImagesEndpoint<ProjectsController> _imagesEndpoint;
		private readonly LibraryEndpoint<ProjectsController> _libraryEndpoint;
		private readonly PublicationsEndpoint<ProjectsController> _publicationsEndpoint;
		private readonly JobsEndpoint<ProjectsController> _jobsEndpoint;
		private readonly PermissionsEndpoint<ProjectsController> _permissionsEndpoint;
		private readonly PermissionService _permissionService;
		private readonly TagService _tagService;
		private readonly IAuthorizationService _authorizationService;
		private readonly GeocodingService _geocodingService;

		public IAssociationsEndpoint AssociationsEndpoint => _associationsEndpoint;
		public IImagesEndpoint ImagesEndpoint => _imagesEndpoint;
		public ILibraryEndpoint LibraryEndpoint => _libraryEndpoint;
		public IPublicationsEndpoint PublicationsEndpoint => _publicationsEndpoint;
		public IJobsEndpoint JobsEndpoint => _jobsEndpoint;
		public IPermissionsEndpoint PermissionsEndpoint => _permissionsEndpoint;

		public ProjectsController(
			IAsyncDocumentSession session,
			IMapper mapper,
			ResourceFactory resourceFactory,
			AssociationsEndpoint<ProjectsController> associationsEndpoint,
			ImagesEndpoint<ProjectsController> imagesEndpoint,
			LibraryEndpoint<ProjectsController> libraryEndpoint,
			PublicationsEndpoint<ProjectsController> publicationsEndpoint,
			JobsEndpoint<ProjectsController> jobsEndpoint,
			PermissionsEndpoint<ProjectsController> permissionsEndpoint,
			PermissionService permissionService,
			TagService tagService,
			IAuthorizationService authorizationService,
			GeocodingService geocodingService)
		{
			_session = session;
			_mapper = mapper;
			_resourceFactory = resourceFactory;
			_associationsEndpoint = associationsEndpoint;
			_imagesEndpoint = imagesEndpoint;
			_libraryEndpoint = libraryEndpoint;
			_publicationsEndpoint = publicationsEndpoint;
			_jobsEndpoint = jobsEndpoint;
			_permissionsEndpoint = permissionsEndpoint;
			_permissionService = permissionService;
			_tagService = tagService;
			_authorizationService = authorizationService;
			_geocodingService = geocodingService;
		}

		[HttpGet]
		[ProducesResponse(typeof(PagedCollectionResource), HttpStatusCode.OK)]
		public async Task<IActionResult> GetProjects(int? skip, int? take, [FromQuery]ProjectsFilterInputModel filter)
		{
			if (filter == null)
			{
				filter = new ProjectsFilterInputModel();
			}

			RavenQueryStatistics statistics;
			var projects = await _session.QueryFrom(filter)
				.Statistics(out statistics)
				.Paged(skip, take)
				.TransformWith<Projects_Summary, DenormalizedProjectSummary>()
				.ToListAsync();

			var resource = _resourceFactory.CreatePagedSummaryCollection<ProjectsController>(
				projects, skip, take, statistics.TotalResults,
				(s, t) => (_) => GetProjects(s, t, filter));

			return Ok(resource);
		}

		[HttpGet(ApiConstants.Routes.Item)]
		[ProducesResponse(typeof(ProjectResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetProject(int id)
		{
			var project = await _session.LoadAsyncAndThrowIfNull<Projects_Denormalized, DenormalizedProject>(id);

			var resource = CreateProjectRepresentation(project);
			return Ok(resource);
		}

		[HttpPost]
		[Authorize(Policies.CreateProject)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(ProjectResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		public async Task<IActionResult> CreateProject([FromBody]ProjectUpdateInputModel updateModel)
		{
			var project = new Project()
			{
				CreatedAt = DateTime.UtcNow
			};
			_permissionService.AddCreatorPermissions(User, project);
			await UpdateModel(updateModel, project);

			await _session.StoreAsync(project);
			await _session.SaveChangesAsync();

			var data = await _session.LoadAsyncAndThrowIfNull<Projects_Denormalized, DenormalizedProject>(project.Id);
			var resource = CreateProjectRepresentation(data);
			return this.CreatedAtAction(c => c.GetProject(_session.GetIdValuePart(project.Id)), resource);
		}

		[HttpPut(ApiConstants.Routes.Item)]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(ProjectResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdateProject(int id, [FromBody]ProjectUpdateInputModel updateModel)
		{
			var project = await _session.LoadAsyncAndThrowIfNull<Project>(id);

			if (await _authorizationService.AuthorizeEditAsync(User, project))
			{
				await UpdateModel(updateModel, project);
				await _session.SaveChangesAsync();

				return await GetProject(id);
			}

			return Forbid();
		}

		[HttpDelete(ApiConstants.Routes.Item)]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(HttpStatusCode.NoContent)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> DeleteProject(int id)
		{
			var project = await _session.LoadAsyncAndThrowIfNull<Project>(id);

			if (await _authorizationService.AuthorizeFullControlAsync(User, project))
			{
				_session.Delete(project);
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
			return await _associationsEndpoint.GetAssociations<Projects_Denormalized, DenormalizedProject>(this, id);
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
			return await _associationsEndpoint.UpdateAssociations<Project>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Images)]
		[ProducesResponse(typeof(ItemsResource<ImageResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetImages(int id)
		{
			return await _imagesEndpoint.GetImages<Projects_Denormalized, DenormalizedProject>(this, id);
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
			return await _imagesEndpoint.UpdateImages<Project>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Jobs)]
		[ProducesResponse(typeof(ItemsResource<JobResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetJobs(int id)
		{
			return await _jobsEndpoint.GetJobs<Project>(this, id);
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
			return await _jobsEndpoint.UpdateJobs<Project>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Library)]
		[ProducesResponse(typeof(ItemsResource<LibraryResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetLibrary(int id)
		{
			return await _libraryEndpoint.GetLibrary<Projects_Denormalized, DenormalizedProject>(this, id);
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
			return await _libraryEndpoint.UpdateLibrary<Project>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Publications)]
		[ProducesResponse(typeof(ItemsResource<LibraryResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetPublications(int id)
		{
			return await _publicationsEndpoint.GetPublications<Projects_Denormalized, DenormalizedProject>(this, id);
		}

		[HttpPut(ApiConstants.Routes.Publications)]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(ItemsResource<LibraryResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> UpdatePublications(int id, [FromBody]LibraryUpdateInputModel updateModel)
		{
			return await _publicationsEndpoint.UpdatePublications<Project>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Permissions)]
		[ProducesResponse(typeof(ItemsResource<ObjectPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> GetPermissions(int id)
		{
			return await _permissionsEndpoint.GetPermissions<Project>(this, id);
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
			return await _permissionsEndpoint.UpdatePermissions<Project>(this, id, updateModel);
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
			return await _permissionsEndpoint.ReplacePermissions<Project>(this, id, updateModel);
		}

		private async Task UpdateModel(ProjectUpdateInputModel updateModel, Project project)
		{
			_mapper.Map(updateModel, project);
			await _geocodingService.Geocode(project.ContactInfo);
			await _tagService.CreateOrUpdateClusters(updateModel.Tags);
		}

		protected HALResponse CreateProjectRepresentation(DenormalizedProject model)
		{
			var id = _session.GetIdValuePart(model.Id);

			var resource = _mapper.Map<ProjectResource>(model.Entity);

			var representation = resource
				.CreateRepresentation(this, _ => GetProject(id))
				.AddLibrary(this, model, id)
				.AddPublications(this, model, id)
				.AddImages(this, model, id)
				.AddJobs(this, model.Entity, id)
				.AddAssociations(this, model, id)
				.AddParentProject(_resourceFactory, model.ParentProject)
				.AddPartnerProjects(_resourceFactory, model.PartnerProjects);

			return representation;
		}
	}
}
