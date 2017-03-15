using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.QueryModels
{
    public interface IHasDenormalizedLibrary
	{
		IHasLibrary Entity { get; }
		IEnumerable<DenormalizedFileSummary> LibraryFiles { get; }
	}
}
