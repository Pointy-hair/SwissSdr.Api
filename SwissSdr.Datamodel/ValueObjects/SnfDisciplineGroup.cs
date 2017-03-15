using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
    public class SnfDisciplineGroup
	{
		public Multilingual<string> GroupName { get; set; }
		public ICollection<SnfDiscipline> Items { get; set; } = new Collection<SnfDiscipline>();
	}
}
