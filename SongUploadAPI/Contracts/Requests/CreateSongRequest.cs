using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SongUploadAPI.Models
{
    public class CreateSongRequest : SongFormData
    {
        public IFormFile File { get; set; }
    }

    public class SongFormData
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Key { get; set; }
        public int Bpm { get; set; }
    }
}
