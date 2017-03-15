using Halcyon.HAL;
using Microsoft.AspNetCore.Mvc;
using SwissSdr.Api.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public static class ResourceExtensions
    {
		public static HALResponse CreateRepresentation(this IResource resource)
		{
			return new HALResponse(resource);
		}

		public static HALResponse CreateRepresentation<TController>(this IResource resource, TController controller, Expression<Action<TController>> selfAction)
			where TController : ControllerBase
		{
			return new HALResponse(resource)
				.AddLinks(controller.CreateSelfLink(selfAction));
		}
		public static HALResponse CreateRepresentation<TController>(this IResource resource, TController controller, Expression<Func<TController, Task>> selfAction)
			where TController : ControllerBase
		{
			return new HALResponse(resource)
				.AddLinks(controller.CreateSelfLink(selfAction));
		}
	}
}
