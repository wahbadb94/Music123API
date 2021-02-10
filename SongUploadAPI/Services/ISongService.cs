using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SongUploadAPI.Models;

namespace SongUploadAPI.Services
{
    public interface ISongService
    {
        public Task<bool> GetAllSongs();

    }
}
