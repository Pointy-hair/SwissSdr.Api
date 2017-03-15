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
using SwissSdr.Api.Infrastructure;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using SwissSdr.Api.Transformers;
using SwissSdr.Api.QueryModels;
using Halcyon.HAL;

namespace SwissSdr.Api.Controllers
{
	[Route("v1/search")]
	public class SearchController : Controller
    {
        private readonly ResourceFactory _resourceFactory;
        private readonly IMapper _mapper;
		private readonly IAsyncDocumentSession _session;

		public SearchController(IAsyncDocumentSession session,
			IMapper mapper,
			ResourceFactory resourceFactory)
		{
			_session = session;
			_mapper = mapper;
			_resourceFactory = resourceFactory;
		}
		
		[HttpGet]
		[ProducesResponse(typeof(ItemsResource<SearchResultResource>), HttpStatusCode.OK)]
		public async Task<IActionResult> Search(int? skip, int? take, [FromQuery]SearchInputModel filter)
		{
			if (filter == null)
			{
				filter = new SearchInputModel();
			}

			RavenQueryStatistics statistics;
			var query = await filter.CreateQueryAsync(_session);
			var results = await query
				.Statistics(out statistics)
				.Paged(skip, take)
				.TransformWith<EntityBase_Summary, DenormalizedEntitySummary>()
				.ToListAsync();
			
			var representation = _resourceFactory.CreatePagedCollectionResource<SearchController>(
				results.Select(CreateSearchResultResource),
				skip, take, statistics.TotalResults,
				(s, t) => _ => Search(skip, take, filter));

			return Ok(representation);
		}

		private HALResponse CreateSearchResultResource(DenormalizedEntitySummary summary)
		{
			var resource = _mapper.Map<SearchResultResource>(summary);

			var representation = resource
				.CreateRepresentation()
				.AddLinks(_resourceFactory.CreateEntitySelfLink(summary.GetEntityType(), _session.GetIdValuePart(summary.Id)))
				.AddEmbeddedResource(ApiConstants.Embedded.Data, _resourceFactory.CreateSummaryResource(summary));

			return representation;
		}
	}
}
