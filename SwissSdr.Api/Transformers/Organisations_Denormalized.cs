using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using SwissSdr.Datamodel;
using SwissSdr.Api.QueryModels;

namespace SwissSdr.Api.Transformers
{
	public class Organisations_Denormalized : AbstractTransformerCreationTask<Organisation>
	{
		public Organisations_Denormalized()
		{
			TransformResults = organisations => from organisation in organisations
												where organisation.Id != null

												let associations = LoadDocument<EntityBase>(organisation.Associations.Where(a => !string.IsNullOrEmpty(a.TargetId)).Select(a => a.TargetId).Distinct())
												let profileImageFile = LoadDocument<File>(organisation.ProfileImageId)
												let imageFiles = LoadDocument<File>(organisation.ImageIds.Distinct())
												let libraryFiles = LoadDocument<File>(organisation.Library.Select(l => l.FileId).Distinct())
												let rootOrganisation = LoadDocument<Organisation>(organisation.RootOrganisationId)
												
												select new DenormalizedOrganisation
												{
													Entity = organisation,
													ProfileImageFile = TransformWith<File, DenormalizedFileSummary>(Files_Summary.Name, profileImageFile).FirstOrDefault(),
													ImageFiles = TransformWith<File, DenormalizedFileSummary>(Files_Summary.Name, imageFiles),
													LibraryFiles = TransformWith<File, DenormalizedFileSummary>(Files_Summary.Name, libraryFiles),
													RootOrganisation = TransformWith<Organisation, DenormalizedOrganisationSummary>(Organisations_Summary.Name, rootOrganisation),
													AssociationTargets = TransformWith<EntityBase, DenormalizedEntitySummary>(EntityBase_Summary.Name, associations)
												};
		}
	}
}
