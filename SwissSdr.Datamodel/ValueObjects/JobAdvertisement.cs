using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public class JobAdvertisement
	{
		public Multilingual<string> Name { get; set; }
		public Multilingual<Richtext> Content { get; set; }
		public string Function { get; set; }
		public string Url { get; set; }
	}
}
