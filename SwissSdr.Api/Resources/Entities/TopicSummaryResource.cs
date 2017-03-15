using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
    public class TopicSummaryResource : SummaryResourceBase
	{
        public Multilingual<string> Name { get; set; }
        public Multilingual<string> Description { get; set; }
    }
}
