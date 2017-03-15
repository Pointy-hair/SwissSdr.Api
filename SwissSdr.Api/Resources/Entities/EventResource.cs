using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
    public class EventResource : EntityResourceBase
    {
		public Multilingual<string> Name { get; set; } = new Multilingual<string>();
		public Multilingual<string> Description { get; set; } = new Multilingual<string>();
        public IEnumerable<string> Languages { get; set; } = new List<string>();
        public ICollection<string> Tags { get; set; } = new Collection<string>();

        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
		public ContactInfo ContactInfo { get; set; } = new ContactInfo();
		public Multilingual<Richtext> Content { get; set; } = new Multilingual<Richtext>();

		public static EventResource CreateTemplate()
		{
			var resource = new EventResource()
			{
				Begin = DateTime.UtcNow.Date,
				End = DateTime.UtcNow.Date
			};
			return resource;
		}
	}
}
