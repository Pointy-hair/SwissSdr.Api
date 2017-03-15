using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using SwissSdr.Datamodel;
using Raven.Abstractions.Indexing;

namespace SwissSdr.Api.Indexes
{
    public class Organisation_Filter : AbstractIndexCreationTask<Organisation>
    {
		public class Result
		{
			public IEnumerable<string> Tags { get; set; }
            public OrganisationType Type { get; set; }
            public IEnumerable<char> IsicClassifications { get; set; }

            public IEnumerable<string> AssociatedEntityIds { get; set; }
			public IEnumerable<string> AssociationDescriptions { get; set; }
			public IEnumerable<string> Localities { get; set; }
			public object Coordinates { get; set; }

            public bool HasJobs { get; set; }

			public IEnumerable<string> Query { get; set; }

			// sorting
			public string Id { get; set; }
			public Dictionary<string, string> Name { get; set; }
			public DateTime UpdatedAt { get; set; }
			public DateTime CreatedAt { get; set; }
		}

        public Organisation_Filter()
		{
			Map = organisations => from organisation in organisations
							select new
							{
								Tags = organisation.Tags,
                                Type = organisation.Type,
                                IsicClassifications = organisation.IsicClassification,
								AssociatedEntityIds = organisation.Associations.Select(e => e.TargetId),
								AssociationDescriptions = organisation.Associations.Select(a => a.Description),
								Localities = organisation.ContactInfo.Addresses.Select(a => a.Locality),
                                Coordinates = SpatialGenerate("Coordinates", organisation.ContactInfo.Addresses.First().Coordinates.Latitude, organisation.ContactInfo.Addresses.First().Coordinates.Longitude),
                                HasJobs = organisation.Jobs.Any(),
								Query = organisation.Name.Select(l => l.Value)
														 .Concat(organisation.Description.Select(l => l.Value))
														 .Concat(organisation.Tags),
								Id = organisation.Id,
								UpdatedAt = organisation.UpdatedAt,
								CreatedAt = organisation.CreatedAt,
								_ = organisation.Name.Select(x => CreateField(
										$"{nameof(Result.Name)}_{x.LanguageCode}",
										string.Concat(new[] { x.Value }.Concat(organisation.Name.Where(l => l.LanguageCode != x.LanguageCode).Select(l => l.Value)))))
							};

			Index(nameof(Result.Query), FieldIndexing.Analyzed);
			Analyze(nameof(Result.Query), "StandardAnalyzer");

			Sort(nameof(Result.Id), Raven.Abstractions.Indexing.SortOptions.Int);
		}
	}
}
