using System.ComponentModel.DataAnnotations;
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
