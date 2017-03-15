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
using System.Collections.ObjectModel;
using System.Net;
using SwissSdr.Api.Services;

namespace SwissSdr.Api.Controllers
{
	[Route("v1")]
	public class TagsController : Controller
	{
        private readonly ResourceFactory _resourceFactory;
        private readonly IMapper _mapper;
		private readonly IAsyncDocumentSession _session;
		private readonly TagService _tagService;

		public TagsController(
			IAsyncDocumentSession session,
			IMapper mapper,
			TagService tagService,
			ResourceFactory resourceFactory)
		{
			_session = session;
			_mapper = mapper;
			_tagService = tagService;
			_resourceFactory = resourceFactory;
        }

		/// <summary>
		/// Gets all <see cref="TagCluster"/>s in the system.
		/// </summary>
		/// <param name="q">Constrain results to tags starting with this value.</param>
		/// <returns></returns>
		[HttpGet("tags")]
		[ProducesResponse(typeof(ItemsResource<TagClusterResourceItem>), HttpStatusCode.OK)]
		public async Task<IActionResult> GetTags(string q)
		{
			IEnumerable<TagCluster> clusters;
			if (!string.IsNullOrEmpty(q))
			{
				clusters = await _tagService.GetClusters(q);
			}
			else
			{
				clusters = await _tagService.GetClusters();
			}

			var resource = new ItemsResource<TagClusterResourceItem>()
			{
				Items = clusters.Select(c => _mapper.Map<TagClusterResourceItem>(c))
			};

			var representation = resource
				.CreateRepresentation(this, _ => GetTags(q));

			return Ok(representation);
		}
	}
}
