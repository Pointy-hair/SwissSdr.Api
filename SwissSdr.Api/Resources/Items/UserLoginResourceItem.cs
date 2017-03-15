using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
	public class UserLoginResourceItem
	{
		public string FriendlyName { get; set; }
		public string Provider { get; set; }
		public string UserId { get; set; }

		public string RemovalUrlTemplate { get; set; }
	}
}
