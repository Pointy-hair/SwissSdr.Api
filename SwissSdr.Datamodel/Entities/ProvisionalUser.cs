using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
    public class ProvisionalUser
    {
		public string Id => CreateId(Login.Provider, Login.UserId);
		public UserLogin Login { get; set; }
		public ICollection<RavenClaim> Claims { get; set; } = new Collection<RavenClaim>();

		public static string CreateId(string provider, string userId) => $"ProvisionalUsers/{provider}/{userId}";
	}
}
