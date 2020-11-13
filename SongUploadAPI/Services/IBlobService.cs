using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using BlobInfo = SongUploadAPI.Models.BlobInfo;

namespace SongUploadAPI.Services
{
    public interface IBlobService
    {
        public Task<BlobInfo> GetBlobAsync(string blobName);
        public Task<IEnumerable<string>> ListBlobNamesAsync();
        public Task UploadFileBlobAsync(string filePath, string fileName);
        public Task<BlobContentInfo> UploadContentBlobAsync(Stream content, string fileName, string contentType);
        public Task DeleteBlobAsync(string blobName);
    }
}
