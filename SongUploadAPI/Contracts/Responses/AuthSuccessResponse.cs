using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SongUploadAPI.Domain;

namespace SongUploadAPI.Contracts.Responses
{
    public class AuthSuccessResponse
    {
        [Required]
        public Token Token{ get; }
        public AuthSuccessResponse(Token token) => Token = token;

    }
}
