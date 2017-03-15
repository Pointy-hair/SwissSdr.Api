using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public class User
	{
		public string Id { get; set; }

		public Gender Gender { get; set; }
		public string Title { get; set; }
		public string Fullname { get; set; }
		public string EMail { get; set; }

		public string ProfileImageId { get; set; }
		public string ProfileImageUrl { get; set; }

		public ICollection<RavenClaim> Claims { get; set; } = new Collection<RavenClaim>();

		public ICollection<RavenClaim> ExternalClaims { get; set; } = new Collection<RavenClaim>();

		public ICollection<UserLogin> Logins { get; set; } = new Collection<UserLogin>();

		public ICollection<object> Settings { get; set; } = new Collection<object>();
	}

	public enum Gender
	{
		Male,
		Female
	}
}
