using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
    public class RavenClaim
	{
		public string Issuer { get; set; }
		public string Type { get; set; }
		public string Value { get; set; }
	}
}
