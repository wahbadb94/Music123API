using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SongUploadAPI.Data;
using SongUploadAPI.Domain;
using SongUploadAPI.Extensions;
using SongUploadAPI.Models;
using SongUploadAPI.Options;
using SongUploadAPI.Utilities;

using TryBindModelAsyncDelegate =
    System.Func<
        SongUploadAPI.Models.SongFormData,
        Microsoft.AspNetCore.Mvc.ModelBinding.FormValueProvider,
        System.Threading.Tasks.Task<bool>>;

namespace SongUploadAPI.Services
{
    public class SongsService : ISongsService
    {
        private readonly FormOptions _defaultFormOptions = new FormOptions();
        private readonly ApplicationDbContext _dbContext;
        private readonly IJobNotificationService _jobNotificationService;
        private readonly IAmsSongUploadService _uploadService;
        private readonly long _uploadFileSizeLimit;

        public SongsService(ApplicationDbContext dbContext,
            IJobNotificationService jobNotificationService,
            IOptions<UploadSettings> uploadSettings,
            IAmsSongUploadService uploadService)
        {
            _dbContext = dbContext;
            _jobNotificationService = jobNotificationService;
            _uploadService = uploadService;
            _uploadFileSizeLimit = uploadSettings.Value.FileSizeLimit;
        }

        public async Task<Result<Song>> CreateSongAsync(string userId, HttpRequest request,
            TryBindModelAsyncDelegate tryBindModelAsync)
        {
            // TODO: (de-clutter) move manual reading of request body to it's own method

            if (!request.IsMultiPartContentType()) return new Error("request is not of type \"multipart/form-data\"");

            // used for creating the resulting entity
            var streamingUrl = "";
            var songId = Guid.Empty;

            // manually read multipart/form-data one section at a time
            // each section is delimited by the 'boundary'
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, request.Body);
            var section = await reader.ReadNextSectionAsync();
            var formAccumulator = new KeyValueAccumulator();

            // begin reading
            await _jobNotificationService.NotifyUserJobStateChange(userId, JobState.Submitting);

            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (hasContentDispositionHeader == false) continue;

                if (contentDisposition.HasFileContent())
                {
                    // validate and process the upload stream
                    var fileProcessedResult = await FileHelpers.TryProcessFileAsync(section.Body,
                        contentDisposition.FileName.Value, _uploadFileSizeLimit);

                    if (fileProcessedResult.Failed) return new Error(fileProcessedResult.ErrorMessage);

                    // upload file to Azure Media Services
                    var fileSize = fileProcessedResult.FileBytes.Length;
                    var uploadStream = new MemoryStream(fileProcessedResult.FileBytes);

                    // event handler to send upload progress to client
                    void UploadProgressChanged(object sender, long bytesUploaded)
                    {
                        var percentage = (double) bytesUploaded / fileSize;
                        _jobNotificationService.NotifyUserUploadPercentageChange(userId, percentage);
                    }

                    var uploadResult = await _uploadService.Upload(userId, uploadStream, section.ContentType,
                        UploadProgressChanged);

                    if (uploadResult.Failed) return new Error(uploadResult.ErrorMessage);


                    songId = uploadResult.Id;
                    streamingUrl = uploadResult.SteamingUrl;
                }

                if (contentDisposition.HasFormDataContent())
                {
                    var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
                    var encoding = GetEncoding(section);

                    if (encoding == null) return new Error("Could not get encoding type");

                    using var streamReader = new StreamReader(section.Body, encoding, true, 1024, true);

                    var value = await streamReader.ReadToEndAsync();
                    if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                        value = string.Empty;
                    
                    formAccumulator.Append(key, value);

                    if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                    {
                        return new Error($"value count for form field {key}, exceeded maximum amount of values");
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            // bind form-data to model
            var formData = new SongFormData();
            var formValueProvider = new FormValueProvider(
                BindingSource.Form,
                new FormCollection(formAccumulator.GetResults()),
                CultureInfo.CurrentCulture);
            var bindingSuccessful = await tryBindModelAsync(formData, formValueProvider);

            if (bindingSuccessful == false) return new Error( $"could not map form-data to type {formData.GetType().Name}");

            var newSong = new Song()
            {
                Id = songId,
                Name = formData.Name,
                Artist = formData.Artist,
                Bpm = formData.Bpm,
                Key = formData.Key,
                StreamingUrl = streamingUrl,
                UserId = userId
            };

            try
            {
                await _dbContext.Songs.AddAsync(newSong);
                await _dbContext.SaveChangesAsync();
                return newSong;
            }
            catch (Exception e)
            {
                return new Error(e.Message);
            }
        }

        public Result<IList<Song>> GetAllSongs(string userId)
        {
            try
            {
               return _dbContext.Songs.Where(song => song.UserId == userId).ToList();
            }
            catch (Exception e)
            {
                return new Error(e.Message);
            }
        }

        public Task<Song> GetSongAsync(string userId, string songId)
        {
            throw new NotImplementedException();
        }

        public Task<Song> UpdateSongAsync(string userId, string songId)
        {
            throw new NotImplementedException();
        }

        public Task<Song> DeleteSongAsync(string userId, string songId)
        {
            throw new NotImplementedException();
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding)) return Encoding.UTF8;

            return mediaType.Encoding;
        }
    }
}
