using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SwissSdr.Api.Resources
{
    public class PersonSummaryResource : SummaryResourceBase
	{
		public string Title { get; set; }
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public ICollection<string> InterestAreas { get; set; }
		public IEnumerable<Multilingual<string>> AssociatedOrganisationNames { get; set; }
		public GeoCoordinate Coordinates { get; set; }
	}
}
