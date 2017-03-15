using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public class Organisation : EntityBase, IHasLibrary, IHasImages, IHasAssociations, IHasJobs
	{
		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
		public Multilingual<string> HierarchyInformation { get; set; }
		public string RootOrganisationId { get; set; }
		public OrganisationType Type { get; set; }
		/// <summary>
		/// Indicates the economic sectors in which this organisation is active per ISIC Rev. 4 or EU NACE Rev. 2
		/// </summary>
		public ICollection<char> IsicClassification { get; set; } = new Collection<char>();
		public string ProfileImageId { get; set; }
		public IList<string> ImageIds { get; set; } = new List<string>();
		public ContactInfo ContactInfo { get; set; } = new ContactInfo();
		public Multilingual<Richtext> Profile { get; set; }
		public ICollection<string> Tags { get; set; } = new Collection<string>();
		public ICollection<SnfDiscipline> Disciplines { get; set; }

		public ICollection<JobAdvertisement> Jobs { get; set; } = new Collection<JobAdvertisement>();

		public IList<LibraryItem> Library { get; set; } = new List<LibraryItem>();

		public ICollection<Association> Associations { get; set; } = new Collection<Association>();
	}

	public enum OrganisationType
	{
		University,
		Faculty,
		ResearchTeam,
		IndependentResearchInstitute,
		Company,
		Association,
		Foundation,
		PublicSector,
		NGO
	}
}
