using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SongUploadAPI.Domain;
using SongUploadAPI.Models;
// complex delegate type alias to make methods more readable
using TryBindModelAsyncDelegate =
    System.Func<
        SongUploadAPI.Models.SongFormData,
        Microsoft.AspNetCore.Mvc.ModelBinding.FormValueProvider,
        System.Threading.Tasks.Task<bool>>;

namespace SongUploadAPI.Services
{
    public interface ISongsService
    {
        public Task<SongCreatedResult> CreateSongAsync(string userId, HttpRequest request, TryBindModelAsyncDelegate tryBindModelAsync);
        public IList<Song> GetAllSongs(string userId);
        public Task<Song> GetSongAsync(string userId, string songId);
        public Task<Song> UpdateSongAsync(string userId, string songId);
        public Task<Song> DeleteSongAsync(string userId, string songId);
    }
}
