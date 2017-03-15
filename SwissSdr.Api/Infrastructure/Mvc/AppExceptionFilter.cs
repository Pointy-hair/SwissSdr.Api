using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Raven.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
	public class AppExceptionFilter : IExceptionFilter
	{

		public void OnException(ExceptionContext context)
		{
			if (!context.ExceptionHandled)
			{
				if (context.Exception is ApiException)
				{
					var apiException = context.Exception as ApiException;
					context.Result = new BadRequestObjectResult(new { Error = apiException.Message });
				}
				else if (context.Exception is ObjectNotFoundException)
				{
					context.Result = new NotFoundResult();
				}
				else if (context.Exception is ConcurrencyException)
				{
					context.Result = new StatusCodeResult((int)HttpStatusCode.Conflict);
				}
			}
		}
	}
}
