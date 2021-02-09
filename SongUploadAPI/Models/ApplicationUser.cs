using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SongUploadAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<Song> Songs { get; set; }
    }
}
