using Microsoft.AspNetCore.Authorization;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Authorization
{
    public static class Policies
    {
		public const string UserAdministration = "UserAdministration";

		public const string CreatePerson = "CreatePerson";
		public const string CreateProject = "CreateProject";
		public const string CreateOrganisation = "CreateOrganisation";
		public const string CreateEvent = "CreateEvent";
		public const string CreateTopic = "CreateTopic";

		public const string Authenticated = "Authenticated";

		public static void CreatePolicies(AuthorizationOptions options)
		{
			options.AddPolicy(UserAdministration, builder => builder.RequireClaim(ClaimTypes.AdministerUsers));

			options.AddPolicy(CreatePerson, builder => builder.AddRequirements(new CreateEntityRequirement(EntityType.Person)));
			options.AddPolicy(CreateProject, builder => builder.AddRequirements(new CreateEntityRequirement(EntityType.Project)));
			options.AddPolicy(CreateOrganisation, builder => builder.AddRequirements(new CreateEntityRequirement(EntityType.Organisation)));
			options.AddPolicy(CreateEvent, builder => builder.AddRequirements(new CreateEntityRequirement(EntityType.Event)));
			options.AddPolicy(CreateTopic, builder => builder.AddRequirements(new CreateEntityRequirement(EntityType.Topic)));

			options.AddPolicy(Authenticated, builder => builder.RequireAuthenticatedUser());
		}
    }
}
