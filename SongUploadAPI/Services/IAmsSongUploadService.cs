using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SongUploadAPI.Domain;

namespace SongUploadAPI.Services
{
    public interface IAmsSongUploadService
    {
        public Task<AmsSongUploadResult> Upload(string userId, Stream fileStream, string contentType, EventHandler<long> uploadProgressChanged);
    }
}
