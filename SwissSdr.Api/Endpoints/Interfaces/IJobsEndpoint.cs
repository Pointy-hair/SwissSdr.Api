using SwissSdr.Api.QueryModels;
using SwissSdr.Api.Resources;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Endpoints
{
    public interface IJobsEndpoint
    {
		ItemsResource<JobResourceItem> CreateJobsResource(IHasJobs hasJobs);
	}
}
