using System.ComponentModel.DataAnnotations;

namespace SongUploadAPI.Contracts.Requests
{
    public class UserLoginRequest
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
