using Raven.Client.Listeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Json.Linq;
using SwissSdr.Datamodel;

namespace SwissSdr.Api.Infrastructure
{
	public class UpdatedAtListener : IDocumentStoreListener
	{
		public void AfterStore(string key, object entityInstance, RavenJObject metadata)
		{
		}

		public bool BeforeStore(string key, object entityInstance, RavenJObject metadata, RavenJObject original)
		{
			var entity = entityInstance as EntityBase;
			if (entity == null)
			{
				return false;
			}

			entity.UpdatedAt = DateTime.UtcNow;

			return true;
		}
	}
}
