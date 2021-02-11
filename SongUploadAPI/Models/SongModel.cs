using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SongUploadAPI.Models
{
    public class SongModel
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Key { get; set; }
        public int Bpm { get; set; }
        public string StreamingUrl { get; set; }
        public string UserId { get; set; }
    }
}
