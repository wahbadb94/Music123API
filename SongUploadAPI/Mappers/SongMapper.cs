using System;
using SongUploadAPI.Contracts.Requests;
using SongUploadAPI.DTOs;

namespace SongUploadAPI.Mappers
{
    public static class SongMapper
    {
        public static SongDto SongFormDataToDto(string userId, SongFormData songFormData, string streamingUrl)
        {
            return new SongDto()
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

        public static void UpdateSongDtoFromFormData(SongDto oldData, SongFormData newData)
        {
            oldData.Artist = newData.Artist;
            oldData.Name = newData.Name;
            oldData.Bpm = newData.Bpm;
            oldData.Key = newData.Key;
        }
    }
}
