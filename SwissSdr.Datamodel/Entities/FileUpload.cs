using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public class FileUpload : EntityBase
	{
		public Guid UploadBlobId { get; set; }
		public string UploadUrl { get; set; }
		public string SasToken { get; set; }

		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
	}
}
