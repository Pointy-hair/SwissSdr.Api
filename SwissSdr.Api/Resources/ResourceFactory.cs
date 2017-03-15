using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Raven.Client;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac;
using SwissSdr.Api.Controllers;
using SwissSdr.Api.QueryModels;
using SwissSdr.Api.Transformers;
using Halcyon.HAL;

namespace SwissSdr.Api.Resources
{
	public class ResourceFactory
	{
		private readonly IMapper _mapper;
		private readonly IUrlHelperFactory _urlHelperFactory;
		private readonly IActionContextAccessor _actionContextAccessor;
		private readonly IAsyncDocumentSession _session;

		public ResourceFactory(
			IMapper mapper,
			IUrlHelperFactory urlHelperFactory,
			IActionContextAccessor actionContextAccessor,
			IAsyncDocumentSession session)
		{
			_mapper = mapper;
			_urlHelperFactory = urlHelperFactory;
			_actionContextAccessor = actionContextAccessor;
			_session = session;
		}

		public HALResponse CreateSummaryResource(IDenormalizedEntitySummary entitySummary)
		{
			if (entitySummary == null)
			{
				return null;
			}

			return CreateSummaryResource(entitySummary, entitySummary.Id, entitySummary.EntityType);
		}

		public HALResponse CreateSummaryResource(DenormalizedEntitySummary entitySummary)
		{
			if (entitySummary == null)
			{
				return null;
			}

			return CreateSummaryResource(entitySummary, entitySummary.Id, entitySummary.GetEntityType());
		}

		private HALResponse CreateSummaryResource(object source, string id, EntityType entityType)
		{
			SummaryResourceBase resource;
			switch (entityType)
			{
				case EntityType.Person:
					resource = _mapper.Map<PersonSummaryResource>(source);
					break;

				case EntityType.Project:
					resource = _mapper.Map<ProjectSummaryResource>(source);
					break;

				case EntityType.Organisation:
					resource = _mapper.Map<OrganisationSummaryResource>(source);
					break;

				case EntityType.Event:
					resource = _mapper.Map<EventSummaryResource>(source);
					break;

				case EntityType.Topic:
					resource = _mapper.Map<TopicSummaryResource>(source);
					break;

				case EntityType.File:
					resource = _mapper.Map<FileSummaryResource>(source);
					break;

				default:
					throw new InvalidOperationException($"Can not create summary resource for entity type '{entityType}'.");
			}

			var representation = new HALResponse(resource)
				.AddLinks(CreateEntitySelfLink(entityType, _session.GetIdValuePart(id)));

			if (entityType != EntityType.File) {
				DenormalizedFileSummary file = null;
				if (source is DenormalizedEntitySummary)
				{
					file = ((DenormalizedEntitySummary)source).Image;
				}
				else if (source is IDenormalizedEntitySummary)
				{
					file = ((IDenormalizedEntitySummary)source).GetImage();
				}

				if (file != null)
				{
					var fileRepresentation = CreateSummaryResource(file);
					var embedName = (entityType == EntityType.Person || entityType == EntityType.Organisation) ? ApiConstants.Embedded.ProfileImage : ApiConstants.Embedded.Image;
					representation.AddEmbeddedResource(embedName, fileRepresentation);
				}
			}

			return representation;
		}

		public Link CreateEntitySelfLink(EntityType entityType, int id)
		{
			var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

			switch (entityType)
			{
				case EntityType.Person:
					return urlHelper.CreateSelfLink<PeopleController>(_ => _.GetPerson(id));

				case EntityType.Project:
					return urlHelper.CreateSelfLink<ProjectsController>(_ => _.GetProject(id));

				case EntityType.Organisation:
					return urlHelper.CreateSelfLink<OrganisationsController>(_ => _.GetOrganisation(id));

				case EntityType.Event:
					return urlHelper.CreateSelfLink<EventsController>(_ => _.GetEvent(id));

				case EntityType.Topic:
					return urlHelper.CreateSelfLink<TopicsController>(_ => _.GetTopic(id));

				case EntityType.File:
					return urlHelper.CreateSelfLink<FilesController>(_ => _.GetFile(id));

				default:
					throw new InvalidOperationException($"Can not generate self-link for resource with entity type '{entityType}'.");
			}
		}

