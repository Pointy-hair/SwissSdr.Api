using Microsoft.AspNetCore.Mvc;
using Raven.Client;
using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;
using SwissSdr.Api.Resources;
using SwissSdr.Api.Services;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Endpoints
{
    public interface IAssociationsEndpoint
    {
		ItemsResource<AssociationResourceItem> CreateAssociationResource(IHasDenormalizedAssociations hasAssociations, EntityType sourceType);
	}
}
