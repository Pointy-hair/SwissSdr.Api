using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
    public class AppSettingsResource : IResource
    {
		public IEnumerable<string> Languages { get; set; }
		public IEnumerable<AssociationDescriptionDefinition> AssociationDescriptionDefinitions { get; set; }

		public IDictionary<string, EntityResourceBase> EntityTemplates { get; set; }

		public IEnumerable<SnfDisciplineGroup> DisciplineDefinitions { get; set; }

		public LoginProviderSettings LoginSettings { get; set; }
	}
}
