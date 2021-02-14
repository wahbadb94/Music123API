using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SongUploadAPI.Contracts.Requests;
using SongUploadAPI.Domain;
using SongUploadAPI.DTOs;

namespace SongUploadAPI.Services
{
    public interface ISongsService
    {
        public delegate Task<bool> TryBindModelAsync(SongFormData songFormData, FormValueProvider formValueProvider);

        public Task<Result<SongDto>> CreateSongAsync(string userId, HttpRequest request, TryBindModelAsync tryBindModelAsync);
        public Task<Result<IList<SongDto>>> GetAllSongsAsync(string userId);
        public Task<Result<SongDto>> GetSongAsync(string userId, string songId);
        public Task<Result<SongDto>> UpdateSongAsync(string userId, string songId, SongFormData updatedSongData);
        public Task<Result<SongDto>> DeleteSongAsync(string userId, string songId);
    }
}
