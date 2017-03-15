using Microsoft.AspNetCore.Mvc;
using SwissSdr.Api.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Endpoints
{
    public interface IHasPermissionsEndpoint
    {
		IPermissionsEndpoint PermissionsEndpoint { get; }

		Task<IActionResult> GetPermissions(int id);
		Task<IActionResult> UpdatePermissions(int id, ObjectPermissionsUpdateInputModel updateModel);
	}
}
