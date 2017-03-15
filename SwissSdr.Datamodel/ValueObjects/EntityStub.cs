using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
    public class EntityStub
    {		
		public EntityType Type { get; set; }
		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
		public string Url { get; set; }
    }
}
