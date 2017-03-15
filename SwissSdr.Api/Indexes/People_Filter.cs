using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using SwissSdr.Datamodel;
using Raven.Abstractions.Indexing;

namespace SwissSdr.Api.Indexes
{
	public class People_Filter : AbstractIndexCreationTask<Person>
	{
		public class Result
		{
			public IEnumerable<string> InterestAreas { get; set; }
			public IEnumerable<string> Languages { get; set; }
			public IEnumerable<string> Localities { get; set; }

			public IEnumerable<string> Associations { get; set; }
			public IEnumerable<string> AssociationTargets { get; set; }
			public IEnumerable<string> AssociationDescriptions { get; set; }

			public object Coordinates { get; set; }

			public IEnumerable<string> Query { get; set; }

			// sorting
			public string Id { get; set; }
			public string Fullname { get; set; }
			public DateTime UpdatedAt { get; set; }
			public DateTime CreatedAt { get; set; }
		}

		public People_Filter()
		{
			Map = people => from person in people
							select new Result
							{
								InterestAreas = person.InterestAreas,
								Languages = person.Languages,
								Localities = person.ContactInfo.Addresses.Select(a => a.Locality),
								Coordinates = SpatialGenerate("Coordinates", person.ContactInfo.Addresses.First().Coordinates.Latitude, person.ContactInfo.Addresses.First().Coordinates.Longitude),
								Associations = person.Associations.Select(a => a.TargetId + "," + a.Description),
								AssociationTargets = person.Associations.Select(a => a.TargetId),
								AssociationDescriptions = person.Associations.Select(a => a.Description),
								Query = new[]
								{
									person.Firstname,
									person.Lastname,
									person.Title
								}.Concat(person.InterestAreas).Concat(person.InterestAreas),
								Id = person.Id,
								Fullname = $"{person.Lastname} {person.Firstname}",
								UpdatedAt = person.UpdatedAt,
								CreatedAt = person.CreatedAt
							};

			Index(nameof(Result.Query), FieldIndexing.Analyzed);
			Analyze(nameof(Result.Query), "StandardAnalyzer");

			Sort(nameof(Result.Id), Raven.Abstractions.Indexing.SortOptions.Int);
		}

		public static string CreateAssociationQueryValue(string id, string description)
		{
			return $"{id},{description}";
		}
	}
}
