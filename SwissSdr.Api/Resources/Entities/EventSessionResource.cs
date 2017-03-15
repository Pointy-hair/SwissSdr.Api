using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
    public class EventSessionResource : EntityResourceBase
	{
        public Multilingual<string> Name { get; set; }
        public Multilingual<Richtext> Content { get; set; }

        public string Venue { get; set; }

        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
    }
}
