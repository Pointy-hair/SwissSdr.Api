using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
	public static class RequestExtensions
	{
		public static RequestCulture GetCulture(this HttpRequest request)
		{
			var requestCultureFeature = request.HttpContext.Features.Get<IRequestCultureFeature>();
			if (requestCultureFeature == null)
			{
				throw new InvalidOperationException("Could not find IRequestCultureFeature on HttpContext. Is the localization middleware active?");
			}

			return requestCultureFeature.RequestCulture;
		}

		public static string GetLanguage(this HttpRequest request)
		{
			var culture = GetCulture(request);
			return culture.UICulture.TwoLetterISOLanguageName;
		}

		// https://github.com/aspnet/Mvc/blob/760c8f38678118734399c58c2dac981ea6e47046/src/Microsoft.AspNetCore.Mvc.Core/Internal/ObjectResultExecutor.cs
		public static List<MediaTypeSegmentWithQuality> GetAcceptableMediaTypes(this HttpRequest request)
		{
			var result = new List<MediaTypeSegmentWithQuality>();
			AcceptHeaderParser.ParseAcceptHeader(request.Headers[HeaderNames.Accept], result);

			result.Sort((left, right) => left.Quality > right.Quality ? -1 : (left.Quality == right.Quality ? 0 : 1));

			return result;
		}
	}
}
