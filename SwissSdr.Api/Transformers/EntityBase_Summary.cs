using Raven.Client.Indexes;
using Raven.Client.Linq.Indexing;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Transformers
{
    public class EntityBase_Summary : AbstractTransformerCreationTask<EntityBase>
    {
		public const string Name = "EntityBase/Summary";

		public EntityBase_Summary()
		{
			TransformResults = entities => from entity in entities
										   where entity.Id != null
										   let json = AsDocument(entity)
										   let metadata = MetadataFor(entity)
										   let imageId = json["ImageIds"] == null
											 ? json.Value<string>("ProfileImageId")
											 : json.Value<string[]>("ImageIds").FirstOrDefault()
										   let image = LoadDocument<File>(imageId)
										   let associatedOrganisations = LoadDocument<Organisation>(json.Value<Association[]>("Associations").Select(a => a.TargetId).Where(s => s.StartsWith("Organisations")))
										   let contactInfo = json.Value<ContactInfo>("ContactInfo")
										   select new DenormalizedEntitySummary()
										   {
											   EntityName = metadata.Value<string>("Raven-Entity-Name"),
											   Id = entity.Id,
											   Permissions = entity.Permissions,
											   Title = json.Value<string>("Title"),
											   Firstname = json.Value<string>("Firstname"),
											   Lastname = json.Value<string>("Lastname"),
											   Begin = json.Value<DateTime>("Begin"),
											   End = json.Value<DateTime>("End"),
											   Name = json.Value<Multilingual<string>>("Name"),
											   Description = json.Value<Multilingual<string>>("Description"),
											   InterestAreas = json.Value<string[]>("InterestAreas"),
											   AssociatedOrganisationNames = associatedOrganisations.Select(o => o.Name),
											   Image = image == null ? null : new DenormalizedFileSummary()
											   {
												   Id = image.Id,
												   Permissions = image.Permissions,
												   Name = image.Name,
												   Description = image.Description,
												   Url = image.Url
											   },
											   ContactInfo = contactInfo
										   };
		}
	}
}
