using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel.Values;

namespace SwissSdr.Api.Resources
{
    public class ProjectResource : EntityResourceBase
    {
		public ProjectType Type { get; set; }
        public Multilingual<string> Name { get; set; } = new Multilingual<string>();
        public Multilingual<string> Description { get; set; } = new Multilingual<string>();
        public Multilingual<string> LaySummary { get; set; } = new Multilingual<string>();
        public Multilingual<string> Abstract { get; set; } = new Multilingual<string>();
        public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

		public IEnumerable<string> UnSdgIds { get; set; } = Enumerable.Empty<string>();
		public IEnumerable<string> UnTopicIds { get; set; } = Enumerable.Empty<string>();

        public string ParentProjectId { get; set; }
		public IEnumerable<string> PartnerProjectIds { get; set; } = Enumerable.Empty<string>();

		public ContactInfo ContactInfo { get; set; } = new ContactInfo();
        public ProjectPhase Phase { get; set; }
        public DateTime? Begin { get; set; }
        public DateTime? End { get; set; }
		public ICollection<SnfDiscipline> Disciplines { get; set; }

		public Multilingual<string> FinancingDescription { get; set; } = new Multilingual<string>();
        public InvestmentCategory? InvestmentCategory { get; set; }
        public Money? InvestmentAmount { get; set; }

		public ProjectContent Content { get; set; } = new ProjectContent();
	}
}
