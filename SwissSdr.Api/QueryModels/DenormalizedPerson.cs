using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SwissSdr.Api.QueryModels
{
	public class DenormalizedPerson : IDenormalizedEntity, IHasDenormalizedPublications, IHasDenormalizedLibrary, IHasDenormalizedAssociations
	{
		public Person Entity { get; set; }
		public DenormalizedFileSummary ProfileImage { get; set; }
		public IEnumerable<DenormalizedFileSummary> LibraryFiles { get; set; }
		public IEnumerable<DenormalizedEntitySummary> AssociationTargets { get; set; }
		public IEnumerable<DenormalizedFileSummary> PublicationFiles { get; set; }

		public string Id => Entity.Id;
		public IDictionary<string, IEnumerable<ObjectPermission>> Permissions => Entity.Permissions;

		IHasPublications IHasDenormalizedPublications.Entity => Entity;
		IHasLibrary IHasDenormalizedLibrary.Entity => Entity;
		IHasAssociations IHasDenormalizedAssociations.Entity => Entity;
	}
}
