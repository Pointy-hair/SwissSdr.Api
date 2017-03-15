using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
	public class PagedCollectionResource : IResource
	{
		public int Skip { get; set; }
		public int Take { get; set; }
		public int TotalResults { get; set; }
	}
}
