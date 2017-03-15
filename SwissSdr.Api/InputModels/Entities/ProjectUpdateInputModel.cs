
using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Values;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.InputModels
{
    public class ProjectUpdateInputModel
    {
        public ProjectType Type { get; set; }
        public Multilingual<string> Name { get; set; }
        public Multilingual<string> Description { get; set; }
        public Multilingual<string> LaySummary { get; set; }
        public Multilingual<string> Abstract { get; set; }
        public ICollection<string> Tags { get; set; }
		public IEnumerable<string> UnSdgIds { get; set; }
		public IEnumerable<string> UnTopicIds { get; set; }

		public string ParentProjectId { get; set; }
        public ICollection<string> PartnerProjectIds { get; set; }

        public ContactInfo ContactInfo { get; set; }
        public ProjectPhase Phase { get; set; }
        public DateTime? Begin { get; set; }
        public DateTime? End { get; set; }
		public ICollection<SnfDiscipline> Disciplines { get; set; }

		public Multilingual<string> FinancingDescription { get; set; }
        public InvestmentCategory? InvestmentCategory { get; set; }
        public Money? InvestmentAmount { get; set; }

        public ProjectContent Content { get; set; }
    }
}
