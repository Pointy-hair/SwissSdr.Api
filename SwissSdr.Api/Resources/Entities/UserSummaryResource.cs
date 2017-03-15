using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
    public class UserSummaryResource : IResource
	{
		public string Id { get; set; }
		public string Title { get; set; }
		public string Fullname { get; set; }
		public string EMail { get; set; }
		public string ThumbnailUrl { get; set; }
	}
}
