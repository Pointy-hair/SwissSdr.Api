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
	public class DenormalizedTopic : IDenormalizedEntity, IHasDenormalizedImages
	{
		public Topic Entity { get; set; }
		public IEnumerable<DenormalizedFileSummary> ImageFiles { get; set; }

		public string Id => Entity.Id;
		public IDictionary<string, IEnumerable<ObjectPermission>> Permissions => Entity.Permissions;

		IHasImages IHasDenormalizedImages.Entity => Entity;
	}
}
