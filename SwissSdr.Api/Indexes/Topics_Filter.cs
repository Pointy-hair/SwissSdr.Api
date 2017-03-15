using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using SwissSdr.Datamodel;
using Raven.Abstractions.Indexing;
using SwissSdr.Datamodel.Settings;

namespace SwissSdr.Api.Indexes
{
	public class Topics_Filter : AbstractIndexCreationTask<Topic>
	{
		public class Result
		{
			public TopicType Type { get; set; }
			public IEnumerable<string> Tags { get; set; }

			public IEnumerable<string> Query { get; set; }

			// sorting
			public string Id { get; set; }
			public Dictionary<string, string> Name { get; set; }
			public int MappedSdg { get; set; }
			public DateTime UpdatedAt { get; set; }
			public DateTime CreatedAt { get; set; }
		}

		public Topics_Filter()
		{
			Map = topics => from topic in topics
							let appSettings = LoadDocument<AppSettings>(AppSettings.AppSettingsId)
							let sdgMapping = appSettings.SdgTopicMappings.FirstOrDefault(m => m.TopicId == topic.Id)
							select new
							{
								Type = topic.Type,
								MappedSdg = sdgMapping.Sdg,
								Tags = topic.Tags,
								Query = topic.Name.Select(l => l.Value)
											   .Concat(topic.Description.Select(l => l.Value))
											   .Concat(topic.Tags),
								Id = topic.Id,
								UpdatedAt = topic.UpdatedAt,
								CreatedAt = topic.CreatedAt,
								_ = topic.Name.Select(x => CreateField(
										$"{nameof(Result.Name)}_{x.LanguageCode}",
										string.Concat(new [] {x.Value}.Concat(topic.Name.Where(l => l.LanguageCode != x.LanguageCode).Select(l => l.Value)))))
							};

			Index(nameof(Result.Query), FieldIndexing.Analyzed);
			Analyze(nameof(Result.Query), "StandardAnalyzer");

			Sort(nameof(Result.MappedSdg), Raven.Abstractions.Indexing.SortOptions.Int);
			Sort(nameof(Result.Id), Raven.Abstractions.Indexing.SortOptions.Int);
		}
	}
}
