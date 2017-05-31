using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public static class UserLoginExtensions
    {
		public static string GetFriendlyName(this UserLogin login, AppSettings settings)
		{
			if (login == null)
			{
				throw new ArgumentException(nameof(login));
			}
			if (settings == null)
			{
				throw new ArgumentException(nameof(settings));
			}

			var matchingProvider = settings.LoginSettings.Providers.SingleOrDefault(p => p.Provider == login.Provider || p.Provider == login.AuthenticateVia);
			if (matchingProvider == null)
			{
				throw new InvalidOperationException($"Could not find login provider '{login.Provider}' in AppSettings.");
			}

			var friendlyName = matchingProvider.FriendlyName;
			if (!string.IsNullOrEmpty(login.Email))
			{
				friendlyName += $" ({login.Email})";
			}

			return friendlyName;
		}
	}
}
