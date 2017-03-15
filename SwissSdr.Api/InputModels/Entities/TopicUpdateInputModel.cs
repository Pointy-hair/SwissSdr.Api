using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.InputModels
{
    public class TopicUpdateInputModel
    {
		public TopicType Type { get; set; }
        public Multilingual<string> Name { get; set; }
        public Multilingual<string> Description { get; set; }
        public Multilingual<string> ShortDescription { get; set; }
        public IEnumerable<string> Tags { get; set; }
		public IEnumerable<string> UnSdgIds { get; set; }

        public Multilingual<Richtext> Content { get; set; }
    }
}
