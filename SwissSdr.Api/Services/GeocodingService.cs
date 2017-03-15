using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SwissSdr.Datamodel;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SwissSdr.Api.Services
{
	public class GeocodingService
	{
		private static HttpClient _httpClient;

		private readonly string _apiKey;
		private readonly ILogger _logger;

		static GeocodingService()
		{
			_httpClient = new HttpClient()
			{
				BaseAddress = new Uri("https://api.mapbox.com")
			};
		}

		public GeocodingService(string apiKey, ILogger logger)
		{
			_apiKey = apiKey;
			_logger = logger;
		}

		public async Task Geocode(ContactInfo contactInfo)
		{
			if (contactInfo?.Addresses?.Any() == true)
			{
				foreach (var address in contactInfo.Addresses)
				{
					await Geocode(address).ConfigureAwait(false);
				}
			}
		}

		public async Task Geocode(ContactInfo.Address address)
		{
			if (address == null)
			{
				return;
			}

			if (address.AddressLines?.Any() == false
				|| address.AddressLines?.All(s => string.IsNullOrEmpty(s)) == true
				|| string.IsNullOrEmpty(address.Locality))
			{
				throw new ArgumentException("Address must contain at least one AddressLine and the Locality.", nameof(address));
			}

			var addressString = $"{string.Join(" ", address.AddressLines)}, {address.PostalCode} {address.Locality}, {address.Country}";
			var requestUrl = $"/geocoding/v5/mapbox.places/{Uri.EscapeDataString(addressString)}.json?autocomplete=false&access_token={_apiKey}";

			var response = await _httpClient.GetAsync(requestUrl).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogWarning($"Could not geocode '{address.ToString()}': Error {response.StatusCode} from mapbox api.");
				return;
			}

			var data = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
			if (!data["features"].Any())
			{
				_logger.LogWarning($"Could not geocode '{address.ToString()}': no features found.");
				return;
			}

			var feature = data["features"][0];
			double confidence = 0;
			if (!double.TryParse(feature["relevance"].ToString(), out confidence) || confidence < 0.3)
			{
				_logger.LogWarning($"Could not geocode '{address.ToString()}': low confidence of {confidence}.");
				return;
			}

			var featureCoordinates = feature["geometry"]["coordinates"];
			address.Coordinates = new GeoCoordinate(double.Parse(featureCoordinates[1].ToString()), double.Parse(featureCoordinates[0].ToString()));

			var postCode = feature["context"].SingleOrDefault(x => x["id"].ToString().StartsWith("postcode."))?["text"]?.ToString();
			if (!string.IsNullOrEmpty(postCode))
			{
				address.PostalCode = postCode;
			}

			var region = feature["context"].SingleOrDefault(x => x["id"].ToString().StartsWith("region."))?["text"]?.ToString();
			if (!string.IsNullOrEmpty(region))
			{
				address.Region = region;
			}

			address.Country = feature["context"].Single(x => x["id"].ToString().StartsWith("country."))["short_code"].ToString();
		}
	}
}
