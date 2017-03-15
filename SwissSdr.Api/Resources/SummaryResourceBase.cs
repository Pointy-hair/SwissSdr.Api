using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
    public abstract class SummaryResourceBase : IResource
    {
		public string Id { get; set; }
		public string ThumbnailUrl { get; set; }
		public IEnumerable<ObjectPermission> Permissions { get; set; }
	}
}
