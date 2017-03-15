using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Infrastructure
{
    // http://www.strathweb.com/2016/09/required-query-string-parameters-in-asp-net-core-mvc/
    public class RequiredFromQueryActionConstraint : IActionConstraint
    {
        private readonly string _parameter;

        public RequiredFromQueryActionConstraint(string parameter)
        {
            _parameter = parameter;
        }

        public int Order => 999;

        public bool Accept(ActionConstraintContext context)
        {
            if (!context.RouteContext.HttpContext.Request.Query.ContainsKey(_parameter))
            {
                return false;
            }

            return true;
        }
    }
}
