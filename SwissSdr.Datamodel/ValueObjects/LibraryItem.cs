using System;
using System.Collections.ObjectModel;

namespace SwissSdr.Datamodel
{
	public class LibraryItem { 
		public string FileId { get; set; }

		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
		public Multilingual<string> Reference { get; set; }

		public string Url { get; set; }
	}
}
