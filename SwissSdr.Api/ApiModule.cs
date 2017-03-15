using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Raven.Client.Document;
using Raven.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Raven.Client.Indexes;
using AutoMapper;
using SwissSdr.Api.Configuration;
using Microsoft.Extensions.Options;
using SwissSdr.Api.Services;
using SwissSdr.Api.Infrastructure;
using SwissSdr.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using SwissSdr.Api.Endpoints;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Reflection;

namespace SwissSdr.Api
{
	public class ApiModule : Autofac.Module
	{
		private ILogger<ApiModule> _logger;

		public ApiModule(IConfigurationRoot configuration, ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<ApiModule>();
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ResourceFactory>().InstancePerLifetimeScope();

			builder.RegisterType<PermissionService>().InstancePerLifetimeScope();
			builder.RegisterType<TagService>().InstancePerLifetimeScope();

			builder.RegisterGeneric(typeof(AssociationsEndpoint<>)).InstancePerLifetimeScope();
			builder.RegisterGeneric(typeof(ImagesEndpoint<>)).InstancePerLifetimeScope();
			builder.RegisterGeneric(typeof(JobsEndpoint<>)).InstancePerLifetimeScope();
			builder.RegisterGeneric(typeof(LibraryEndpoint<>)).InstancePerLifetimeScope();
			builder.RegisterGeneric(typeof(PublicationsEndpoint<>)).InstancePerLifetimeScope();
			builder.RegisterGeneric(typeof(PermissionsEndpoint<>)).InstancePerLifetimeScope();
			
			builder.Register(CreateCloudBlobClient).InstancePerDependency();
			builder.Register(CreateGeocodingService).SingleInstance();

			// ravendb
			builder.Register(CreateDocumentStore).SingleInstance();
			builder.Register(CreateSession).InstancePerLifetimeScope();
		}

		protected CloudBlobClient CreateCloudBlobClient(IComponentContext context)
		{
			var options = context.Resolve<IOptions<AzureStorageOptions>>().Value;
			return new CloudBlobClient(options.BlobServiceEndpoint, new StorageCredentials(options.AccountName, options.ApiKey));
		}
		
		protected GeocodingService CreateGeocodingService(IComponentContext context)
		{
			var options = context.Resolve<IOptions<GeocodingOptions>>().Value;
			var logger = context.Resolve<ILogger<GeocodingService>>();
			return new GeocodingService(options.MapboxApiKey, logger);
		}

		protected IAsyncDocumentSession CreateSession(IComponentContext context)
		{
			var store = context.Resolve<IDocumentStore>();
			var session = store.OpenAsyncSession();

			session.Advanced.UseOptimisticConcurrency = true;

			return session;
		}

		protected IDocumentStore CreateDocumentStore(IComponentContext context)
		{
			var options = context.Resolve<IOptions<RavenDbOptions>>().Value;

			var documentStore = new DocumentStore()
			{
				Url = options.Url,
				ApiKey = options.ApiKey
			};
			documentStore.Initialize();
			RavenExtensions.Store = documentStore;
			
			documentStore.RegisterListener(new UpdatedAtListener());
			IndexCreation.CreateIndexes(typeof(Indexes.People_Filter).GetTypeInfo().Assembly, documentStore);

			return documentStore;
		}
	}
}
