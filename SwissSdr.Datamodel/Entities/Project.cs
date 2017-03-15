using SwissSdr.Datamodel.Values;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public class Project : EntityBase, IHasImages, IHasLibrary, IHasPublications, IHasAssociations, IHasJobs
	{
		public ProjectType Type { get; set; }
		public Multilingual<string> Name { get; set; } = new Multilingual<string>();
		public Multilingual<string> Description { get; set; } = new Multilingual<string>();
		public Multilingual<string> LaySummary { get; set; } = new Multilingual<string>();
		public Multilingual<string> Abstract { get; set; } = new Multilingual<string>();
		public ICollection<string> Tags { get; set; } = new Collection<string>();
		public ICollection<string> UnSdgIds { get; set; } = new Collection<string>();
		public ICollection<string> UnTopicIds { get; set; } = new Collection<string>();
		public IList<string> ImageIds { get; set; } = new List<string>();

		public string ParentProjectId { get; set; }
		public ICollection<string> PartnerProjectIds { get; set; } = new Collection<string>();

		public ContactInfo ContactInfo { get; set; } = new ContactInfo();
		public ProjectPhase Phase { get; set; }
		public DateTime? Begin { get; set; }
		public DateTime? End { get; set; }
		public ICollection<SnfDiscipline> Disciplines { get; set; }

		public Multilingual<string> FinancingDescription { get; set; } = new Multilingual<string>();
		public InvestmentCategory? InvestmentCategory { get; set; }
		public Money? InvestmentAmount { get; set; }

		public ProjectContent Content { get; set; } = new ProjectContent();

		public ICollection<JobAdvertisement> Jobs { get; set; } = new Collection<JobAdvertisement>();

		public IList<LibraryItem> Library { get; set; } = new List<LibraryItem>();
		public IList<LibraryItem> Publications { get; set; } = new List<LibraryItem>();

		public ICollection<Association> Associations { get; set; } = new Collection<Association>();
	}

	public enum ProjectPhase
	{
		Planned,
		Active,
		Finished
	}

	public enum ProjectType
	{
		Project,
		Programme
	}

	public enum InvestmentCategory
	{
		Under100K,
		Between100KAnd500K,
		Between500KAnd1MM,
		Between1MMAnd5MM,
		Over5MM
	}
}
