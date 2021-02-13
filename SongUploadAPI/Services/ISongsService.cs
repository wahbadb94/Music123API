using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SongUploadAPI.Contracts.Requests;
using SongUploadAPI.Domain;
using SongUploadAPI.Models;

namespace SongUploadAPI.Services
{
    public interface ISongsService
    {
        public delegate Task<bool> TryBindModelAsync(SongFormData songFormData, FormValueProvider formValueProvider);

        public Task<Result<Song>> CreateSongAsync(string userId, HttpRequest request, TryBindModelAsync tryBindModelAsync);
        public Result<IList<Song>> GetAllSongs(string userId);
        public Task<Song> GetSongAsync(string userId, string songId);
        public Task<Song> UpdateSongAsync(string userId, string songId);
        public Task<Song> DeleteSongAsync(string userId, string songId);

    }
}
