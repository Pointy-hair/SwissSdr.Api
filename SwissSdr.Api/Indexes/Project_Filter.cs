using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using Raven.Abstractions.Indexing;
using SwissSdr.Datamodel;

namespace SwissSdr.Api.Indexes
{
    public class Project_Filter : AbstractIndexCreationTask<Project>
    {
        public class Result
        {
            public IEnumerable<string> Tags { get; set; }
            public ProjectPhase Phase { get; set; }
            public DateTime? Begin { get; set; }
            public DateTime? End { get; set; }
            public IEnumerable<string> AssociatedEntityIds { get; set; }
            public IEnumerable<string> AssociationDescriptions { get; set; }
            public bool HasJobs { get; set; }

			public IEnumerable<string> Query { get; set; }

			// sorting
			public string Id { get; set; }
			public Dictionary<string, string> Name { get; set; }
			public DateTime UpdatedAt { get; set; }
			public DateTime CreatedAt { get; set; }
		}

        public Project_Filter()
        {
            Map = projects => from project in projects
                            select new
                            {
                                Tags = project.Tags,
                                Phase = project.Phase,
                                Begin = project.Begin,
                                End = project.End,
                                AssociatedEntityIds = project.Associations.Select(e => e.TargetId),
                                AssociationDescriptions = project.Associations.Select(a => a.Description),
                                HasJobs = project.Jobs.Any(),
								Query = project.Name.Select(l => l.Value)
											   .Concat(project.Description.Select(l => l.Value))
											   .Concat(project.Tags),
								Id = project.Id,
								UpdatedAt = project.UpdatedAt,
								CreatedAt = project.CreatedAt,
								_ = project.Name.Select(x => CreateField(
										$"{nameof(Result.Name)}_{x.LanguageCode}",
										string.Concat(new[] { x.Value }.Concat(project.Name.Where(l => l.LanguageCode != x.LanguageCode).Select(l => l.Value)))))
							};

			Index(nameof(Result.Query), FieldIndexing.Analyzed);
			Analyze(nameof(Result.Query), "StandardAnalyzer");

			Sort(nameof(Result.Id), Raven.Abstractions.Indexing.SortOptions.Int);
		}
    }
}
