using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public static class TypeExtensions
    {
		// from https://github.com/AutoMapper/AutoMapper.Extensions.Microsoft.DependencyInjection
		public static bool ImplementsGenericInterface(this Type type, Type interfaceType)
		{
			if (type.IsGenericType(interfaceType))
			{
				return true;
			}
			foreach (var @interface in type.GetTypeInfo().ImplementedInterfaces)
			{
				if (@interface.IsGenericType(interfaceType))
				{
					return true;
				}
			}
			return false;
		}

		// from https://github.com/AutoMapper/AutoMapper.Extensions.Microsoft.DependencyInjection
		public static bool IsGenericType(this Type type, Type genericType)
		{
			return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == genericType;
		}
	}
}
