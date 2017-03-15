using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
    public interface IHasLibrary
    {
		IList<LibraryItem> Library { get; set; }
	}
}
