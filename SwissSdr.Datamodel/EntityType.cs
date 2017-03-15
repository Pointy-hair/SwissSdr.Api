using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public static class EntityTypeNames
	{
		public const string Topic = "topic";
		public const string Project = "project";
		public const string Person = "person";
		public const string Organisation = "organisation";
		public const string Event = "event";
		public const string File = "file";

		public static EntityType? Parse(string value)
		{
			if (Enum.TryParse(value, true, out EntityType result))
			{
				return new EntityType?(result);
			}

			return null;
		}
	}

	public enum EntityType
	{
		Topic,
		Project,
		Person,
		Organisation,
		Event,
		File
	}

	public static class EntityTypeExtensions {
		private static Dictionary<Type, EntityType> _typeMappings = new Dictionary<Type, EntityType>()
		{
			{ typeof(Project), EntityType.Project },
			{ typeof(Person), EntityType.Person},
			{ typeof(Organisation), EntityType.Organisation},
			{ typeof(Event), EntityType.Event},
			{ typeof(Topic), EntityType.Topic},
			{ typeof(File), EntityType.File }
		};

		public static EntityType GetEntityType(this EntityBase entity)
		{
			return GetEntityType(entity.GetType());
		}

		public static EntityType GetEntityType(Type type)
		{
			if (_typeMappings.TryGetValue(type, out var entityType))
			{
				return entityType;
			}
			throw new ArgumentException("Must be an entity type", nameof(type));
		}
	}
}
