using Raven.Client.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public static class RavenQueryableExtensions
    {


		public static IRavenQueryable<T> Paged<T>(this IRavenQueryable<T> query, int? skip, int? take, int defaultPageSize = ApiConstants.DefaultPageSize)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return query
				.Skip(skip ?? 0)
				.Take(take ?? defaultPageSize);
		}
    }
}
