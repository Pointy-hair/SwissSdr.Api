using AspNet.Mvc.TypedRouting;
using Microsoft.AspNetCore.Mvc;
using Raven.Client;
using Raven.Client.Linq;
using SwissSdr.Api.Indexes;
using SwissSdr.Api.Infrastructure;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Routing;
using System.Globalization;

namespace SwissSdr.Api.InputModels
{
	public class PeopleFilterInputModel : IQueryCreator<People_Filter.Result>, IQueryValueProvider
	{
		public string Query { get; set; }

		public IEnumerable<string> InterestAreas { get; set; }

		public IEnumerable<string> Languages { get; set; }

		public IEnumerable<string> AssociatedEntities { get; set; }

		public string AssociationDescription { get; set; }

		public string Locality { get; set; }

		[ModelBinder(BinderType = typeof(GeoCoordinateModelBinder))]
		public GeoCoordinate? Coordinates { get; set; }

		public SortOptions? Sort { get; set; }

		public IRavenQueryable<People_Filter.Result> CreateQuery(IAsyncDocumentSession session)
		{
			var query = session.Query<People_Filter.Result, People_Filter>();

			if (!string.IsNullOrEmpty(Query))
			{
				query = query.Search(r => r.Query, $"{Query}*", escapeQueryOptions: EscapeQueryOptions.AllowPostfixWildcard);
			}

			if (InterestAreas != null && InterestAreas.Any())
			{
				query = query.Where(x => x.InterestAreas.ContainsAll(InterestAreas));
			}

			if (Languages != null && Languages.Any())
			{
				query = query.Where(x => x.Languages.In(Languages));
			}

			if (!string.IsNullOrEmpty(AssociationDescription) && AssociatedEntities != null && AssociatedEntities.Any(s => !string.IsNullOrEmpty(s)))
			{
				query = query.Where(x => x.Associations.ContainsAny(AssociatedEntities.Select(id => People_Filter.CreateAssociationQueryValue(id, AssociationDescription))));
			}
			else
			{
				if (AssociatedEntities != null && AssociatedEntities.Any(s => !string.IsNullOrEmpty(s)))
				{
					query = query.Where(x => x.AssociationTargets.ContainsAll(AssociatedEntities));
				}

				if (!string.IsNullOrEmpty(AssociationDescription))
				{
					query = query.Where(x => x.AssociationDescriptions.Contains(AssociationDescription));
				}
			}

			if (!string.IsNullOrEmpty(Locality))
			{
				query = query.Where(x => x.Localities.Contains(Locality));
			}

			if (Coordinates.HasValue)
			{
				query = query.Spatial(x => x.Coordinates, c => c.WithinRadius(20, Coordinates.Value.Latitude, Coordinates.Value.Longitude));
			}

			var currentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
			switch (Sort)
			{
				case SortOptions.Random:
					query = query.Customize(opt => opt.RandomOrdering());
					break;

				case SortOptions.Updated:
					query = query.OrderByDescending(x => x.UpdatedAt);
					break;

				case SortOptions.Created:
					query = query.OrderByDescending(x => x.CreatedAt);
					break;

				case SortOptions.Name:
				case SortOptions.Default:
				default:
					query = query.OrderBy(x => x.Fullname)
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

			if (InterestAreas != null && InterestAreas.Any())
			{
				values.Add(nameof(InterestAreas).ToCamelCase(), InterestAreas);
			}

			if (Languages != null && Languages.Any())
			{
				values.Add(nameof(Languages).ToCamelCase(), Languages);
			}

			if (AssociatedEntities != null && AssociatedEntities.Any(s => !string.IsNullOrEmpty(s)))
			{
				values.Add(nameof(AssociatedEntities).ToCamelCase(), AssociatedEntities);
			}

			if (!string.IsNullOrEmpty(AssociationDescription))
			{
				values.Add(nameof(AssociationDescription).ToCamelCase(), AssociationDescription);
			}

			if (!string.IsNullOrEmpty(Locality))
			{
				values.Add(nameof(Locality).ToCamelCase(), Locality);
			}

			if (Coordinates.HasValue)
			{
				values.Add(nameof(Coordinates).ToCamelCase(), Coordinates.Value.ToString());
			}

			return values;
		}
	}
}
