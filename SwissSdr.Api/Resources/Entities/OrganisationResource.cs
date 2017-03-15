using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwissSdr.Datamodel;
using System.Collections.ObjectModel;
using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;

namespace SwissSdr.Api.Resources
{
    public class OrganisationResource : EntityResourceBase
    {
		public Multilingual<string> Name { get; set; } = new Multilingual<string>();
        public Multilingual<string> Description { get; set; } = new Multilingual<string>();
		public Multilingual<string> HierarchyInformation { get; set; } = new Multilingual<string>();
		public string RootOrganisationId { get; set; }
		public OrganisationType Type { get; set; }
		public ICollection<SnfDiscipline> Disciplines { get; set; }

		public IEnumerable<char> IsicClassification { get; set; } = new Collection<char>();
        public string ProfileImageId { get; set; }
		public ContactInfo ContactInfo { get; set; } = new ContactInfo();
        public Multilingual<Richtext> Profile { get; set; } = new Multilingual<Richtext>();
		public IEnumerable<string> Tags { get; set; } = new Collection<string>();
    }
}
