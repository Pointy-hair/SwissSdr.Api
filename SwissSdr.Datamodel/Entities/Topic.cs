using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public class Topic : EntityBase, IHasImages
	{
		public TopicType Type { get; set; }
		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
		public ICollection<string> Tags { get; set; } = new Collection<string>();
		public ICollection<string> UnSdgIds { get; set; } = new Collection<string>();

		public Multilingual<Richtext> Content { get; set; }

		public IList<string> ImageIds { get; set; } = new Collection<string>();
	}

	public enum TopicType
	{
		Normal,
		UnSdg,
		UnTopic
	}
}
