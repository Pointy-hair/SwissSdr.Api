using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public interface IHasJobs
	{
		ICollection<JobAdvertisement> Jobs { get; set; }
	}
}
