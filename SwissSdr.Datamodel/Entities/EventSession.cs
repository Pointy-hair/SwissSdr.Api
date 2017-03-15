using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public class EventSession : EntityBase
	{
		public Multilingual<string> Name { get; set; }
		public Multilingual<Richtext> Content { get; set; }

		public string Venue { get; set; }
		public DateTime Begin { get; set; }
		public DateTime End { get; set; }

		public static string GetId(int eventId, int sessionId)
		{
			return $"events/{eventId}/sessions/{sessionId}";
		}

		public static string GetPartialId(int eventId)
		{
			return $"events/{eventId}/sessions/";
		}
	}
}
