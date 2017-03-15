using SwissSdr.Api.QueryModels;
using SwissSdr.Api.Resources;
using SwissSdr.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Endpoints
{
	public interface IImagesEndpoint
	{
		ItemsResource<ImageResourceItem> CreateImageResource(IHasDenormalizedImages hasImages);
	}
}
