using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Shared
{
    public static class SwissSdrConstants
	{
		public const string Authority = "https://accounts.swiss-sdr.ch";

		public static class Storage
		{
			public const string FileBlobContainerName = "files";
			public const string UploadBlobContainerName = "uploads";
		}
	}
}
