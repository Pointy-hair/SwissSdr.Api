using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
	public static class ApiConstants
	{
		public const int DefaultPageSize = 30;

		public const string HalMediaType = "application/hal+json";
		public const string GeoJsonMediaType = "application/geo+json";

		public const string ApiScope = "swisssdr-api";

		public static class Routes
		{
			public const string Users = "v1/users";
			public const string Permissions = "{id:int}/permissions";

			public const string People = "v1/people";
			public const string Projects = "v1/projects";
			public const string Organisations = "v1/organisations";
			public const string Events = "v1/events";
			public const string Topics = "v1/topics";

			public const string Item = "{id:int}";
			public const string Associations = "{id:int}/associations";
			public const string Images = "{id:int}/images";
			public const string Jobs = "{id:int}/jobs";
			public const string Library = "{id:int}/library";
			public const string Publications = "{id:int}/publications";

			public const string EventSessions = "v1/events/{eventId:int}/sessions";
		}

		public static class Rels
		{
			public const string Associations = "associations";
			public const string Images = "images";
			public const string Jobs = "jobs";
			public const string Sessions = "sessions";
			public const string Library = "library";
			public const string Publications = "publications";
			public const string Permissions = "permisssions";
			public const string Speakers = "speakers";

			public const string Settings = "settings";
			public const string Events = "events";
			public const string Topics = "topics";
			public const string Projects = "projects";
			public const string People = "people";
			public const string Organisations = "organisations";
			public const string Files = "files";
			public const string Users = "users";
			public const string CurrentUser = "currentuser";

			public const string First = "first";
			public const string Prev = "prev";
			public const string Next = "next";
			public const string Last = "last";

			public const string Search = "search";
			public const string Tags = "tags";
		}

		public static class Embedded
		{
			public const string Items = "items";
			public const string User = "user";
			public const string Data = "data";
			public const string Image = "image";
			public const string ProfileImage = "profileImage";
			public const string ParentProject = "parentProject";
			public const string PartnerProjects = "partnerProjects";
		}
	}
}
