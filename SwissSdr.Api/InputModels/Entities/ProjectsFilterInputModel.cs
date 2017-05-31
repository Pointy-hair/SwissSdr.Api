using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Linq;
using SwissSdr.Api.Indexes;
using AspNet.Mvc.TypedRouting;
using Microsoft.AspNetCore.Routing;
using System.Globalization;

namespace SwissSdr.Api.InputModels
{
	public class ProjectsFilterInputModel : IQueryCreator<Indexes.Project_Filter.Result>, IQueryValueProvider
	{
		public string Query { get; set; }

		public IEnumerable<string> Tags { get; set; }

		public IEnumerable<ProjectPhase> Phases { get; set; }
		public DateTime? Begin { get; set; }
		public DateTime? End { get; set; }

		public string AssociatedEntity { get; set; }
		public string AssociationDescription { get; set; }

		public bool? HasJobs { get; set; }

		public SortOptions? Sort { get; set; }

		public IRavenQueryable<Project_Filter.Result> CreateQuery(IAsyncDocumentSession session)
		{
			var query = session.Query<Project_Filter.Result, Project_Filter>();

			if (!string.IsNullOrEmpty(Query))
			{
				query = query.Search(r => r.Query, $"{Query}*", escapeQueryOptions: EscapeQueryOptions.AllowPostfixWildcard);
			}

			if (Tags != null && Tags.Any())
			{
				query = query.Where(x => x.Tags.ContainsAll(Tags));
			}

			if (Phases != null && Phases.Any())
			{
				query = query.Where(x => x.Phase.In(Phases));
			}

			if (Begin.HasValue)
			{
				query = query.Where(x => x.Begin.HasValue && Begin <= x.Begin);
			}

			if (End.HasValue)
			{
				query = query.Where(x => x.End.HasValue && End >= x.End);
			}

			if (!string.IsNullOrEmpty(AssociatedEntity))
			{
				query = query.Where(x => x.AssociatedEntityIds.Contains(AssociatedEntity));
			}

			if (!string.IsNullOrEmpty(AssociationDescription))
			{
				query = query.Where(x => x.AssociationDescriptions.Contains(AssociationDescription));
			}

			if (HasJobs.HasValue)
			{
				query = query.Where(x => x.HasJobs == HasJobs);
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

			if (Phases != null && Phases.Any())
			{
				values.Add(nameof(Phases).ToCamelCase(), Phases);
			}

			if (Begin.HasValue)
			{
				values.Add(nameof(Begin).ToCamelCase(), Begin.Value.ToString("yyyy-MM-dd"));
			}

			if (End.HasValue)
			{
				values.Add(nameof(End).ToCamelCase(), End.Value.ToString("yyyy-MM-dd"));
			}

			if (!string.IsNullOrEmpty(AssociatedEntity))
			{
				values.Add(nameof(AssociatedEntity).ToCamelCase(), AssociatedEntity);
			}

			if (!string.IsNullOrEmpty(AssociationDescription))
			{
				values.Add(nameof(AssociationDescription).ToCamelCase(), AssociationDescription);
			}

			if (HasJobs.HasValue)
			{
				values.Add(nameof(HasJobs).ToCamelCase(), HasJobs);
			}

			return values;
		}
	}
}
