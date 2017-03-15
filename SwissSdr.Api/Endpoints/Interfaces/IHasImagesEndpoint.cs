using Microsoft.AspNetCore.Mvc;
using SwissSdr.Api.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Endpoints
{
    public interface IHasImagesEndpoint
    {
		IImagesEndpoint ImagesEndpoint { get; }

		Task<IActionResult> GetImages(int id);
		Task<IActionResult> UpdateImages(int id, ImagesUpdateInputModel updateModel);
	}
}
