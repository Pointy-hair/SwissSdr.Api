using Microsoft.AspNetCore.Mvc;
using SwissSdr.Api.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Endpoints
{
    public interface IHasJobsEndpoint
    {
		IJobsEndpoint JobsEndpoint { get; }

		Task<IActionResult> GetJobs(int id);
		Task<IActionResult> UpdateJobs(int id, JobsUpdateInputModel updateModel);
	}
}
