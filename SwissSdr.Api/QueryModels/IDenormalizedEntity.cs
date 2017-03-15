using SwissSdr.Datamodel.Authorization;
using System.Collections.Generic;

namespace SwissSdr.Api.QueryModels
{
	public interface IDenormalizedEntity
	{
		string Id { get; }

		IDictionary<string, IEnumerable<ObjectPermission>> Permissions { get; }
	}
}
