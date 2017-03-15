
using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;

namespace SwissSdr.Api.QueryModels
{
	public interface IDenormalizedEntitySummary
	{
		string Id { get; set; }

		EntityType EntityType { get; }

		IDictionary<string, IEnumerable<ObjectPermission>> Permissions { get; set; }

		DenormalizedFileSummary GetImage();
	}
}
