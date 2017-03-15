using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using SwissSdr.Shared;

namespace SwissSdr.Api.Resources
{
    public class FileResource : EntityResourceBase
	{
		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }

		public string Extension { get; set; }
		public string MimeType { get; set; }
		public double Size { get; set; }
		public string Url { get; set; }
		public Dictionary<ImageSize, string> Urls { get; set; }

		public ICollection<object> Metadata { get; } = new List<object>();
	}
}
