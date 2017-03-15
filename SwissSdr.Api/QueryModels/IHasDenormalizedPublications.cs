using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.QueryModels
{
    public interface IHasDenormalizedPublications
    {
		IHasPublications Entity { get; }
		IEnumerable<DenormalizedFileSummary> PublicationFiles { get; }
	}
}
