using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;

namespace SwissSdr.Api.Infrastructure
{
	/// <summary>
	/// A custom Newtonsoft.Json contract resolver, inheriting from <see cref="CamelCasePropertyNamesContractResolver"/> 
	/// that always serializes <see cref="Enum"/> values, regardless of the set <see cref="JsonSerializerSettings.DefaultValueHandling"/>.
	/// </summary>
	public class CustomContractResolver : CamelCasePropertyNamesContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);

			if (typeof(Enum).IsAssignableFrom(property.PropertyType))
			{
				property.DefaultValueHandling = DefaultValueHandling.Include;
			}

			return property;
		}
	}
}
