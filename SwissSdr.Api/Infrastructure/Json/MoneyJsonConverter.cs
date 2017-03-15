using Newtonsoft.Json;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Infrastructure
{
	public class MoneyJsonConverter : JsonConverter
	{
		private const string AmountPropertyName = "Amount";
		private const string CurrencyCodePropertyName = "CurrencyCode";

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Money) || objectType == typeof(Money?);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}

			decimal amount = 0;
			string currencyCode = null;

			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.EndObject)
				{
					return new Money(amount, currencyCode);
				}

				if (reader.TokenType == JsonToken.PropertyName)
				{
					var propertyName = (string)reader.Value;
					if (propertyName.Equals(AmountPropertyName, StringComparison.OrdinalIgnoreCase))
					{
						var jsonAmount = reader.ReadAsDouble();
						if (jsonAmount.HasValue)
						{
							amount = (decimal)jsonAmount.Value;
						}
					}
					else if (propertyName.Equals(CurrencyCodePropertyName, StringComparison.OrdinalIgnoreCase))
					{
						currencyCode = reader.ReadAsString();
					}
				}
			}

			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var money = (Money?)value;

			if (money.HasValue)
			{
				writer.WriteStartObject();

				writer.WritePropertyName(AmountPropertyName);
				writer.WriteValue(money.Value.Amount);

				writer.WritePropertyName(CurrencyCodePropertyName);
				writer.WriteValue(money.Value.CurrencyCode);

				writer.WriteEndObject();
			}
			else
			{
				writer.WriteNull();
			}
		}
	}
}
