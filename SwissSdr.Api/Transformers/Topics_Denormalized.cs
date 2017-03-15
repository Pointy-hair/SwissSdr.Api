using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using SwissSdr.Datamodel;
using SwissSdr.Api.QueryModels;

namespace SwissSdr.Api.Transformers
{
	public class Topics_Denormalized : AbstractTransformerCreationTask<Topic>
	{
		public Topics_Denormalized()
		{
			TransformResults = topics => from topic in topics
										 where topic.Id != null
										 let imageFiles = LoadDocument<File>(topic.ImageIds.Distinct())
										 select new DenormalizedTopic
										 {
											 Entity = topic,
											 ImageFiles = TransformWith<File, DenormalizedFileSummary>(Files_Summary.Name, imageFiles)
										 };
		}
	}
}
