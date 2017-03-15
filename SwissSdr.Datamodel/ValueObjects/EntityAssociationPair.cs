using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel.Settings
{
    public class EntityAssociationPair: IEquatable<EntityAssociationPair>
    {
		public EntityType Source { get; set; }
		public EntityType Target { get; set; }
		public int Index { get; set; }

		public EntityAssociationPair(EntityType source, EntityType target, int index)
		{
			Source = source;
			Target = target;
			Index = index;
		}

		public override int GetHashCode()
		{
			return Source.GetHashCode() ^ Target.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var pair = obj as EntityAssociationPair;
			return Equals(pair);
		}

		public bool Equals(EntityAssociationPair other)
		{
			return Source == other.Source
				&& Target == other.Target;
		}
	}
}
