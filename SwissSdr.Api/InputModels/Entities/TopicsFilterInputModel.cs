using SwissSdr.Api.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Linq;
using AspNet.Mvc.TypedRouting;
using Microsoft.AspNetCore.Routing;
using SwissSdr.Datamodel;
using System.Globalization;

namespace SwissSdr.Api.InputModels
{
    public class TopicsFilterInputModel : IQueryCreator<Topics_Filter.Result>, IQueryValueProvider
	{
		public TopicType? Type { get; set; }
		public string Query { get; set; }

		public IEnumerable<string> Tags { get; set; }

		public SortOptions? Sort { get; set; }

		public IRavenQueryable<Topics_Filter.Result> CreateQuery(IAsyncDocumentSession session)
		{
			var query = session.Query<Topics_Filter.Result, Topics_Filter>();

			if (Type.HasValue)
			{
				query = query.Where(x => x.Type == Type.Value);
			}

			if (!string.IsNullOrEmpty(Query))
			{
				query = query.Search(r => r.Query, $"{Query}*", escapeQueryOptions: EscapeQueryOptions.AllowPostfixWildcard);
			}

			if (Tags != null && Tags.Any())
			{
				query = query.Where(x => x.Tags.ContainsAll(Tags));
			}

			var currentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
			switch (Sort)
			{
				case SortOptions.Name:
					query = query.OrderBy(x => x.Name[currentLanguage])
						.ThenBy(x => x.Id);
					break;

				case SortOptions.Random:
					query = query.Customize(opt => opt.RandomOrdering());
					break;

				case SortOptions.Updated:
					query = query.OrderByDescending(x => x.UpdatedAt);
					break;

				case SortOptions.Created:
					query = query.OrderByDescending(x => x.CreatedAt);
					break;

				case SortOptions.Default:
				default:
					query = query.OrderBy(x => x.MappedSdg);
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

			return values;
		}
	}
}
