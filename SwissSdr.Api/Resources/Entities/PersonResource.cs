using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;

namespace SwissSdr.Api.Resources
{
    public class PersonResource : EntityResourceBase
    {
		public string Salutation { get; set; }
		public string Title { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
		public string ProfileImageId { get; set; }
		public Multilingual<Richtext> Profile { get; set; } = new Multilingual<Richtext>();

		public IEnumerable<string> Languages { get; set; } = new List<string>();
		public ICollection<string> InterestAreas { get; set; } = new Collection<string>();
		public ContactInfo ContactInfo { get; set; } = new ContactInfo();
	}
}
