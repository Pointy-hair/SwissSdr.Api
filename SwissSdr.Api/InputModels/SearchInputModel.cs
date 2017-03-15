using AspNet.Mvc.TypedRouting;
using SwissSdr.Api.Indexes;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Raven.Client;
using Raven.Client.Linq;
using System.Globalization;

namespace SwissSdr.Api.InputModels
{
    public class SearchInputModel : IAsyncQueryCreator<EntityBase_Search.Result>, IQueryValueProvider
    {
		public string Query { get; set; }
		public IEnumerable<string> Tags { get; set; }
		public string Related { get; set; }

		public IEnumerable<EntityType> Types { get; set; }

		public SortOptions? Sort { get; set; }

		public async Task<IRavenQueryable<EntityBase_Search.Result>> CreateQueryAsync(IAsyncDocumentSession session)
		{
			var query = session.Query<EntityBase_Search.Result, EntityBase_Search>();

			if (Tags?.Any() == true)
			{
				var tagsList = Tags.ToList();
				var tagQuery = session.Advanced.AsyncDocumentQuery<TagCluster, TagCluster_Tag>();
				for (int i = 0; i < tagsList.Count; i++)
				{
					tagQuery = tagQuery.WhereEquals("Tag", tagsList[i]);
					if (i < tagsList.Count - 1)
					{
						tagQuery = tagQuery.OrElse();
					}
				}

				var tagClusters = await tagQuery.ToListAsync();
				query = query.Where(r => r.Tags.In(tagClusters.SelectMany(c => c.Tags.Select(l => l.Value))));
			}

			if (!string.IsNullOrEmpty(Query))
			{
				query = query.Search(r => r.Query, $"{Query}*", escapeQueryOptions: EscapeQueryOptions.AllowPostfixWildcard);
			}

			if (Types?.Any() == true)
			{
				query = query.Where(r => r.EntityType.In(Types));
			}

			// TODO: add related querying

			var currentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
			switch (Sort)
			{
				case SortOptions.Random:
					query = query.Customize(opt => opt.RandomOrdering());
					break;
				case SortOptions.Default:
				case SortOptions.Name:
				default:
					query = query.OrderBy(x => x.Name[currentLanguage])
						.ThenBy(x => x.Id);
					break;
			}

			return query;
		}

		public RouteValueDictionary GetRouteValues()
		{
			var values = new RouteValueDictionary();

			if (!string.IsNullOrEmpty(Query))
			{
				values.Add(nameof(Query).ToCamelCase(), Query);
			}

			if (Tags != null && Tags.Any())
			{
				values.Add(nameof(Tags).ToCamelCase(), Tags);
			}

			if (!string.IsNullOrEmpty(Related))
			{
				values.Add(nameof(Related).ToCamelCase(), Related);
			}

			if (Types != null && Types.Any())
			{
				values.Add(nameof(Types).ToCamelCase(), Types);
			}

			if (Sort.HasValue)
			{
				values.Add(nameof(Sort).ToCamelCase(), Sort);
			}

			return values;
		}
	}
}
