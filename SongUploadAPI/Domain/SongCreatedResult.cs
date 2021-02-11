using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SongUploadAPI.Models;

namespace SongUploadAPI.Domain
{
    public class SongCreatedResult
    {
        public bool Succeeded { get; set; }
        public bool Failed => !Succeeded;
        public string ErrorMessage { get; set; }
        public Song Song { get; set; }
    }
}
