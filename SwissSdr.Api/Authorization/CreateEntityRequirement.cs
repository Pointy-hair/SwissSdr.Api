using Microsoft.AspNetCore.Authorization;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Authorization
{
    public class CreateEntityRequirement : IAuthorizationRequirement
    {
		public EntityType Type { get; }

		public CreateEntityRequirement(EntityType type)
		{
			Type = type;
		}
    }
}
