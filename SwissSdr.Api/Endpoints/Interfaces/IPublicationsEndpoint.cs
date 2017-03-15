using Microsoft.AspNetCore.Mvc;
using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;
using SwissSdr.Api.Resources;
using SwissSdr.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Endpoints
{
    public interface IPublicationsEndpoint
    {
		ItemsResource<LibraryResourceItem> CreatePublicationsResource(IHasDenormalizedPublications hasPublications);
	}
}
