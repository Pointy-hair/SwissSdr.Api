using Microsoft.AspNetCore.Authorization;
using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Authorization
{
	public class PermissionAuthorizationRequirement : IAuthorizationRequirement
	{
		public IEnumerable<ObjectPermission> RequiredPermissions { get; }

		public PermissionAuthorizationRequirement(params ObjectPermission[] requiredPermissions)
		{
			RequiredPermissions = requiredPermissions;
		}
	}
}
