using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SongUploadAPI.Options
{
    public class BlobStorageSettings
    {
        public string InputAssetContainerName { get; set; }
        public string OutputAssetContainerName { get; set; }
    }
}
