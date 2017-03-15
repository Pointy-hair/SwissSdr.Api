using Raven.Client.Indexes;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Indexes
{
    public class TagCluster_Tag : AbstractIndexCreationTask<TagCluster, TagCluster_Tag.Result>
    {
		public class Result
		{
			public string LanguageCode { get; set; }
			public string Tag { get; set; }
		}

		public TagCluster_Tag()
		{
			Map = clusters => from cluster in clusters
							  from localized in cluster.Tags
							  select new Result
							  {
								  LanguageCode = localized.LanguageCode,
								  Tag = localized.Value
							  };
		}
    }
}
