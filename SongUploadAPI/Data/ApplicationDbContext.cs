using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SongUploadAPI.Controllers;
using SongUploadAPI.DTOs;

namespace SongUploadAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        // DbSet<ApplicationUser> inherited
        public DbSet<SongDto> Songs { get; set; }
    }
}
