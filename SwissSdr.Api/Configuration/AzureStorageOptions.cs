using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Configuration
{
    public class AzureStorageOptions
    {
        public const string Name = "AzureStorage";

        public string AccountName { get; set; }
        public Uri BlobServiceEndpoint { get; set; }
        public string ApiKey { get; set; }
    }
}
