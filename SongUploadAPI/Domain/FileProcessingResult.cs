using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SongUploadAPI.Domain
{
    public class FileProcessingResult
    {
        public bool Succeeded { get; set; }
        public bool Failed => !Succeeded;
        public string ErrorMessage { get; set; }
        public byte[] FileBytes { get; set; }
    }
}
