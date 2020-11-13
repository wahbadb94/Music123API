using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using SongUploadAPI.Options;
using BlobInfo = SongUploadAPI.Models.BlobInfo;

namespace SongUploadAPI.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobStorageSettings _blobStorageSettings;

        public BlobService(BlobServiceClient blobServiceClient, IOptions<BlobStorageSettings> blobStorageSettings)
        {
            _blobServiceClient = blobServiceClient;
            _blobStorageSettings = blobStorageSettings.Value;
        }
        
        public async Task<BlobInfo> GetBlobAsync(string blobName)
        {
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_blobStorageSettings.InputAssetContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            var blobDownloadInfo = await blobClient.DownloadAsync();

            return new BlobInfo(blobDownloadInfo.Value.Content, blobDownloadInfo.Value.ContentType);


        }

        public async Task<IEnumerable<string>> ListBlobNamesAsync()
        {
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_blobStorageSettings.InputAssetContainerName);

            var items = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                items.Add(blobItem.Name);
            }

            return items;
        }

        public async Task UploadFileBlobAsync(string filePath, string fileName)
        {
            throw new NotImplementedException();
        }

        public async Task<BlobContentInfo> UploadContentBlobAsync(Stream content, string fileName, string contentType)
        {
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_blobStorageSettings.InputAssetContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.UploadAsync(content, new BlobHttpHeaders() {ContentType = contentType});
            return response.Value;
        }

        public Task DeleteBlobAsync(string blobName)
        {
            throw new NotImplementedException();
        }
    }
}