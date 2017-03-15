using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
    public class JobResourceItem
    {
		public Multilingual<string> Name { get; set; }
		public Multilingual<Richtext> Content { get; set; }
		public string Function { get; set; }
		public string Url { get; set; }
	}
}
