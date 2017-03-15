using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Api.QueryModels
{
	public class DenormalizedPersonSummary : IDenormalizedEntitySummary
	{
		public EntityType EntityType => EntityType.Person;

		public string Id { get; set; }
		public IDictionary<string, IEnumerable<ObjectPermission>> Permissions { get; set; }

		public string Title { get; set; }
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public ICollection<string> InterestAreas { get; set; }
		public DenormalizedFileSummary ProfileImage { get; set; }
		public IEnumerable<Multilingual<string>> AssociatedOrganisationNames { get; set; }
		public GeoCoordinate Coordinates { get; set; }

		public DenormalizedFileSummary GetImage() => ProfileImage;
	}
}
