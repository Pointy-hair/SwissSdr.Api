using SwissSdr.Api.Controllers;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
	public static class EntityTypeControllerExtensions
	{
		private static Dictionary<Type, EntityType> _controllerMappings = new Dictionary<Type, EntityType>()
		{
			{ typeof(PeopleController), EntityType.Person},
			{ typeof(ProjectsController), EntityType.Project},
			{ typeof(OrganisationsController), EntityType.Organisation},
			{ typeof(EventsController), EntityType.Event},
			{ typeof(TopicsController), EntityType.Topic},
			{ typeof(FilesController), EntityType.File}
		};

		public static EntityType GetEntityTypeFromControllerType(Type controllerType)
		{
			if (_controllerMappings.TryGetValue(controllerType, out var entityType))
			{
				return entityType;
			}
			throw new ArgumentException("Must be an entity controller type", nameof(controllerType));
		}
	}
}
