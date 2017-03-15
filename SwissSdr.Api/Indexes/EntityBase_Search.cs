using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using Raven.Abstractions.Indexing;
using SwissSdr.Datamodel;

namespace SwissSdr.Api.Indexes
{
	public class EntityBase_Search : AbstractMultiMapIndexCreationTask<EntityBase_Search.Result>
	{
		public class Result
		{
			public EntityType EntityType { get; set; }
			public IEnumerable<string> Tags { get; set; }
			public IEnumerable<string> Query { get; set; }

			// sorting
			public string Id { get; set; }
			public Dictionary<string, string> Name { get; set; }
		}

		public EntityBase_Search()
		{
			AddMap<Person>(people => from person in people
									 select new
									 {
										 EntityType = EntityType.Person,
										 Tags = person.InterestAreas,
										 Query = new[] { person.Firstname, person.Lastname, person.Title }
											.Concat(person.InterestAreas),
										 Id = person.Id,
										 _ = new [] {"de", "en", "fr", "it"}.Select(lang => CreateField($"{nameof(Result.Name)}_{lang}", $"{person.Lastname}{person.Firstname}"))
									 });

			AddMap<Organisation>(organisations => from organisation in organisations
												  select new
												  {
													  EntityType = EntityType.Organisation,
													  Tags = organisation.Tags,
													  Query = organisation.Name.Select(l => l.Value)
														 .Concat(organisation.Description.Select(l => l.Value))
														 .Concat(organisation.Tags),
													  Id = organisation.Id,
													  _ = organisation.Name.Select(x => CreateField(
															  $"{nameof(Result.Name)}_{x.LanguageCode}",
															  string.Concat(new[] { x.Value }.Concat(organisation.Name.Where(l => l.LanguageCode != x.LanguageCode).Select(l => l.Value)))))
												  });

			AddMap<Project>(projects => from project in projects
										select new
										{
											EntityType = EntityType.Project,
											Tags = project.Tags,
											Query = project.Name.Select(l => l.Value)
											   .Concat(project.Description.Select(l => l.Value))
											   .Concat(project.Tags),
											Id = project.Id,
											_ = project.Name.Select(x => CreateField(
													$"{nameof(Result.Name)}_{x.LanguageCode}",
													string.Concat(new[] { x.Value }.Concat(project.Name.Where(l => l.LanguageCode != x.LanguageCode).Select(l => l.Value)))))
										});

			AddMap<Event>(events => from evnt in events
									select new
									{
										EntityType = EntityType.Event,
										Tags = evnt.Tags,
										Query = evnt.Name.Select(l => l.Value)
										   .Concat(evnt.Description.Select(l => l.Value))
										   .Concat(evnt.Tags),
										Id = evnt.Id,
										_ = evnt.Name.Select(x => CreateField(
												$"{nameof(Result.Name)}_{x.LanguageCode}",
												string.Concat(new[] { x.Value }.Concat(evnt.Name.Where(l => l.LanguageCode != x.LanguageCode).Select(l => l.Value)))))
									});

			AddMap<Topic>(topics => from topic in topics
									select new
									{
										EntityType = EntityType.Topic,
										Tags = topic.Tags,
										Query = topic.Name.Select(l => l.Value)
										   .Concat(topic.Description.Select(l => l.Value))
										   .Concat(topic.Tags),
										Id = topic.Id,
										_ = topic.Name.Select(x => CreateField(
												$"{nameof(Result.Name)}_{x.LanguageCode}",
												string.Concat(new[] { x.Value }.Concat(topic.Name.Where(l => l.LanguageCode != x.LanguageCode).Select(l => l.Value)))))
									});

			Index(r => r.Query, FieldIndexing.Analyzed);
			Analyze(r => r.Query, "StandardAnalyzer");
		}
	}
}
