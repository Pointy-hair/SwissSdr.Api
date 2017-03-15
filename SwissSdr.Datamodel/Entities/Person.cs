using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SwissSdr.Datamodel
{
	/// <summary>
	/// Represents a natural person
	/// </summary>
	public class Person : EntityBase, IHasLibrary, IHasPublications, IHasAssociations
	{
		public string Salutation { get; set; }
		public string Title { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string ProfileImageId { get; set; }
        public Multilingual<Richtext> Profile { get; set; } = new Multilingual<Richtext>();

        public IList<string> Languages { get; set; } = new List<string>();
		public ICollection<string> InterestAreas { get; set; } = new Collection<string>();
		public ContactInfo ContactInfo { get; set; } = new ContactInfo();

		public IList<LibraryItem> Library { get; set; } = new List<LibraryItem>();
		public IList<LibraryItem> Publications { get; set; } = new List<LibraryItem>();

		public ICollection<Association> Associations { get; set; } = new Collection<Association>();
	}
}
