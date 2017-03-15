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
    public class People_Summary : AbstractTransformerCreationTask<Person>
    {
		public const string Name = "People/Summary";

		public People_Summary()
		{
			TransformResults = entities => from entity in entities
										   where entity.Id != null
										   let image = LoadDocument<File>(entity.ProfileImageId)
										   let associatedOrganisations = LoadDocument<Organisation>(entity.Associations.Select(a => a.TargetId).Where(s => s.StartsWith("organisation", StringComparison.OrdinalIgnoreCase)))
										   let address = entity.ContactInfo.Addresses.FirstOrDefault()
										   select new DenormalizedPersonSummary()
										   {
											   Id = entity.Id,
											   Permissions = entity.Permissions,
											   Firstname = entity.Firstname,
											   Lastname = entity.Lastname,
											   Title = entity.Title,
											   InterestAreas = entity.InterestAreas,
											   ProfileImage = TransformWith<File, DenormalizedFileSummary>(Files_Summary.Name, image).SingleOrDefault(),
											   AssociatedOrganisationNames = associatedOrganisations.Select(o => o.Name),
											   Coordinates = address == null ? new GeoCoordinate() : address.Coordinates
										   };
		}
	}
}
