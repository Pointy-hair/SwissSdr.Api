using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Configuration
{
    public class GeocodingOptions
	{
		public const string Name = "Geocoding";

		public string GoogleApiKey { get; set; }
		public string MapboxApiKey { get; set; }
	}
}
