using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using SwissSdr.Datamodel;
using Raven.Abstractions.Indexing;

namespace SwissSdr.Api.Indexes
{
    public class Event_Filter : AbstractIndexCreationTask<Event>
    {
		public class Result
		{
			public IEnumerable<string> Tags { get; set; }
            public DateTime Begin { get; set; }
            public DateTime End { get; set; }
            public IEnumerable<string> Languages { get; set; }
			public IEnumerable<string> AssociatedEntityIds { get; set; }
			public IEnumerable<string> AssociationDescriptions { get; set; }
			public IEnumerable<string> Localities { get; set; }
			public object Coordinates { get; set; }

			public IEnumerable<string> Query { get; set; }

			// sorting
			public string Id { get; set; }
			public Dictionary<string, string> Name { get; set; }
			public DateTime UpdatedAt { get; set; }
			public DateTime CreatedAt { get; set; }
		}

		public Event_Filter()
		{
			Map = events => from evnt in events
							select new
							{
								Tags = evnt.Tags,
                                Begin = evnt.Begin,
                                End = evnt.End,
								Languages = evnt.Languages,
								AssociatedEntityIds = evnt.Associations.Select(e => e.TargetId),
								AssociationDescriptions = evnt.Associations.Select(a => a.Description),
								Localities = evnt.ContactInfo.Addresses.Select(a => a.Locality),
								Query = evnt.Name.Select(l => l.Value)
											   .Concat(evnt.Description.Select(l => l.Value))
											   .Concat(evnt.Tags),
								Id = evnt.Id,
								UpdatedAt = evnt.UpdatedAt,
								CreatedAt = evnt.CreatedAt,
								_ = evnt.Name.Select(x => CreateField(
										$"{nameof(Result.Name)}_{x.LanguageCode}",
										string.Concat(new[] { x.Value }.Concat(evnt.Name.Where(l => l.LanguageCode != x.LanguageCode).Select(l => l.Value)))))
							};

			Index(nameof(Result.Query), FieldIndexing.Analyzed);
			Analyze(nameof(Result.Query), "StandardAnalyzer");

			Sort(nameof(Result.Id), Raven.Abstractions.Indexing.SortOptions.Int);
		}
	}
}
