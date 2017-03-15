using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
	public class UserPermissionResourceItem
	{
		public string EntityId { get; set; }
		public EntityType Type { get; set; }
		public IEnumerable<ObjectPermission> Permissions { get; set; }
	}
}
