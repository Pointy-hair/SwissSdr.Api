using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
	public class AssociationResourceItem
	{
		public AssociationResourceItemType AssociationType { get; set; }
		public string AssociationDescription { get; set; }

		public EntityType SourceType { get; set; }
		public EntityType TargetType { get; set; }
		public string TargetId { get; set; }

		public object Target { get; set; }
	}

	public enum AssociationResourceItemType
	{
		Entity,
		Stub
	}
}
