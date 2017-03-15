using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;
using Microsoft.Extensions.Logging;

namespace SwissSdr.Api
{
	public class MultilingualJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			if (objectType.GetTypeInfo().IsGenericType)
			{
				return objectType.GetGenericTypeDefinition() == typeof(Multilingual<>);
			}

			return false;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}

			var multilingual = Activator.CreateInstance(objectType);
			var multilingualAddMethod = objectType.GetMethod(nameof(Multilingual<object>.Add), new Type[] { objectType.GetGenericArguments()[0], typeof(string) });

			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.EndObject)
				{
					return multilingual;
				}

				if (reader.TokenType == JsonToken.PropertyName)
				{
					var languageCode = reader.Value;
					reader.Read();
					var value = serializer.Deserialize(reader, objectType.GetGenericArguments()[0]);

					multilingualAddMethod.Invoke(multilingual, new object[] { value, languageCode });
				}
			}

			return multilingual;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var localizedValueType = typeof(LocalizedValue<>).MakeGenericType(value.GetType().GetGenericArguments());

			var localizedValueLanguageCodeProperty = localizedValueType.GetProperty(nameof(LocalizedValue<object>.LanguageCode));
			var localizedValueValueProperty = localizedValueType.GetProperty(nameof(LocalizedValue<object>.Value));

			writer.WriteStartObject();
			foreach (var item in (IEnumerable)value)
			{
				var itemLanguageCode = localizedValueLanguageCodeProperty.GetValue(item)?.ToString();
				var itemValue = localizedValueValueProperty.GetValue(item);

				writer.WritePropertyName(itemLanguageCode);
				serializer.Serialize(writer, itemValue);
			}
			writer.WriteEndObject();
		}
	}
}
