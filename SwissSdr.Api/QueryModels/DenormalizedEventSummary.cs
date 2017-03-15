using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Api.QueryModels
{
	public class DenormalizedEventSummary : IDenormalizedEntitySummary
	{
		public EntityType EntityType => EntityType.Event;

		public string Id { get; set; }
		public IDictionary<string, IEnumerable<ObjectPermission>> Permissions { get; set; }

		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
		public DateTime Begin { get; set; }
		public DateTime End { get; set; }
		public DenormalizedFileSummary Image { get; set; }
		public GeoCoordinate Coordinates { get; set; }

		public DenormalizedFileSummary GetImage() => Image;
	}
}
