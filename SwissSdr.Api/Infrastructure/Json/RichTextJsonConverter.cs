using Ganss.XSS;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Infrastructure
{
	public class RichTextJsonConverter : JsonConverter
	{
		private static readonly HtmlSanitizer _sanitizer;

		static RichTextJsonConverter()
		{
			_sanitizer = new HtmlSanitizer(
				allowedTags: new[] {
					"b", "i", "u",
					"br", "p",
					"ul", "ol", "li",
					"h1", "h2", "h3", "h4", "h5", "h6",
					"a", "img"
				}, 
				allowedAttributes: new[] {
					"href", "alt", "title", "rel", "target"
			});
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Richtext);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}

			var richtext = new Richtext();

			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.EndObject)
				{
					return richtext;
				}

				if (reader.TokenType == JsonToken.PropertyName)
				{
					richtext.Data = _sanitizer.Sanitize(reader.ReadAsString());
				}
			}

			return richtext;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var richtext = value as Richtext;
			if (richtext == null)
			{
				throw new JsonSerializationException("Unexpected value when converting Richtext.");
			}

			DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

			writer.WriteStartObject();

			writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(nameof(Richtext.Data)) : nameof(Richtext.Data));
			writer.WriteValue(_sanitizer.Sanitize(richtext.Data));

			writer.WriteEndObject();
		}
	}
}
