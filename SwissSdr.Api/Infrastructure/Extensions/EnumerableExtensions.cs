using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public static class EnumerableExtensions
    {
		public static IEnumerable<string> WhereNotEmpty(this IEnumerable<string> source)
		{
			return source.Where(s => !string.IsNullOrEmpty(s));
		}

		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source)
		{
			return source.Where(s => s != null);
		}
		public static IEnumerable<T?> WhereNotNull<T>(this IEnumerable<T?> source) where T : struct
		{
			return source.Where(s => s.HasValue);
		}
		public static IEnumerable<T> UnboxNullable<T>(this IEnumerable<T?> source) where T: struct
		{
			return source.Select(x => x.GetValueOrDefault());
		}

		public static bool AnyNull<T>(this IEnumerable<T> source)
		{
			return source.Any(x => x == null);
		}
	}
}
