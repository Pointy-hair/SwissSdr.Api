using System;

namespace SwissSdr.Datamodel
{
	public class Association
	{
		/// <summary>
		/// The description of this association
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// The reference that representes the target entity.
		/// </summary>
		public string TargetId { get; set; }

		public EntityStub TargetStub { get; set; }

		// serialization constructor
		protected Association() { }

		public Association(string targetId, string description)
		{
			TargetId = targetId;
			Description = description;
		}
		public Association(EntityStub stub, string description)
		{
			TargetId = null;
			Description = description;
			TargetStub = stub;
		}

		public static Association CreateStub(EntityType type, Multilingual<string> name, Multilingual<string> description, string url, string associationDescription)
		{
			var stub = new EntityStub()
			{
				Type = type,
				Name = name,
				Description = description,
				Url = url
			};

			return new Association(stub, associationDescription);
		}
	}
}
