using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.InputModels
{
    public class UserUpdateInputModel
	{
		public Gender Gender { get; set; }
		public string Title { get; set; }
		public string Fullname { get; set; }
		public string EMail { get; set; }

		public string ProfileImageId { get; set; }

		public IEnumerable<RavenClaim> Claims { get; set; }

		public IEnumerable<object> Settings { get; set; }
	}
}
