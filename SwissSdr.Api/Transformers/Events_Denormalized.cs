using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using SwissSdr.Datamodel;
using SwissSdr.Api.QueryModels;

namespace SwissSdr.Api.Transformers
{
	public class Events_Denormalized : AbstractTransformerCreationTask<Event>
	{
		public Events_Denormalized()
		{
			TransformResults = events => from evnt in events
										 where evnt.Id != null

										 let associations = LoadDocument<EntityBase>(evnt.Associations.Where(a => !string.IsNullOrEmpty(a.TargetId)).Select(a => a.TargetId).Distinct())
										 let imageFiles = LoadDocument<File>(evnt.ImageIds.Distinct())
										 let libraryFiles = LoadDocument<File>(evnt.Library.Select(l => l.FileId).Distinct())

										 select new DenormalizedEvent
										 {
											 Entity = evnt,
											 ImageFiles = TransformWith<File, DenormalizedFileSummary>(Files_Summary.Name, imageFiles),
											 LibraryFiles = TransformWith<File, DenormalizedFileSummary>(Files_Summary.Name, libraryFiles),
											 AssociationTargets = TransformWith<EntityBase, DenormalizedEntitySummary>(EntityBase_Summary.Name, associations)
										 };
		}
	}
}
