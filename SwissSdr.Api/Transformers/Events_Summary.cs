using Raven.Client.Indexes;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Transformers
{
    public class Events_Summary : AbstractTransformerCreationTask<Event>
    {
		public const string Name = "Events/Summary";

		public Events_Summary()
		{
			TransformResults = entities => from entity in entities
										   where entity.Id != null
										   let image = LoadDocument<File>(entity.ImageIds.FirstOrDefault())
										   let address = entity.ContactInfo.Addresses.FirstOrDefault()
										   select new DenormalizedEventSummary()
										   {
											   Id = entity.Id,
											   Permissions = entity.Permissions,
											   Name = entity.Name,
											   Description = entity.Description,
											   Begin = entity.Begin,
											   End = entity.End,
											   Image = TransformWith<File, DenormalizedFileSummary>(Files_Summary.Name, image).SingleOrDefault(),
											   Coordinates = address == null ? new GeoCoordinate() : address.Coordinates
										   };
		}
	}
}
