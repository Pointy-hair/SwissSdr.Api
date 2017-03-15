using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
    public interface IHasPublications
    {
		IList<LibraryItem> Publications { get; set; }
	}
}
