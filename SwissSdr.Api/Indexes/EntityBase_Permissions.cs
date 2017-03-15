using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using Raven.Client.Linq.Indexing;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Indexes
{
	public class EntityBase_Permissions : AbstractMultiMapIndexCreationTask<EntityBase_Permissions.Result>
	{
		public class Result
		{
			public int UserIdNumerical { get; set; }
			public IEnumerable<DenormalizedEntitySummary> EntityPermissions { get; set; }
		}

		public EntityBase_Permissions()
		{

			AddMap<Person>(people => from person in people
									 let image = LoadDocument<File>(person.ProfileImageId)
									 let hasImage = image != null
									 from item in person.Permissions
									 select new
									 {
										 UserIdNumerical = item.Key.Split('/')[1].ParseInt(),
										 EntityPermissions = new[] {
											 new {
												 EntityName = MetadataFor(person).Value<string>("Raven-Entity-Name"),
												 Id = person.Id,
												 Permissions = new [] { new { item.Key, item.Value } }.ToDictionary(x => x.Key, x => x.Value),
												 Title = person.Title,
												 Firstname = person.Firstname,
												 Lastname = person.Lastname,
												 Image = !hasImage ? null : new DenormalizedFileSummary()
												 {
													 Id = image.Id,
													 Permissions = image.Permissions,
													 Name = image.Name,
													 Description = image.Description,
													 Url = image.Url
												 }
											 }
										 }
									 });

			AddMap<Organisation>(organisations => from organisation in organisations
												  let image = LoadDocument<File>(organisation.ProfileImageId)
												  let hasImage = image != null
												  from item in organisation.Permissions
												  select new
												  {
													  UserIdNumerical = item.Key.Split('/')[1].ParseInt(),
													  EntityPermissions = new[] {
														 new {
															 EntityName = MetadataFor(organisation).Value<string>("Raven-Entity-Name"),
															 Id = organisation.Id,
												 Permissions = new [] { new { item.Key, item.Value } }.ToDictionary(x => x.Key, x => x.Value),
															 Name = organisation.Name,
															 Description = organisation.Description,
															 Image = !hasImage ? null : new DenormalizedFileSummary()
															 {
																 Id = image.Id,
																 Permissions = image.Permissions,
																 Name = image.Name,
																 Description = image.Description,
																 Url = image.Url
															 }
														 }
													 }
												  });

			AddMap<Project>(projects => from project in projects
										let image = LoadDocument<File>(project.ImageIds.FirstOrDefault())
										let hasImage = image != null
										from item in project.Permissions
										select new
										{
											UserIdNumerical = item.Key.Split('/')[1].ParseInt(),
											EntityPermissions = new[] {
												new {
													EntityName = MetadataFor(project).Value<string>("Raven-Entity-Name"),
													Id = project.Id,
												 Permissions = new [] { new { item.Key, item.Value } }.ToDictionary(x => x.Key, x => x.Value),
													Name = project.Name,
													Description = project.Description,
													Image = !hasImage ? null : new DenormalizedFileSummary()
													{
														Id = image.Id,
														Permissions = image.Permissions,
														Name = image.Name,
														Description = image.Description,
														Url = image.Url
													}
											 }
										 }
										});

			AddMap<Event>(events => from evnt in events
									let image = LoadDocument<File>(evnt.ImageIds.FirstOrDefault())
									let hasImage = image != null
									from item in evnt.Permissions
									select new
									{
										UserIdNumerical = item.Key.Split('/')[1].ParseInt(),
										EntityPermissions = new[] {
											 new {
												 EntityName = MetadataFor(evnt).Value<string>("Raven-Entity-Name"),
												 Id = evnt.Id,
												 Permissions = new [] { new { item.Key, item.Value } }.ToDictionary(x => x.Key, x => x.Value),
												 Name = evnt.Name,
												 Description = evnt.Description,
												 Begin = evnt.Begin,
												 End = evnt.End,
												 Image = !hasImage ? null : new DenormalizedFileSummary()
												 {
													 Id = image.Id,
													 Permissions = image.Permissions,
													 Name = image.Name,
													 Description = image.Description,
													 Url = image.Url
												 }
											 }
										 }
									});

			AddMap<Topic>(topics => from topic in topics
									let image = LoadDocument<File>(topic.ImageIds.FirstOrDefault())
									let hasImage = image != null
									from item in topic.Permissions
									select new
									{
										UserIdNumerical = item.Key.Split('/')[1].ParseInt(),
										EntityPermissions = new[] {
											 new {
												 EntityName = MetadataFor(topic).Value<string>("Raven-Entity-Name"),
												 Id = topic.Id,
												 Permissions = new [] { new { item.Key, item.Value } }.ToDictionary(x => x.Key, x => x.Value),
												 Name = topic.Name,
												 Description = topic.Description,
												 Image = !hasImage ? null : new DenormalizedFileSummary()
												 {
													 Id = image.Id,
													 Permissions = image.Permissions,
													 Name = image.Name,
													 Description = image.Description,
													 Url = image.Url
												 }
											 }
										 }
									});

			Reduce = results => from result in results
								group result by result.UserIdNumerical
								into g
								select new Result
								{
									UserIdNumerical = g.Key,
									EntityPermissions = g.SelectMany(x => x.EntityPermissions)
								};

			StoreAllFields(FieldStorage.Yes);
		}
	}
}
