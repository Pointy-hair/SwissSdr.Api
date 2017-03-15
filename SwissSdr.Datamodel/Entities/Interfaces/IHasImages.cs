using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
    public interface IHasImages
    {
		IList<string> ImageIds { get; set; }
	}
}
