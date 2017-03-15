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
	public class DenormalizedOrganisation : IDenormalizedEntity, IHasDenormalizedImages, IHasDenormalizedLibrary, IHasDenormalizedAssociations
	{
		public Organisation Entity { get; set; }
		public DenormalizedFileSummary ProfileImageFile { get; set; }
		public IEnumerable<DenormalizedFileSummary> ImageFiles { get; set; }
		public IEnumerable<DenormalizedFileSummary> LibraryFiles { get; internal set; }
		public IEnumerable<DenormalizedOrganisationSummary> RootOrganisation { get; internal set; }
		public IEnumerable<DenormalizedEntitySummary> AssociationTargets { get; set; }

		public string Id => Entity.Id;
		public IDictionary<string, IEnumerable<ObjectPermission>> Permissions => Entity.Permissions;

		IHasImages IHasDenormalizedImages.Entity => Entity;
		IHasLibrary IHasDenormalizedLibrary.Entity => Entity;
		IHasAssociations IHasDenormalizedAssociations.Entity => Entity;
	}
}
