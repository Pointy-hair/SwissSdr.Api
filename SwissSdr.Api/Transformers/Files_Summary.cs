using Raven.Client.Indexes;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Transformers
{
    public class Files_Summary : AbstractTransformerCreationTask<File>
    {
		public const string Name = "Files/Summary";

		public Files_Summary()
		{
			TransformResults = entities => from entity in entities
										   where entity.Id != null
										   select new DenormalizedFileSummary()
										   {
											   Id = entity.Id,
											   Permissions = entity.Permissions,
											   Name = entity.Name,
											   Description = entity.Description,
											   Url = entity.Url,
										   };
		}
	}
}
