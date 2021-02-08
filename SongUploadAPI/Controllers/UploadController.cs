using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SongUploadAPI.Filters;
using SongUploadAPI.Hubs;
using SongUploadAPI.Options;
using SongUploadAPI.Utilities;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.Management.Media.Models;
using SongUploadAPI.Services;
using Exception = System.Exception;

namespace SongUploadAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private static readonly FormOptions DefaultFormOptions = new FormOptions();

        private readonly string[] _permittedExtensions = {".wav"};
        private readonly Dictionary<string, List<byte[]>> _fileSignatures = new Dictionary<string, List<byte[]>>
        {
            {".wav", new List<byte[]> { new byte[] { 0x52, 0x49, 0x46, 0x46 } } },
        };
        private readonly long _fileSizeLimit;
        private readonly JobUpdateHub _jobUpdateHub;
        private readonly IMediaService _mediaService;
        private long _uploadFileSize;
        private string _currentUser;

        public UploadController(JobUpdateHub jobUpdateHub,
            IOptions<UploadSettings> uploadSettings,
            IMediaService mediaService)
        {
            //TODO: change to KestrelSettings:MaxFileSize
            _fileSizeLimit = uploadSettings.Value.FileSizeLimit;
            _jobUpdateHub = jobUpdateHub;
            _mediaService = mediaService;
        }

        [DisableFormValueModelBinding]
        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            _currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // make sure content is multipart-formdata
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File",
                    $"The request couldn't be processed (Error 1).");

                return BadRequest(ModelState);
            }

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                DefaultFormOptions.MultipartBoundaryLengthLimit);
            var formAccumulator = new KeyValueAccumulator();        // used foy key-val pairs, will need later
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();
            string streamingUrl;

            // begin reading
            await _jobUpdateHub.Clients.Group(_currentUser).SendAsync("jobStateChange", "submitting");
            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    // if content is file
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        var fileBytes = await ProcessFile(
                            section.Body,
                            ModelState,
                            contentDisposition.FileName.Value);

                        _uploadFileSize = fileBytes.Length;
                        Console.WriteLine(fileBytes.Length);
                        
                        if (ModelState.IsValid == false || fileBytes.Length == 0)
                        {
                            return BadRequest(ModelState);
                        }

                        try
                        {
                            var uploadStream = new MemoryStream(fileBytes);

                            streamingUrl = await UploadSongToAms(
                                uploadStream,
                                section.ContentType);

                            return Ok(streamingUrl);
                        }
                        catch (Exception e)
                        {
                            ModelState.AddModelError("AMS Upload Failed", e.Message);
                            return BadRequest(ModelState);
                        }
                    }

                    // will need formAccumulator here
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {

                    }
                }
                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            return Created(nameof(UploadController), null);
        }

        [HttpGet]
        public async Task WhoAmI()
        {
            _currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _jobUpdateHub.Clients.Group(_currentUser).SendAsync("receiveMessage", $"Hi there {_currentUser}!");
        }

        private async Task<byte[]> ProcessFile(Stream sectionBody, ModelStateDictionary modelState, string fileName)
        {
            if (sectionBody == null)
            {
                modelState.AddModelError("File", "The file stream does not exist");
            }
            else
            {
                await using var memoryStream = new MemoryStream();
                await sectionBody.CopyToAsync(memoryStream);

                if (memoryStream.Length == 0)
                {
                    modelState.AddModelError("File", "The file is empty");
                }
                else if (memoryStream.Length > _fileSizeLimit)
                {
                    var megabyteSizeLimit = _fileSizeLimit / 1048576;
                    modelState.AddModelError("File",
                        $"The file exceeds {megabyteSizeLimit:N1} MB.");
                }
                else if (IsValidExtension(memoryStream, fileName) == false)
                {
                    modelState.AddModelError("File", "This file type isn't permitted or the signature doesn't match the extenstion");
                }
                else
                {
                    return memoryStream.ToArray();
                }
            }

            return new byte[0];
        }

        private bool IsValidExtension(Stream dataStream, string fileName)
        {
            // check file name
            if (string.IsNullOrEmpty(fileName)) return false;

            // check file extenstion
            var extIndexOf = fileName.LastIndexOf('.');
            var ext = fileName.Substring(extIndexOf).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || (_permittedExtensions.Contains(ext) == false)) return false;

            //check file signature
            dataStream.Position = 0;
            var reader = new BinaryReader(dataStream);
            var signatures = _fileSignatures[ext];
            var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));
            var result = signatures.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature));
            return result;
        }

        private async Task<string> UploadSongToAms(Stream fileStream, string contentType)
        {
            await _mediaService.Initialize();

            // ensure unique asset name
            var uniqueName = $"{Guid.NewGuid():N}";
            var inputAssetName = $"{uniqueName}-input";
            var outputAssetName = $"{uniqueName}-output";
            var jobName = $"{uniqueName}-job";
            var locatorName = $"{uniqueName}-locator";

            var uploadProgressHandler = new Progress<long>();
            uploadProgressHandler.ProgressChanged += UploadProgressChanged;

            //TODO: run async operations concurrently. will be a fun "Before and After" comparison
            // begin asset upload
            await _jobUpdateHub.Clients.Group(_currentUser).SendAsync("jobStateChange", "uploading");
            await _mediaService.CreateAndUploadInputAssetAsync(fileStream, inputAssetName, contentType, uploadProgressHandler);
            var outputAsset = await _mediaService.CreateOutputAssetAsync(outputAssetName);

            // begin encoding, register user to receive updates on encoding job
            var userConnectionId = _jobUpdateHub.UserConnectionIds[_currentUser];
            await _jobUpdateHub.Groups.AddToGroupAsync(userConnectionId, jobName);
            await _jobUpdateHub.Clients.Group(_currentUser).SendAsync("jobStateChange", "encoding");
            await _mediaService.SubmitJobAsync(inputAssetName, outputAssetName, jobName);
            await _jobUpdateHub.Groups.RemoveFromGroupAsync(userConnectionId, jobName);

            // generate streaming locator, once job has finished
            await _jobUpdateHub.Clients.Group(_currentUser).SendAsync("jobStateChange", "finalizing");
            var locator = await _mediaService.CreateStreamingLocatorAsync(locatorName, outputAsset.Name);
            var urls = await _mediaService.GetStreamingUrlsAsync(locator.Name);
            await _jobUpdateHub.Clients.Group(_currentUser).SendAsync("jobStateChange", "finished");

            return urls[0];
        }

        private void UploadProgressChanged(object sender, long bytesUploaded)
        {
            var percentage = (double) bytesUploaded / _uploadFileSize;
            _jobUpdateHub.Clients.Group(_currentUser).SendAsync("uploadPercentageChange", percentage);
        }
    }
}