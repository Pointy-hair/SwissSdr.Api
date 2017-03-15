using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
    public class OrganisationSummaryResource : SummaryResourceBase
	{
        public Multilingual<string> Name { get; set; }
        public Multilingual<string> Description { get; set; }
		public GeoCoordinate Coordinates { get; set; }
	}
}
