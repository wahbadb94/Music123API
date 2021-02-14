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
        public Task<Result<IList<Song>>> GetAllSongsAsync(string userId);
        public Task<Result<Song>> GetSongAsync(string userId, string songId);
        public Task<Result<Song>> UpdateSongAsync(string userId, string songId, SongFormData updatedSongData);
        public Task<Result<Song>> DeleteSongAsync(string userId, string songId);
    }
}
