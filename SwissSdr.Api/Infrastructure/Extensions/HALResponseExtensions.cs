using Halcyon.HAL;
using Microsoft.AspNetCore.Mvc;
using SwissSdr.Api.Controllers;
using SwissSdr.Api.Endpoints;
using SwissSdr.Api.QueryModels;
using SwissSdr.Api.Resources;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
	public static class HALResponseExtensions
	{
		public static HALResponse AddSelfLink<TController>(this HALResponse representation, TController controller, Expression<Action<TController>> action)
			where TController : ControllerBase
		{
			representation.AddLinks(controller.CreateSelfLink(action));
			return representation;
		}
		public static HALResponse AddSelfLink<TController>(this HALResponse representation, TController controller, Expression<Func<TController, Task>> action)
			where TController : ControllerBase
		{
			representation.AddLinks(controller.CreateSelfLink(action));
			return representation;
		}

		public static HALResponse AddProfileImage(this HALResponse representation, ResourceFactory resourceFactory, DenormalizedFileSummary file)
		{
			if (file != null)
			{
				representation.AddEmbeddedResource(ApiConstants.Embedded.ProfileImage, resourceFactory.CreateSummaryResource(file));
			}
			return representation;
		}

		public static HALResponse AddParentProject(this HALResponse representation, ResourceFactory resourceFactory, DenormalizedProjectSummary parentProject)
		{
			if (parentProject != null)
			{
				representation.AddEmbeddedResource(ApiConstants.Embedded.ParentProject, resourceFactory.CreateSummaryResource(parentProject));
			}
			return representation;
		}

		public static HALResponse AddPartnerProjects(this HALResponse representation, ResourceFactory resourceFactory, IEnumerable<DenormalizedProjectSummary> partnerProjects)
		{
			if (partnerProjects.Any())
			{
				var itemRepresentations = partnerProjects.Select(p => resourceFactory.CreateSummaryResource(p));
				representation.AddEmbeddedCollection(ApiConstants.Embedded.PartnerProjects, itemRepresentations);
			}
			return representation;
		}

		public static HALResponse AddImages<TController>(this HALResponse representation, TController controller, IHasDenormalizedImages model, int id)
			where TController : ControllerBase, IHasImagesEndpoint
		{
			if (model.Entity.ImageIds.Any())
			{
				var images = new HALResponse(controller.ImagesEndpoint.CreateImageResource(model))
					.AddLinks(controller.CreateSelfLink(_ => _.GetImages(id)));
				representation.AddEmbeddedResource(ApiConstants.Rels.Images, images);
			}

			representation.AddLinks(controller.CreateLink(ApiConstants.Rels.Images, _ => _.GetImages(id)));

			return representation;
		}

		public static HALResponse AddAssociations<TController>(this HALResponse representation, TController controller, IHasDenormalizedAssociations model, int id)
			where TController : ControllerBase, IHasAssociationsEndpoint
		{
			if (model.Entity.Associations.Any())
			{
				var sourceType = EntityTypeControllerExtensions.GetEntityTypeFromControllerType(typeof(TController));

				var associations = new HALResponse(controller.AssociationsEndpoint.CreateAssociationResource(model, sourceType))
					.AddLinks(controller.CreateSelfLink(_ => _.GetAssociations(id)));
				representation.AddEmbeddedResource(ApiConstants.Rels.Associations, associations);
			}

			representation.AddLinks(controller.CreateLink(ApiConstants.Rels.Associations, _ => _.GetAssociations(id)));

			return representation;
		}

		public static HALResponse AddLibrary<TController>(this HALResponse representation, TController controller, IHasDenormalizedLibrary model, int id)
			where TController : ControllerBase, IHasLibraryEndpoint
		{
			if (model.Entity.Library.Any())
			{
				var library = new HALResponse(controller.LibraryEndpoint.CreateLibraryResource(model))
					.AddLinks(controller.CreateSelfLink(_ => _.GetLibrary(id)));
				representation.AddEmbeddedResource(ApiConstants.Rels.Library, library);
			}

			representation.AddLinks(controller.CreateLink(ApiConstants.Rels.Library, _ => _.GetLibrary(id)));

			return representation;
		}

		public static HALResponse AddPublications<TController>(this HALResponse representation, TController controller, IHasDenormalizedPublications model, int id)
			where TController : ControllerBase, IHasPublicationsEndpoint
		{
			if (model.Entity.Publications.Any())
			{
				var library = new HALResponse(controller.PublicationsEndpoint.CreatePublicationsResource(model))
					.AddLinks(controller.CreateSelfLink(_ => _.GetPublications(id)));
				representation.AddEmbeddedResource(ApiConstants.Rels.Publications, library);
			}

			representation.AddLinks(controller.CreateLink(ApiConstants.Rels.Publications, _ => _.GetPublications(id)));

			return representation;
		}

		public static HALResponse AddEventSessions(this HALResponse representation, EventsController controller, IEnumerable<EventSessionResource> sessionResources, int eventId)
		{
			if (sessionResources.Any())
			{
				var sessionRepresentations = sessionResources.Select(x => x
					.CreateRepresentation()
					.AddLinks(controller.Url.CreateSelfLink<EventSessionsController>(c => c.GetEventSession(eventId, RavenExtensions.GetIdValuePart(null, x.Id))))
				);

				// wrap with dummy intermediary to match the other embeds for api consistency
				// it would be preferrable to change the others, but that is not viable right 
				// now because of refactoring concerns in the front end code
				var dummyWrapper = new ItemsResource<HALResponse>()
					{
						Items = sessionRepresentations
					}
					.CreateRepresentation()
					.AddLinks(controller.Url.CreateSelfLink<EventSessionsController>(c => c.GetEventSessions(eventId)));

				representation.AddEmbeddedResource(ApiConstants.Rels.Sessions, dummyWrapper);
			}

			representation.AddLinks(controller.Url.CreateLink<EventSessionsController>(ApiConstants.Rels.Sessions, c => c.GetEventSessions(eventId)));

			return representation;
		}

		public static HALResponse AddJobs<TController>(this HALResponse representation, TController controller, IHasJobs model, int id)
			where TController : ControllerBase, IHasJobsEndpoint
		{
			if (model.Jobs.Any())
			{
				var jobs = new HALResponse(controller.JobsEndpoint.CreateJobsResource(model))
					.AddLinks(controller.CreateSelfLink(_ => _.GetJobs(id)));
				representation.AddEmbeddedResource(ApiConstants.Rels.Jobs, jobs);
			}

			representation.AddLinks(controller.CreateLink(ApiConstants.Rels.Jobs, _ => _.GetJobs(id)));

			return representation;
		}
	}
}
