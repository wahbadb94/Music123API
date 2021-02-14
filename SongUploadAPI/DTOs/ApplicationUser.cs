using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SongUploadAPI.DTOs
{
    public class ApplicationUser : IdentityUser
    {
        public List<SongDto> Songs { get; set; }
    }
}
