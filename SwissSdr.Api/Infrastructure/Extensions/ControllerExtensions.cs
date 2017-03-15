using AutoMapper;
using Halcyon.HAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public static class ControllerExtensions
    {
		public static string Link<TController>(this TController controller, Expression<Action<TController>> action) where TController : ControllerBase
        {
            return controller.Url.Link(action, null);
		}
		public static string Link<TController>(this TController controller, Expression<Func<TController, Task>> action) where TController : ControllerBase
		{
			return controller.Url.Link(action, null);
		}
		
		public static string Link<TController>(this TController controller, Expression<Action<TController>> action, object values) where TController : ControllerBase
        {
            var request = controller.Url.ActionContext.HttpContext.Request;
            return controller.Url.Action(action, values, request.Scheme, request.Host.ToString());
		}
		public static string Link<TController>(this TController controller, Expression<Func<TController, Task>> action, object values) where TController : ControllerBase
		{
			var request = controller.Url.ActionContext.HttpContext.Request;
			return controller.Url.Action(action, values, request.Scheme, request.Host.ToString());
		}

		public static Link CreateSelfLink<TController>(this TController controller, Expression<Action<TController>> action) where TController : ControllerBase
        {
            return CreateSelfLink(controller, action, null);
		}
		public static Link CreateSelfLink<TController>(this TController controller, Expression<Func<TController, Task>> action) where TController : ControllerBase
		{
			return CreateSelfLink(controller, action, null);
		}
		public static Link CreateSelfLink<TController>(this TController controller, Expression<Action<TController>> action, object values) where TController : ControllerBase
        {
            return controller.Url.CreateSelfLink(action, values);
		}
		public static Link CreateSelfLink<TController>(this TController controller, Expression<Func<TController, Task>> action, object values) where TController : ControllerBase
		{
			return controller.Url.CreateSelfLink(action, values);
		}

		public static Link CreateLink<TController>(this TController controller, string rel, Expression<Action<TController>> action) where TController : ControllerBase
        {
            return CreateLink(controller, rel, action, null);
		}
		public static Link CreateLink<TController>(this TController controller, string rel, Expression<Func<TController, Task>> action) where TController : ControllerBase
		{
			return CreateLink(controller, rel, action, null);
		}
		public static Link CreateLink<TController>(this TController controller, string rel, Expression<Action<TController>> action, object values) where TController : ControllerBase
        {
            return controller.Url.CreateLink(rel, action, values);
		}
		public static Link CreateLink<TController>(this TController controller, string rel, Expression<Func<TController, Task>> action, object values) where TController : ControllerBase
		{
			return controller.Url.CreateLink(rel, action, values);
		}
	}
}
