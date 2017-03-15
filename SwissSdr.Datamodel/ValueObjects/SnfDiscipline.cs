using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public class SnfDiscipline
	{
		public int? Group { get; set; }
		public int? Subgroup { get; set; }
		public int? Discipline { get; set; }

		public Multilingual<string> Name { get; set; }
	}
}
