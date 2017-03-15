using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
    public class FileUploadResource : EntityResourceBase
	{
		public Guid UploadBlobId { get; set; }
		public string UploadUrl { get; set; }
		public string SasToken { get; set; }

		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
	}
}
