using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SwissSdr.Datamodel
{
	public class File : EntityBase
	{
		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }

		public string Extension { get; set; }
		public string MimeType { get; set; }
		public long Size { get; set; }
		public string Url { get; set; }

		public ICollection<object> Metadata { get; } = new List<object>();

		public string GetBlobName() => $"{Id.Replace("/", "-")}{Extension}";
	}

	public class ImageMetadata
	{
		public int Width { get; set; }
		public int Height { get; set; }
	}
}