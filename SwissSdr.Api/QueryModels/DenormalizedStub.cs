using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwissSdr.Datamodel.Authorization;

namespace SwissSdr.Api.QueryModels
{
	public class DenormalizedStub : IDenormalizedEntity, IDenormalizedEntitySummary
	{
		public EntityType EntityType => Type;

		public string Id { get; set; }
		public IDictionary<string, IEnumerable<ObjectPermission>> Permissions { get; set; }

		public EntityType Type { get; set; }
		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
		public string Url { get; set; }

		public DenormalizedFileSummary GetImage() => null;
	}
}
