using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SongUploadAPI.Domain;

namespace SongUploadAPI.Contracts.Responses
{
    public class AuthFailedResponse
    {
        [Required]
        public Error Error { get; }
        public AuthFailedResponse(Error error) => Error = error;
    }
}
