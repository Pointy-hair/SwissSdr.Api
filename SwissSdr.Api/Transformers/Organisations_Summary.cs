using Raven.Client.Indexes;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Transformers
{
    public class Organisations_Summary : AbstractTransformerCreationTask<Organisation>
    {
		public const string Name = "Organisations/Summary";

		public Organisations_Summary()
		{
			TransformResults = entities => from entity in entities
										   where entity.Id != null
										   let image = LoadDocument<File>(entity.ProfileImageId)
										   let address = entity.ContactInfo.Addresses.FirstOrDefault()
										   select new DenormalizedOrganisationSummary()
										   {
											   Id = entity.Id,
											   Permissions = entity.Permissions,
											   Name = entity.Name,
											   Description = entity.Description,
											   ProfileImage = TransformWith<File, DenormalizedFileSummary>(Files_Summary.Name, image).SingleOrDefault(),
											   Coordinates = address == null ? new GeoCoordinate() : address.Coordinates
										   };
		}
	}
}
