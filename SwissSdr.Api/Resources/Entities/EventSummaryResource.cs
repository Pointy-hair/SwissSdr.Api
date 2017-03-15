using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
    public class EventSummaryResource : SummaryResourceBase
    {
        public Multilingual<string> Name { get; set; } = new Multilingual<string>();
        public Multilingual<string> Description { get; set; } = new Multilingual<string>();
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
		public GeoCoordinate Coordinates { get; set; }
	}
}
