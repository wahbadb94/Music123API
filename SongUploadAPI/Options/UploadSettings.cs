using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SongUploadAPI.Options
{
    public class UploadSettings
    {
        public long FileSizeLimit { get; set; }
        public string StoredFilesPath { get; set; }
    }
}
