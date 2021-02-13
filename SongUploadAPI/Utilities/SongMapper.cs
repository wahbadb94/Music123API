using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SongUploadAPI.Contracts.Requests;
using SongUploadAPI.Models;

namespace SongUploadAPI.Utilities
{
    public static class SongMapper
    {
        public static Song GetSongFromSongFormData(string userId, SongFormData songFormData, string streamingUrl)
        {
            return new Song()
            {
                Id = Guid.NewGuid(),
                Name = songFormData.Name,
                Artist = songFormData.Artist,
                Bpm = songFormData.Bpm,
                Key = songFormData.Key,
                StreamingUrl = streamingUrl,
                UserId = userId
            };
        }
    }
}
