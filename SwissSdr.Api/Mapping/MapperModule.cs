using Autofac;
using AutoMapper;
using SwissSdr.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SwissSdr.Api.Mapping
{
	// assembly scanning code from https://github.com/AutoMapper/AutoMapper.Extensions.Microsoft.DependencyInjection

	public class MapperModule : Autofac.Module
	{
		private Type[] _resolverAndConverterTypes;

		public MapperModule()
		{
			_resolverAndConverterTypes = new[]
			{
				typeof(IValueResolver<,,>),
				typeof(IMemberValueResolver<,,,>),
				typeof(ITypeConverter<,>)
			};
		}

		protected override void Load(ContainerBuilder builder)
		{
			var thisAssembly = typeof(MapperModule).GetTypeInfo().Assembly;

			builder.RegisterAssemblyTypes(thisAssembly)
				.Where(t => typeof(Profile).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
				.Where(t => !t.GetTypeInfo().IsAbstract)
				.As<Profile>()
				.SingleInstance();

			builder.RegisterAssemblyTypes(thisAssembly)
				.Where(t => t.GetTypeInfo().IsClass)
				.Where(t => !t.GetTypeInfo().IsAbstract)
				.Where(t => !t.GetTypeInfo().ContainsGenericParameters)
				.Where(t => _resolverAndConverterTypes.Any(ct => t.ImplementsGenericInterface(ct)))
				.InstancePerLifetimeScope();

			builder.Register(c => CreateConfiguration(c.Resolve<IEnumerable<Profile>>()))
				.SingleInstance();
			builder.Register(c => CreateMapper(c.Resolve<IConfigurationProvider>(), c.Resolve<Func<IComponentContext>>()))
				.As<IMapper>()
				.InstancePerLifetimeScope();
		}

		private static Mapper CreateMapper(IConfigurationProvider configurationProvider, Func<IComponentContext> contextFactory)
		{
			return new Mapper(configurationProvider, t => contextFactory().Resolve(t));
		}

		protected IConfigurationProvider CreateConfiguration(IEnumerable<Profile> profiles)
		{
			var config = new MapperConfiguration(cfg =>
			{
				foreach (var profile in profiles)
				{
					cfg.AddProfile(profile);
				}
			});

			//config.AssertConfigurationIsValid();

			return config;
		}
	}
}
