using Newtonsoft.Json;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Api.Infrastructure
{
	public class GeoCoordinateJsonConverter : JsonConverter
	{
		private const string LatitudePropertyName = "Latitude";
		private const string LongitudePropertyName = "Longitude";

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(GeoCoordinate);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}

			double latitude = 0;
			double longitude = 0;

			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.EndObject)
				{
					return new GeoCoordinate(latitude, longitude);
				}

				if (reader.TokenType == JsonToken.PropertyName)
				{
					var propertyName = (string)reader.Value;
					if (propertyName.Equals(LatitudePropertyName, StringComparison.OrdinalIgnoreCase))
					{
						var jsonLatitude = reader.ReadAsDouble();
						if (jsonLatitude.HasValue)
						{
							latitude = jsonLatitude.Value;
						}
					}
					else if (propertyName.Equals(LongitudePropertyName, StringComparison.OrdinalIgnoreCase))
					{
						var jsonLongitude = reader.ReadAsDouble();
						if (jsonLongitude.HasValue)
						{
							longitude = jsonLongitude.Value;
						}
					}
				}
			}

			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var coordinate = (GeoCoordinate?)value;

			if (coordinate.HasValue)
			{
				writer.WriteStartObject();

				writer.WritePropertyName(LatitudePropertyName);
				writer.WriteValue(coordinate.Value.Latitude);

				writer.WritePropertyName(LongitudePropertyName);
				writer.WriteValue(coordinate.Value.Longitude);

				writer.WriteEndObject();
			}
			else
			{
				writer.WriteNull();
			}
		}
	}
}
