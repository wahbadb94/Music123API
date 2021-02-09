using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;
using Microsoft.AspNetCore.Identity;

namespace SongUploadAPI.Models
{
    public class Song
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Key { get; set; }
        public int Bpm { get; set; }
        public string StreamingUrl { get; set; }
        public string UserId { get; set; } // actual foreign key

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }
    }
}