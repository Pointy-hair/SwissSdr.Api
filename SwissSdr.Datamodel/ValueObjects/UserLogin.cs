using SwissSdr.Datamodel.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public class UserLogin
	{
		public string Provider { get; set; }
		public string UserId { get; set; }
		public string AuthenticateVia { get; set; }

		public string Email { get; set; }

		public string GetRemovalUrl(string authority) => $"{authority}/Account/RemoveExternalLogin?provider={Uri.EscapeDataString(Provider)}&userId={Uri.EscapeDataString(UserId)}&returnUrl={{returnUrl}}";
	}
}
