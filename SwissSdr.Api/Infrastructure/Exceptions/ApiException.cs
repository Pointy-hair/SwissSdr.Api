using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace SwissSdr.Api
{	
	public class ApiException : Exception
	{
		public ApiException() { }
		public ApiException(string message) : base(message) { }
		public ApiException(string message, Exception inner) : base(message, inner) { }
	}
}
