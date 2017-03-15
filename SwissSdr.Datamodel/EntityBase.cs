using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public abstract class EntityBase
	{
		public string Id { get; set; }

		public DateTime UpdatedAt { get; set; }
		public DateTime CreatedAt { get; set; }

		public IDictionary<string, IEnumerable<ObjectPermission>> Permissions { get; set; } = new Dictionary<string, IEnumerable<ObjectPermission>>();
	}
}
