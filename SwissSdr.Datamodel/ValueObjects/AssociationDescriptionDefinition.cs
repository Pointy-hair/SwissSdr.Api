using System;
using System.Collections.Generic;

namespace SwissSdr.Datamodel.Settings
{
	public class AssociationDescriptionDefinition
	{
		public string Name { get; set; }
		public ICollection<EntityAssociationPair> AllowedEntityAssociations { get; set; }
		public Multilingual<string> DisplayName { get; set; }
	}
}
