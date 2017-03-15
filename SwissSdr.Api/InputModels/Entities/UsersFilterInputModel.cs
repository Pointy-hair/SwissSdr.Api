using AspNet.Mvc.TypedRouting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using SwissSdr.Api.Indexes;
using Raven.Client;
using Raven.Client.Linq;

namespace SwissSdr.Api.InputModels
{
	public class UsersFilterInputModel : IQueryCreator<Users_Filter.Result>, IQueryValueProvider
	{
		public string Query { get; set; }

		public SortOptions Sort { get; set; }

		public IRavenQueryable<Users_Filter.Result> CreateQuery(IAsyncDocumentSession session)
		{
			var query = session.Query<Users_Filter.Result, Users_Filter>();

			if (!string.IsNullOrEmpty(Query))
			{
				query = query.Search(r => r.Query, $"{Query}*", escapeQueryOptions: EscapeQueryOptions.AllowPostfixWildcard);
			}

			query = query.OrderBy(x => x.Fullname).ThenBy(x => x.Id);

			return query;
		}

		public RouteValueDictionary GetRouteValues()
		{
			var values = new RouteValueDictionary();

			if (!string.IsNullOrEmpty(Query))
			{
				values.Add(nameof(Query).ToCamelCase(), Query);
			}

			return values;
		}
	}
}
