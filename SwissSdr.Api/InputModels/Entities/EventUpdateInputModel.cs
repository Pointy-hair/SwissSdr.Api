using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.InputModels
{
    public class EventUpdateInputModel
    {
		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
		public IList<string> Languages { get; set; }
		public ICollection<string> Tags { get; set; }

		public DateTime Begin { get; set; }
		public DateTime End { get; set; }

		public ContactInfo ContactInfo { get; set; }
		public Multilingual<Richtext> Content { get; set; }
	}
}
