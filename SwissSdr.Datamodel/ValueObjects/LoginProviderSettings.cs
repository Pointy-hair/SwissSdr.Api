using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SwissSdr.Datamodel.Settings
{
	public class LoginProviderSettings
	{
		public string AddLoginUrlTemplate { get; set; }
		public ICollection<LoginProvider> Providers { get; set; } = new Collection<LoginProvider>();
	}
}
