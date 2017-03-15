using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwissSdr.Datamodel.Authorization;

namespace SwissSdr.Api.QueryModels
{
	public class DenormalizedFileSummary : IDenormalizedEntitySummary
	{
		public EntityType EntityType => EntityType.File;

		public string Id { get; set; }
		public IDictionary<string, IEnumerable<ObjectPermission>> Permissions { get; set; }

		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }

		public string Url { get; set; }

		public DenormalizedFileSummary GetImage() => null;
	}
}
