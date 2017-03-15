using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using SwissSdr.Datamodel;
using SwissSdr.Api.QueryModels;

namespace SwissSdr.Api.Transformers
{
	public class People_Denormalized : AbstractTransformerCreationTask<Person>
	{
		public People_Denormalized()
		{
			TransformResults = people => from person in people
										 where person.Id != null

										 let associations = LoadDocument<EntityBase>(person.Associations.Where(a => !string.IsNullOrEmpty(a.TargetId)).Select(a => a.TargetId).Distinct())
										 let imageFile = LoadDocument<File>(person.ProfileImageId)
										 let publicationFiles = LoadDocument<File>(person.Publications.Select(l => l.FileId).Distinct())
										 let libraryFiles = LoadDocument<File>(person.Library.Select(l => l.FileId).Distinct())

										 select new DenormalizedPerson
										 {
											 Entity = person,
											 ProfileImage = TransformWith<File, DenormalizedFileSummary>(Files_Summary.Name, imageFile).FirstOrDefault(),
											 LibraryFiles = TransformWith<File, DenormalizedFileSummary>(Files_Summary.Name, libraryFiles),
											 PublicationFiles = TransformWith<File, DenormalizedFileSummary>(Files_Summary.Name, publicationFiles),
											 AssociationTargets = TransformWith<EntityBase, DenormalizedEntitySummary>(EntityBase_Summary.Name, associations)
										 };
		}
	}
}
