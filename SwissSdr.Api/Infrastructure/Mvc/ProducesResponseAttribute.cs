using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public class ProducesResponseAttribute : ProducesResponseTypeAttribute
	{
		public ProducesResponseAttribute(HttpStatusCode statusCode) : base(typeof(void), (int)statusCode) { }
		public ProducesResponseAttribute(Type type, HttpStatusCode statusCode) : base(type, (int)statusCode) { }
	}
}
