using System;
using System.IO;
using System.Threading.Tasks;
using SongUploadAPI.Domain;

namespace SongUploadAPI.Services
{
    public interface IAmsSongUploadService
    {
        public Task<Result<string>> Upload(string userId, Stream fileStream, string contentType, EventHandler<long> uploadProgressChanged);
    }
}
