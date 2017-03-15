using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
	public static class RavenExtensions
	{
		private static IDocumentStore _store;
		private static Dictionary<string, EntityType> _entityTypeFromKeyPrefix;
		private static Dictionary<Type, Type> _entityTypeFromDenormalized;

		public static IDocumentStore Store
		{
			get
			{
				return _store;
			}
			set
			{
				_store = value;
				Initialize();
			}
		}

		private static void Initialize()
		{
			_entityTypeFromKeyPrefix = new[] {
					typeof(Person),
					typeof(Project),
					typeof(Organisation),
					typeof(Event),
					typeof(Topic),
					typeof(File)
				}.ToDictionary(
					t => _store.Conventions.GetTypeTagName(t),
					t => EntityTypeExtensions.GetEntityType(t)
				);

			_entityTypeFromDenormalized = new Dictionary<Type, Type>()
			{
				{ typeof(DenormalizedPerson), typeof(Person) },
				{ typeof(DenormalizedProject), typeof(Project) },
				{ typeof(DenormalizedOrganisation), typeof(Organisation) },
				{ typeof(DenormalizedEvent), typeof(Event) },
				{ typeof(DenormalizedTopic), typeof(Topic) }
			};
		}

		public static int GetIdValuePart(this IAsyncDocumentSession session, string idString)
		{
			var idValuePart = Store.Conventions.FindIdValuePartForValueTypeConversion(null, idString);
			int id;
			if (int.TryParse(idValuePart, out id))
			{
				return id;
			}
			else
			{
				throw new ArgumentException($"Id value part of '{idString}' is not an int", nameof(idString));
			}
		}

		public static string GetFullId<TEntity>(this IAsyncDocumentSession session, int numericIdPart)
		{
			return Store.Conventions.FindFullDocumentKeyFromNonStringIdentifier(numericIdPart, typeof(TEntity), false);
		}

		public static async Task<T> LoadAsyncAndThrowIfNull<T>(this IAsyncDocumentSession session, string id)
		{
			var entity = await session.LoadAsync<T>(id);
			if (entity == null)
			{
				throw new ObjectNotFoundException();
			}
			return entity;
		}
		public static async Task<T> LoadAsyncAndThrowIfNull<T>(this IAsyncDocumentSession session, int id)
		{
			var entity = await session.LoadAsync<T>(id);
			if (entity == null)
			{
				throw new ObjectNotFoundException();
			}
			return entity;
		}

		public static async Task<T> LoadAsyncAndThrowIfNull<T>(this IAsyncLoaderWithInclude<T> loader, int id)
		{
			var entity = await loader.LoadAsync<T>(id);
			if (entity == null)
			{
				throw new ObjectNotFoundException();
			}
			return entity;
		}

		public static async Task<T> LoadAsyncAndThrowIfNull<T>(this IAsyncLoaderWithInclude<T> loader, string id)
		{
			var entity = await loader.LoadAsync<T>(id);
			if (entity == null)
			{
				throw new ObjectNotFoundException();
			}
			return entity;
		}

		public static async Task<IEnumerable<T>> LoadAsyncAndThrowIfNull<T>(this IAsyncDocumentSession session, IEnumerable<string> ids)
		{
			var entities = await session.LoadAsync<T>(ids);

			var entitiesNotFound = entities.Zip(ids, (e, id) => new { Id = id, Exists = e != null }).Where(x => !x.Exists);
			if (entitiesNotFound.Any())
			{
				throw new ApiException($"Could not find entity(s) {string.Join(", ", entitiesNotFound.Select(x => $"'{x.Id}'"))}.");
			}

			return entities;
		}

		public static async Task<T> LoadAsyncAndThrowIfNull<TTransformer, T>(this IAsyncDocumentSession session, int id)
			where TTransformer : AbstractTransformerCreationTask, new()
		{
			var fullId = Store.Conventions.FindFullDocumentKeyFromNonStringIdentifier(id, _entityTypeFromDenormalized[typeof(T)], false);

			var entity = await session.LoadAsync<TTransformer, T>(fullId);
			if (entity == null)
			{
				throw new ObjectNotFoundException();
			}
			return entity;
		}
		public static async Task<T> LoadAsyncAndThrowIfNull<TTransformer, T>(this IAsyncDocumentSession session, string id)
			where TTransformer : AbstractTransformerCreationTask, new()
		{
			var entity = await session.LoadAsync<TTransformer, T>(id);
			if (entity == null)
			{
				throw new ObjectNotFoundException();
			}
			return entity;
		}

		public static async Task<IEnumerable<T>> LoadAsyncAndThrowIfNull<TTransformer, T>(this IAsyncDocumentSession session, IEnumerable<string> ids)
			where TTransformer : AbstractTransformerCreationTask, new()
		{
			var entities = await session.LoadAsync<TTransformer, T>(ids);

			var entitiesNotFound = entities.Zip(ids, (e, id) => new { Id = id, Exists = e != null }).Where(x => !x.Exists);
			if (entitiesNotFound.Any())
			{
				throw new ApiException($"Could not find entity(s) {string.Join(", ", entitiesNotFound.Select(x => $"'{x.Id}'"))}.");
			}

			return entities;
		}

		public static IRavenQueryable<T> QueryFrom<T>(this IAsyncDocumentSession session, IQueryCreator<T> queryObject)
		{
			return queryObject.CreateQuery(session);
		}

		public static EntityType GetEntityType(this DenormalizedEntitySummary entitySummary)
		{
			return _entityTypeFromKeyPrefix[entitySummary.EntityName];
		}

		public static string GetEntityName(this EntityBase entity)
		{
			return _store.Conventions.GetTypeTagName(entity.GetType());
		}
	}
}
