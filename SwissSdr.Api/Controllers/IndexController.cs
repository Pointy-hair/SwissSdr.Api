using Halcyon.HAL;
using Halcyon.Web.HAL;
using Microsoft.AspNetCore.Mvc;
using SwissSdr.Api.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SwissSdr.Api.Controllers
{
	public class IndexController : ControllerBase
	{
		/// <summary>
		/// The API entrypoint
		/// </summary>
		/// <returns>A list of all available endpoints</returns>
		[Route("v1")]
		[HttpGet]
		public IActionResult Index_V1()
		{
			return this.HAL(new[] {
				this.CreateSelfLink(c => c.Index_V1()),

				Url.CreateLink<AppSettingsController>(ApiConstants.Rels.Settings, c => c.GetSettings()),
				Url.CreateLink<UsersController>(ApiConstants.Rels.CurrentUser, c => c.GetCurrentUser()),
				Url.CreateLink<UsersController>(ApiConstants.Rels.Users, c => c.GetUsers(null, null, null)),

				Url.CreateLink<EventsController>(ApiConstants.Rels.Events, c => c.GetEvents(null, null, null)),
				Url.CreateLink<FilesController>(ApiConstants.Rels.Files, c => c.GetFiles(null, null)),
				Url.CreateLink<OrganisationsController>(ApiConstants.Rels.Organisations, c => c.GetOrganisations(null, null, null)),
				Url.CreateLink<PeopleController>(ApiConstants.Rels.People, c => c.GetPersons(null, null, null)),
				Url.CreateLink<ProjectsController>(ApiConstants.Rels.Projects, c => c.GetProjects(null, null, null)),
				Url.CreateLink<TopicsController>(ApiConstants.Rels.Topics, c => c.GetTopics(null, null, null)),

				Url.CreateLink<SearchController>(ApiConstants.Rels.Search, c => c.Search(null, null, null)),
				Url.CreateLink<TagsController>(ApiConstants.Rels.Tags, c => c.GetTags(null))
			});
		}
		
		/// <summary>
		/// The API Root
		/// </summary>
		/// <returns>A redirect to the entrypoint of the newest API version</returns>
		[Route("")]
		[HttpGet]
		public IActionResult Index()
		{
			return this.RedirectToAction(_ => Index_V1());
		}
	}
}
