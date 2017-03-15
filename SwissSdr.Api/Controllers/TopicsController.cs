using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Raven.Client;
using AutoMapper;
using SwissSdr.Datamodel;
using SwissSdr.Api.Resources;
using SwissSdr.Api.InputModels;
using RestApiHelpers.Validation;
using SwissSdr.Api.Services;
using System.Net;
using SwissSdr.Api.QueryModels;
using Microsoft.AspNetCore.Authorization;
using SwissSdr.Api.Authorization;
using SwissSdr.Api.Transformers;
using Halcyon.HAL;
using SwissSdr.Api.Endpoints;

namespace SwissSdr.Api.Controllers
{
	[Route(ApiConstants.Routes.Topics)]
	public class TopicsController : ControllerBase, IHasImagesEndpoint, IHasPermissionsEndpoint
	{
		private readonly IMapper _mapper;
		private readonly IAsyncDocumentSession _session;
		private readonly ResourceFactory _resourceFactory;
		private readonly ImagesEndpoint<TopicsController> _imagesEndpoint;
		private readonly PermissionsEndpoint<TopicsController> _permissionsEndpoint;
		private readonly PermissionService _permissionService;
		private readonly TagService _tagService;
		private readonly IAuthorizationService _authorizationService;

		public IImagesEndpoint ImagesEndpoint => _imagesEndpoint;
		public IPermissionsEndpoint PermissionsEndpoint => _permissionsEndpoint;

		public TopicsController(
			IAsyncDocumentSession session,
			IMapper mapper,
			ResourceFactory resourceFactory,
			ImagesEndpoint<TopicsController> imagesEndpoint,
			PermissionsEndpoint<TopicsController> permissionsEndpoint,
			PermissionService permissionService,
			TagService tagService,
			IAuthorizationService authorizationService)
		{
			_session = session;
			_mapper = mapper;
			_resourceFactory = resourceFactory;
			_imagesEndpoint = imagesEndpoint;
			_permissionsEndpoint = permissionsEndpoint;
			_permissionService = permissionService;
			_tagService = tagService;
			_authorizationService = authorizationService;
		}

		[HttpGet]
		[ProducesResponse(typeof(PagedCollectionResource), HttpStatusCode.OK)]
		public async Task<IActionResult> GetTopics(int? skip, int? take, [FromQuery]TopicsFilterInputModel filter)
		{
			if (filter == null)
			{
				filter = new TopicsFilterInputModel();
			}

			RavenQueryStatistics statistics;
			var topics = await _session.QueryFrom(filter)
				.Statistics(out statistics)
				.Paged(skip, take)
				.TransformWith<Topics_Summary, DenormalizedTopicSummary>()
				.ToListAsync();

			var representation = _resourceFactory.CreatePagedSummaryCollection<TopicsController>(
				topics, skip, take, statistics.TotalResults,
				(s, t) => (_) => GetTopics(s, t, filter));

			return Ok(representation);
		}

		[HttpGet(ApiConstants.Routes.Item)]
		[ProducesResponse(typeof(TopicResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetTopic(int id)
		{
			var topic = await _session.LoadAsyncAndThrowIfNull<Topics_Denormalized, DenormalizedTopic>(id);

			var representation = CreateTopicRepresentation(topic);
			return Ok(representation);
		}

		[HttpPost]
		[Authorize(Policies.CreateTopic)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(TopicResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> CreateTopic([FromBody]TopicUpdateInputModel updateModel)
		{
			var topic = new Topic()
			{
				CreatedAt = DateTime.UtcNow
			};
			_permissionService.AddCreatorPermissions(User, topic);

			_mapper.Map(updateModel, topic);
			await _tagService.CreateOrUpdateClusters(updateModel.Tags);
			await _session.StoreAsync(topic);
			await _session.SaveChangesAsync();

			var data = await _session.LoadAsyncAndThrowIfNull<Topics_Denormalized, DenormalizedTopic>(topic.Id);
			var representation = CreateTopicRepresentation(data);
			return this.CreatedAtAction(c => c.GetTopic(_session.GetIdValuePart(topic.Id)), representation);
		}

		[HttpPut(ApiConstants.Routes.Item)]
		[Authorize(Policies.Authenticated)]
		[ValidateActionParameters]
		[ReturnBadRequestOnModelError]
		[ProducesResponse(typeof(TopicResource), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.BadRequest)]
		public async Task<IActionResult> UpdateTopic(int id, [FromBody]TopicUpdateInputModel updateModel)
		{
			var topic = await _session.LoadAsyncAndThrowIfNull<Topic>(id);

			if (await _authorizationService.AuthorizeEditAsync(User, topic))
			{
				_mapper.Map(updateModel, topic);
				await _tagService.CreateOrUpdateClusters(updateModel.Tags);
				await _session.SaveChangesAsync();

				return await GetTopic(id);
			}

			return Forbid();
		}

		[HttpDelete(ApiConstants.Routes.Item)]
		[Authorize(Policies.Authenticated)]
		[ProducesResponse(HttpStatusCode.NoContent)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> DeleteTopic(int id)
		{
			var topic = await _session.LoadAsyncAndThrowIfNull<Topic>(id);

			if (await _authorizationService.AuthorizeEditAsync(User, topic))
			{
				_session.Delete(topic);
				await _session.SaveChangesAsync();

				return NoContent();
			}

			return Forbid();
		}

		[HttpGet(ApiConstants.Routes.Images)]
		[ProducesResponse(typeof(ItemsResource<ImageResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetImages(int id)
		{
			return await _imagesEndpoint.GetImages<Topics_Denormalized, DenormalizedTopic>(this, id);
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
			return await _imagesEndpoint.UpdateImages<Topic>(this, id, updateModel);
		}

		[HttpGet(ApiConstants.Routes.Permissions)]
		[ProducesResponse(typeof(ItemsResource<ObjectPermissionResourceItem>), HttpStatusCode.OK)]
		[ProducesResponse(HttpStatusCode.NotFound)]
		[ProducesResponse(HttpStatusCode.Forbidden)]
		public async Task<IActionResult> GetPermissions(int id)
		{
			return await _permissionsEndpoint.GetPermissions<Topic>(this, id);
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
			return await _permissionsEndpoint.UpdatePermissions<Topic>(this, id, updateModel);
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
			return await _permissionsEndpoint.ReplacePermissions<Topic>(this, id, updateModel);
		}

		protected HALResponse CreateTopicRepresentation(DenormalizedTopic model)
		{
			var id = _session.GetIdValuePart(model.Id);

			var resource = _mapper.Map<TopicResource>(model.Entity);

			var representation = resource
				.CreateRepresentation(this, _ => GetTopic(id))
				.AddImages(this, model, id);

			return representation;
		}
	}
}
