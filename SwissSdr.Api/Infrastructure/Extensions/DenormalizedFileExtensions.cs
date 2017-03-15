using Microsoft.AspNetCore.Http;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using SwissSdr.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public static class DenormalizedFileSummaryExtensions
    {
		public static IHttpContextAccessor HttpContextAccessor { get; set; }
		private static Regex UrlSafe = new Regex("[^a-zA-Z0-9-_]");

		public static string GetImageUrl(this File file, ImageSize size)
		{
			if (size == ImageSize.Original)
			{
				return file.Url;
			}

			var request = HttpContextAccessor.HttpContext.Request;
			var filename = UrlSafe.Replace(file.Name.ValueByBestMatch(), string.Empty);
			return $"{request.Scheme}://{request.Host.ToUriComponent()}/v1/files/{RavenExtensions.GetIdValuePart(null, file.Id)}/{size.ToString().ToLowerInvariant()}/{filename}.jpg";
		}
		public static string GetImageUrl(this DenormalizedFileSummary file, ImageSize size)
		{
			if (size == ImageSize.Original)
			{
				return file.Url;
			}

			var request = HttpContextAccessor.HttpContext.Request;
			var filename = UrlSafe.Replace(file.Name.ValueByBestMatch(), string.Empty);
			return $"{request.Scheme}://{request.Host.ToUriComponent()}/v1/files/{RavenExtensions.GetIdValuePart(null, file.Id)}/{size.ToString().ToLowerInvariant()}/{filename}.jpg";
		}
	}
}
