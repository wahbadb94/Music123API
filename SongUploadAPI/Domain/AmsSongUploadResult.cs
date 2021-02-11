using System;

namespace SongUploadAPI.Domain
{
    public class AmsSongUploadResult
    {
        public bool Succeeded { get; set; }
        public bool Failed => !Succeeded;
        public Guid Id { get; set; }
        public string SteamingUrl { get; set; }
        public string ErrorMessage { get; set; }
    }
}