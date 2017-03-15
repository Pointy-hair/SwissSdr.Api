using Microsoft.AspNetCore.Mvc;
using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
    public abstract class EntityResourceBase : IResource
    {
		public string Id { get; set; }
		public IEnumerable<ObjectPermission> Permissions { get; set; }
	}
}
