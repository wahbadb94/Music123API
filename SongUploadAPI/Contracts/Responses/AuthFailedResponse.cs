using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SongUploadAPI.Contracts.Responses
{
    public class AuthFailedResponse
    {
        [Required]
        public IEnumerable<string> Errors { get; set; }
    }
}
