using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System.Collections.Generic;

namespace SwissSdr.Api.QueryModels
{
	public class DenormalizedOrganisationSummary : IDenormalizedEntitySummary
	{
		public EntityType EntityType => EntityType.Organisation;

		public string Id { get; set; }
		public IDictionary<string, IEnumerable<ObjectPermission>> Permissions { get; set; }

		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
		public DenormalizedFileSummary ProfileImage { get; set; }
		public GeoCoordinate Coordinates { get; set; }

		public DenormalizedFileSummary GetImage() => ProfileImage;
	}
}