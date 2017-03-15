using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public static class BlobExtensions
    {
		public static bool IsMediaContentType(this CloudBlob blob)
		{
			return blob.Properties.ContentType.StartsWith("images/")
				|| blob.Properties.ContentType.StartsWith("audio/")
				|| blob.Properties.ContentType.StartsWith("video/");
		}
    }
}
