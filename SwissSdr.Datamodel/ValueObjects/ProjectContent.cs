using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel.Values
{
    public class ProjectContent
    {
		public Multilingual<Richtext> Basic { get; set; } = new Multilingual<Richtext>();
		public Multilingual<Richtext> Extended { get; set; } = new Multilingual<Richtext>();
	}
}
