using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Api.QueryModels
{
	public class DenormalizedTopicSummary : IDenormalizedEntitySummary
	{
		public EntityType EntityType => EntityType.Topic;

		public string Id { get; set; }
		public IDictionary<string, IEnumerable<ObjectPermission>> Permissions { get; set; }

		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
		public DenormalizedFileSummary Image { get; set; }

		public DenormalizedFileSummary GetImage() => Image;
	}
}
