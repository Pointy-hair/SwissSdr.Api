using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
	public class SearchResultResource : IResource
	{
		public EntityType Type { get; set; }

		public string DisplayName { get; set; }
		public string DisplayImageUrl { get; set; }

	}
}
