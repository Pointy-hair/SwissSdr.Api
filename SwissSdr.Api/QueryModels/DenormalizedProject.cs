using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Api.QueryModels
{
	public class DenormalizedProject : IDenormalizedEntity, IHasDenormalizedImages, IHasDenormalizedPublications, IHasDenormalizedLibrary, IHasDenormalizedAssociations
	{
		public Project Entity { get; set; }
		public IEnumerable<DenormalizedFileSummary> LibraryFiles { get; set; }
		public DenormalizedProjectSummary ParentProject { get; set; }
		public IEnumerable<DenormalizedProjectSummary> PartnerProjects { get; set; }
		public IEnumerable<DenormalizedEntitySummary> AssociationTargets { get; set; }
		public IEnumerable<DenormalizedFileSummary> ImageFiles { get; set; }
		public IEnumerable<DenormalizedFileSummary> PublicationFiles { get; set; }

		public string Id => Entity.Id;
		public IDictionary<string, IEnumerable<ObjectPermission>> Permissions => Entity.Permissions;

		IHasPublications IHasDenormalizedPublications.Entity => Entity;
		IHasLibrary IHasDenormalizedLibrary.Entity => Entity;
		IHasAssociations IHasDenormalizedAssociations.Entity => Entity;
		IHasImages IHasDenormalizedImages.Entity => Entity;
	}
}
