using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwissSdr.Datamodel.Authorization;

namespace SwissSdr.Api.QueryModels
{
	public class DenormalizedEvent : IDenormalizedEntity, IHasDenormalizedImages, IHasDenormalizedLibrary, IHasDenormalizedAssociations
	{
		public Event Entity { get; set; }
		public IEnumerable<DenormalizedFileSummary> ImageFiles { get; set; }
		public IEnumerable<DenormalizedFileSummary> LibraryFiles { get; set; }
		public IEnumerable<DenormalizedEntitySummary> AssociationTargets { get; set; }

		public string Id => Entity.Id;
		public IDictionary<string, IEnumerable<ObjectPermission>> Permissions => Entity.Permissions;

		IHasImages IHasDenormalizedImages.Entity => Entity;
		IHasLibrary IHasDenormalizedLibrary.Entity => Entity;
		IHasAssociations IHasDenormalizedAssociations.Entity => Entity;
	}
}
