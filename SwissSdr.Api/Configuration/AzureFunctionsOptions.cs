using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Configuration
{
    public class AzureFunctionsOptions
    {
		public const string Name = "AzureFunctions";

		public string ApiKey { get; set; }
		public string Endpoint { get; set; }
	}
}
