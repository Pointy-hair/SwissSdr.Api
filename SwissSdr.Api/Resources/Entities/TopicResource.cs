using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
    public class TopicResource : EntityResourceBase
    {
		public TopicType Type { get; set; }
        public Multilingual<string> Name { get; set; } = new Multilingual<string>();
		public Multilingual<string> Description { get; set; } = new Multilingual<string>();
		public ICollection<string> Tags { get; set; } = new Collection<string>();

		public IEnumerable<string> UnSdgIds { get; set; } = Enumerable.Empty<string>();

		public Multilingual<Richtext> Content { get; set; } = new Multilingual<Richtext>();

    }
}
