using Microsoft.AspNetCore.Mvc;
using SwissSdr.Api.Indexes;
using SwissSdr.Api.Infrastructure;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Linq;
using AspNet.Mvc.TypedRouting;
using Microsoft.AspNetCore.Routing;
using System.Globalization;

namespace SwissSdr.Api.InputModels
{
	public class OrganisationsFilterInputModel : IQueryCreator<Organisation_Filter.Result>, IQueryValueProvider
	{
		public string Query { get; set; }

		public IEnumerable<string> Tags { get; set; }

		public OrganisationType? Type { get; set; }
		public char? IsicClassification { get; set; }

		public string AssociatedEntity { get; set; }
		public string AssociationDescription { get; set; }

		public string Locality { get; set; }
		[ModelBinder(BinderType = typeof(GeoCoordinateModelBinder))]
		public GeoCoordinate? Coordinates { get; set; }

		public bool? HasJobs { get; set; }

		public SortOptions? Sort { get; set; }

		public IRavenQueryable<Organisation_Filter.Result> CreateQuery(IAsyncDocumentSession session)
		{
			var query = session.Query<Organisation_Filter.Result, Organisation_Filter>();

			if (!string.IsNullOrEmpty(Query))
			{
				query = query.Search(r => r.Query, $"{Query}*", escapeQueryOptions: EscapeQueryOptions.AllowPostfixWildcard);
			}

			if (Tags != null && Tags.Any())
			{
				query = query.Where(x => x.Tags.ContainsAll(Tags));
			}

			if (Type.HasValue)
			{
				query = query.Where(x => x.Type == Type.Value);
			}

			if (IsicClassification.HasValue)
			{
				query = query.Where(x => x.IsicClassifications.Contains(IsicClassification.Value));
			}

			if (!string.IsNullOrEmpty(AssociatedEntity))
			{
				query = query.Where(x => x.AssociatedEntityIds.Contains(AssociatedEntity));
			}

			if (!string.IsNullOrEmpty(AssociationDescription))
			{
				query = query.Where(x => x.AssociationDescriptions.Contains(AssociationDescription));
			}

			if (!string.IsNullOrEmpty(Locality))
			{
				query = query.Where(x => x.Localities.Contains(Locality));
			}

			if (Coordinates.HasValue)
			{
				query = query.Spatial(x => x.Coordinates, c => c.WithinRadius(20, Coordinates.Value.Latitude, Coordinates.Value.Longitude));
			}

			if (HasJobs.HasValue)
			{
				query = query.Where(x => x.HasJobs == HasJobs.Value);
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

			if (Type.HasValue)
			{
				values.Add(nameof(Type).ToCamelCase(), Type);
			}

			if (IsicClassification.HasValue)
			{
				values.Add(nameof(IsicClassification).ToCamelCase(), IsicClassification);
			}

			if (!string.IsNullOrEmpty(AssociatedEntity))
			{
				values.Add(nameof(AssociatedEntity).ToCamelCase(), AssociatedEntity);
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
				values.Add(nameof(Coordinates).ToCamelCase(), Coordinates);
			}

			if (HasJobs.HasValue)
			{
				values.Add(nameof(HasJobs).ToCamelCase(), HasJobs);
			}

			return values;
		}
	}
}
