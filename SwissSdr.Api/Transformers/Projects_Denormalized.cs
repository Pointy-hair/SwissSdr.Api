using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using SwissSdr.Datamodel;
using SwissSdr.Api.QueryModels;

namespace SwissSdr.Api.Transformers
{
    public class Projects_Denormalized : AbstractTransformerCreationTask<Project>
    {
		public Projects_Denormalized()
		{
			TransformResults = projects => from project in projects
										   where project.Id != null

										   let associations = LoadDocument<EntityBase>(project.Associations.Where(a => !string.IsNullOrEmpty(a.TargetId)).Select(a => a.TargetId).Distinct())
										   let parentProject = LoadDocument<Project>(project.ParentProjectId)
										   let partnerProjects = LoadDocument<Project>(project.PartnerProjectIds)
										   let imageFiles = LoadDocument<File>(project.ImageIds)
										   let publicationFiles = LoadDocument<File>(project.Publications.Select(l => l.FileId).Distinct())
										   let libraryFiles = LoadDocument<File>(project.Library.Select(l => l.FileId).Distinct())

										   select new DenormalizedProject
										   {
											   Entity = project,
											   ParentProject = TransformWith<Project, DenormalizedProjectSummary>(Projects_Summary.Name, parentProject).FirstOrDefault(),
											   PartnerProjects = TransformWith<IEnumerable<Project>, DenormalizedProjectSummary>(Projects_Summary.Name, partnerProjects),
											   ImageFiles = TransformWith<IEnumerable<File>, DenormalizedFileSummary>(Files_Summary.Name, imageFiles),
											   LibraryFiles = TransformWith<IEnumerable<File>, DenormalizedFileSummary>(Files_Summary.Name, libraryFiles),
											   PublicationFiles = TransformWith<IEnumerable<File>, DenormalizedFileSummary>(Files_Summary.Name, publicationFiles),
											   AssociationTargets = TransformWith<EntityBase, DenormalizedEntitySummary>(EntityBase_Summary.Name, associations)
										   };
		}
	}
}
