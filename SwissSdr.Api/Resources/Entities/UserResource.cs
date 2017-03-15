using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
	public class UserResource : IResource
	{
		public string Id { get; set; }

		public Gender Gender { get; set; }
		public string Title { get; set; }
		public string Fullname { get; set; }
		public string EMail { get; set; }
		public string ProfileImageId { get; set; }

		public ICollection<RavenClaim> Claims { get; set; } = new Collection<RavenClaim>();

		public ICollection<UserLoginResourceItem> Logins { get; set; } = new Collection<UserLoginResourceItem>();

	}
}
