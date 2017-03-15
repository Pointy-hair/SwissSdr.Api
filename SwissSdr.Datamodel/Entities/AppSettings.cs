using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel.Settings
{
	public class AppSettings
	{
		public const string AppSettingsId = "settings/appsettings";

		public string Id => AppSettingsId;

		public ICollection<string> Languages { get; set; }
		public ICollection<AssociationDescriptionDefinition> AssociationDescriptionDefinitions { get; set; }

		public ICollection<SdgTopicAssociation> SdgTopicMappings { get; set; } = new Collection<SdgTopicAssociation>();

		public ICollection<SnfDisciplineGroup> DisciplineDefinitions { get; set; }

		public LoginProviderSettings LoginSettings { get; set; } = new LoginProviderSettings();
	}
}
