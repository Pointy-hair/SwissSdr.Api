using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public class Event : EntityBase, IHasImages, IHasLibrary, IHasAssociations
	{
		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
		public IList<string> Languages { get; set; } = new List<string>();
		public ICollection<string> Tags { get; set; } = new Collection<string>();

		public IList<string> ImageIds { get; set; } = new List<string>();
		public DateTime Begin { get; set; }
        public DateTime End { get; set; }
		public ContactInfo ContactInfo { get; set; }
		public Multilingual<Richtext> Content { get; set; }

		public IList<LibraryItem> Library { get; set; } = new List<LibraryItem>();

		public ICollection<Association> Associations { get; set; } = new Collection<Association>();
	}
}
