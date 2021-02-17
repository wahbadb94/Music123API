using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SongUploadAPI.Contracts.Requests;
using SongUploadAPI.Data;
using SongUploadAPI.Domain;
using SongUploadAPI.DTOs;
using SongUploadAPI.Extensions;
using SongUploadAPI.Options;
using SongUploadAPI.Utilities;

namespace SongUploadAPI.Services
{
    public class SongsService : ISongsService
    {
        private readonly FormOptions _defaultFormOptions = new();
        private readonly ApplicationDbContext _dbContext;
        private readonly IJobNotificationService _jobNotificationService;
        private readonly IAmsSongUploadService _uploadService;
        private readonly long _uploadFileSizeLimit;
        private readonly IMapper _mapper;

        public SongsService(ApplicationDbContext dbContext,
            IJobNotificationService jobNotificationService,
            IOptions<UploadSettings> uploadSettings,
            IAmsSongUploadService uploadService,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _jobNotificationService = jobNotificationService;
            _uploadService = uploadService;
            _mapper = mapper;
            _uploadFileSizeLimit = uploadSettings.Value.FileSizeLimit;
        }

        public async Task<Result<SongDto>> CreateSongAsync(string userId, HttpRequest request,
            ISongsService.TryBindModelAsync tryBindModelAsync)
        {
            if (!request.IsMultiPartContentType()) return new Error("request is not of type \"multipart/form-data\"");

            var streamingUrl = "";

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
                    // validate and process the stream from the form
                    var fileProcessedResult = await FileHelpers.TryProcessFileAsync(section.Body,
                        contentDisposition.FileName.Value, _uploadFileSizeLimit);

                    if (fileProcessedResult.IsError) return fileProcessedResult.AsError;
                    
                    var fileBytes = fileProcessedResult.AsOk;
                    var fileSize = fileBytes.Length;
                    var uploadStream = new MemoryStream(fileBytes);

                    // event handler to send upload progress to client
                    void UploadProgressChanged(object sender, long bytesUploaded)
                    {
                        var percentage = (double) bytesUploaded / fileSize;
                        _jobNotificationService.NotifyUserUploadPercentageChange(userId, percentage);
                    }

                    // either returns the streamingURL after successfully uploading, or Error
                    var uploadResult = await _uploadService.Upload(userId, uploadStream, section.ContentType,
                        UploadProgressChanged);

                    if (uploadResult.IsError) return uploadResult.AsError;

                    streamingUrl = uploadResult.AsOk;
                }

                if (contentDisposition.HasFormDataContent())
                {
                    var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
                    var encoding = RequestHelpers.GetEncoding(section);

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
            var songFormData = new SongFormData();
            var formValueProvider = new FormValueProvider(
                BindingSource.Form,
                new FormCollection(formAccumulator.GetResults()),
                CultureInfo.CurrentCulture);
            var bindingSuccessful = await tryBindModelAsync(songFormData, formValueProvider);

            if (bindingSuccessful == false) return new Error( $"could not map form-data to type {songFormData.GetType().Name}");

            // weird Mapster syntax for mapping runtime values, don't really like these "magic strings"
            var newSong = _mapper.From(songFormData)
                .AddParameters("userId", userId)
                .AddParameters("streamingUrl", streamingUrl)
                .AdaptToType<SongDto>();

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

        public async Task<Result<IList<SongDto>>> GetAllSongsAsync(string userId)
        {
            try
            {
                return await _dbContext.Songs
                    .Where(song => song.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                return new Error(e.Message);
            }
        }

        public async Task<Result<SongDto>> GetSongAsync(string userId, string songId)
        {
            var result = await _dbContext.Songs
                .Where(song => song.UserId == userId)
                .FirstOrDefaultAsync(song => song.Id.ToString() == songId);

            if (result == null) return new Error("could not find song with that id");
            
            return result;
        }

        public async Task<Result<SongDto>> UpdateSongAsync(string userId, string songId, SongFormData updatedSongData) =>
            await (await GetSongAsync(userId, songId))
                .Match(
                    async foundSong =>
                    {
                        updatedSongData.Adapt(foundSong);
                        try
                        {
                            _dbContext.Songs.Update(foundSong);
                            await _dbContext.SaveChangesAsync();
                            return foundSong;
                        }
                        catch (Exception e)
                        {
                            return new Error(e.Message);
                        }
                    },
                    error => Task.FromResult<Result<SongDto>>(error));

        public async Task<Result<SongDto>> DeleteSongAsync(string userId, string songId) =>
            await (await GetSongAsync(userId, songId))
                .Match(
                    async foundSong =>
                    {
                        _dbContext.Songs.Remove(foundSong);
                        try
                        {
                            await _dbContext.SaveChangesAsync();
                            return foundSong;
                        }
                        catch (Exception e)
                        {
                            return new Error(e.Message);
                        }
                    },
                    err => Task.FromResult<Result<SongDto>>(err));
    }
}
