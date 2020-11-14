using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Management.Media.Models;

namespace SongUploadAPI.Services
{
    public interface IMediaService
    {
        public Task Initialize();

        public Task<Asset> CreateAndUploadInputAssetAsync(Stream fileStream, string assetName, string contentType);
        public Task<Asset> CreateOutputAssetAsync(string assetName);
        public Task<Job> SubmitJobAsync(string inputAssetName, string outputAssetName, string jobName);
        public Task<StreamingLocator> CreateStreamingLocatorAsync(string streamingLocatorName, string assetName);
        public Task<IList<string>> GetStreamingUrlsAsync(string locatorName);
    }
}