		public HALResponse CreateCollectionResource<TResource, T, TController>(
			IEnumerable<T> source,
			Expression<Action<TController>> resourceSelfAction,
			Func<T, Expression<Action<TController>>> itemSelfAction)
			where TController : ControllerBase
		{
			var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

			var collectionResource = new ItemsResource<T>();
			var resourceItems = source
				.Select(p =>
				{
					var resourceItem = _mapper.Map<TResource>(p);
					return new HALResponse(resourceItem)
						.AddLinks(urlHelper.CreateSelfLink(itemSelfAction(p)));
				});

			var representation = new HALResponse(collectionResource)
				.AddLinks(urlHelper.CreateSelfLink(resourceSelfAction))
				.AddEmbeddedCollection(ApiConstants.Embedded.Items, resourceItems);

			return representation;
		}

		public HALResponse CreatePagedSummaryCollection<TController>(
			IEnumerable<IDenormalizedEntitySummary> source, int? skip, int? take, int totalResults,
			Func<int, int, Expression<Func<TController, Task>>> resourceSelfAction)
			where TController : ControllerBase
		{
			var resourceItems = source.Select(CreateSummaryResource);
			return CreatePagedCollectionResource(resourceItems, skip, take, totalResults, resourceSelfAction);
		}

		public HALResponse CreatePagedCollectionResource<TResource, T, TController>(
			IEnumerable<T> source, int? skip, int? take, int totalResults,
			Func<int, int, Expression<Func<TController, Task>>> resourceSelfAction,
			Func<T, Expression<Func<TController, Task>>> itemSelfAction)
			where TController : ControllerBase
		{
			var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

			var resourceItems = source
				.Select(p =>
				{
					var resourceItem = _mapper.Map<TResource>(p);
					return new HALResponse(resourceItem)
						.AddLinks(urlHelper.CreateSelfLink(itemSelfAction(p)));
				});

			return CreatePagedCollectionResource(resourceItems, skip, take, totalResults, resourceSelfAction);
		}

		public HALResponse CreatePagedCollectionResource<TController>(
			IEnumerable<HALResponse> items, int? skip, int? take, int totalResults,
			Func<int, int, Expression<Func<TController, Task>>> resourceSelfAction)
			where TController : ControllerBase
		{
			var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

			var skipValue = skip ?? 0;
			var takeValue = take ?? ApiConstants.DefaultPageSize;
			var count = items.Count();

			var collectionResource = new PagedCollectionResource()
			{
				Skip = skipValue,
				Take = takeValue,
				TotalResults = totalResults
			};

			var representation = new HALResponse(collectionResource)
				.AddLinks(urlHelper.CreateSelfLink(resourceSelfAction(skipValue, takeValue)))
				.AddEmbeddedCollection(ApiConstants.Embedded.Items, items);

			if (count > 0 && count < totalResults)
			{
				representation.AddLinks(urlHelper.CreateLink(ApiConstants.Rels.First, resourceSelfAction.Invoke(0, takeValue)));

				if (skipValue > 0)
				{
					representation.AddLinks(urlHelper.CreateLink(ApiConstants.Rels.Prev, resourceSelfAction.Invoke(Math.Max(skipValue - takeValue, 0), takeValue)));
				}
				if (skipValue + takeValue < totalResults)
				{
					representation.AddLinks(urlHelper.CreateLink(ApiConstants.Rels.Next, resourceSelfAction.Invoke(skipValue + takeValue, takeValue)));
					representation.AddLinks(urlHelper.CreateLink(ApiConstants.Rels.Last, resourceSelfAction.Invoke(totalResults - takeValue, takeValue)));
				}
			}

			return representation;
		}
	}
}
