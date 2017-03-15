using Raven.Client.Indexes;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Transformers
{
    public class Topics_Summary : AbstractTransformerCreationTask<Topic>
    {
		public const string Name = "Topics/Summary";

		public Topics_Summary()
		{
			TransformResults = entities => from entity in entities
										   where entity.Id != null
										   let image = LoadDocument<File>(entity.ImageIds.FirstOrDefault())
										   select new DenormalizedTopicSummary()
										   {
											   Id = entity.Id,
											   Permissions = entity.Permissions,
											   Name = entity.Name,
											   Description = entity.Description,
											   Image = TransformWith<File, DenormalizedFileSummary>(Files_Summary.Name, image).SingleOrDefault()
										   };
		}
	}
}
