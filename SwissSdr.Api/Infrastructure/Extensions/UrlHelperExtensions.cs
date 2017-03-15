using Halcyon.HAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using SwissSdr.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Generates an absolute URL using the specified route name and <see cref="Expression{TDelegate}"/> for an action method,
        /// from which action name, controller name and route values are resolved.
        /// </summary>
        /// <typeparam name="TController">Controller, from which the action is specified.</typeparam>
        /// <param name="action">
        /// The <see cref="Expression{TDelegate}"/>, from which action name, 
        /// controller name and route values are resolved.
        /// </param>
        /// <returns>The generated absolute URL.</returns>
        public static string Link<TController>(this IUrlHelper helper, Expression<Action<TController>> action) where TController : class
		{
            return helper.Link(action, null);
		}
		public static string Link<TController>(this IUrlHelper helper, Expression<Func<TController, Task>> action) where TController : class
		{
			return helper.Link(action, null);
		}

		/// <summary>
		/// Generates an absolute URL using the specified route name, <see cref="Expression{TDelegate}"/> for an action method,
		/// from which action name, controller name and route values are resolved, and the specified additional route values.
		/// </summary>
		/// <typeparam name="TController">Controller, from which the action is specified.</typeparam>
		/// <param name="action">
		/// The <see cref="Expression{TDelegate}"/>, from which action name, 
		/// controller name and route values are resolved.
		/// </param>
		/// <param name="values">An object that contains additional route values.</param>
		/// <returns>The generated absolute URL.</returns>
		public static string Link<TController>(this IUrlHelper helper, Expression<Action<TController>> action, object values) where TController : class
        {
            var request = helper.ActionContext.HttpContext.Request;

            return helper.Action(action, values, request.Scheme, request.Host.ToString());
		}
		public static string Link<TController>(this IUrlHelper helper, Expression<Func<TController, Task>> action, object values) where TController : class
		{
			var request = helper.ActionContext.HttpContext.Request;

			return helper.Action(action, values, request.Scheme, request.Host.ToString());
		}


		public static Link CreateSelfLink<TController>(this IUrlHelper helper, Expression<Action<TController>> action, string title = null, string method = null, bool replaceParameters = true, bool isRelArray = false) where TController : class
		{
            return CreateSelfLink(helper, action, null, title, method, replaceParameters, isRelArray);
		}
		public static Link CreateSelfLink<TController>(this IUrlHelper helper, Expression<Func<TController, Task>> action, string title = null, string method = null, bool replaceParameters = true, bool isRelArray = false) where TController : class
		{
			return CreateSelfLink(helper, action, null, title, method, replaceParameters, isRelArray);
		}
		public static Link CreateSelfLink<TController>(this IUrlHelper helper, Expression<Action<TController>> action, object values, string title = null, string method = null, bool replaceParameters = true, bool isRelArray = false) where TController : class
		{
			return CreateLink(helper, Halcyon.HAL.Link.RelForSelf, action, title, method, replaceParameters, isRelArray);
		}
		public static Link CreateSelfLink<TController>(this IUrlHelper helper, Expression<Func<TController, Task>> action, object values, string title = null, string method = null, bool replaceParameters = true, bool isRelArray = false) where TController : class
		{
			return CreateLink(helper, Halcyon.HAL.Link.RelForSelf, action, title, method, replaceParameters, isRelArray);
		}

		public static Link CreateLink<TController>(this IUrlHelper helper, string rel, Expression<Action<TController>> action, string title = null, string method = null, bool replaceParameters = true, bool isRelArray = false) where TController : class
		{
			return CreateLink(helper, rel, action, null, title, method, replaceParameters, isRelArray);
		}
		public static Link CreateLink<TController>(this IUrlHelper helper, string rel, Expression<Func<TController, Task>> action, string title = null, string method = null, bool replaceParameters = true, bool isRelArray = false) where TController : class
		{
			return CreateLink(helper, rel, action, null, title, method, replaceParameters, isRelArray);
		}
		public static Link CreateLink<TController>(this IUrlHelper helper, string rel, Expression<Action<TController>> action, object values, string title = null, string method = null, bool replaceParameters = true, bool isRelArray = false) where TController : class
		{
			var href = helper.Link(action, values);
			return new Link(rel, href, title, method, replaceParameters, isRelArray);
		}
		public static Link CreateLink<TController>(this IUrlHelper helper, string rel, Expression<Func<TController, Task>> action, object values, string title = null, string method = null, bool replaceParameters = true, bool isRelArray = false) where TController : class
		{
			var href = helper.Link(action, values);
			return new Link(rel, href, title, method, replaceParameters, isRelArray);
		}
	}
}
