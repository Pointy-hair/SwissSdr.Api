using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.QueryModels
{
    public interface IHasDenormalizedAssociations
	{
		IHasAssociations Entity { get; }
		IEnumerable<DenormalizedEntitySummary> AssociationTargets { get; }
	}
}
