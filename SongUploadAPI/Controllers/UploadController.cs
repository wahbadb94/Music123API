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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SongUploadAPI.Services;
using Exception = System.Exception;

namespace SongUploadAPI.Controllers
{
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
        private readonly IHubContext<JobUpdateHub> _hubContext;
        private readonly IMediaService _mediaService;

        public UploadController(IHubContext<JobUpdateHub> hubContext,
            IOptions<UploadSettings> uploadSettings,
            IMediaService mediaService)
        {
            _fileSizeLimit = uploadSettings.Value.FileSizeLimit;
            _hubContext = hubContext;
            _mediaService = mediaService;
        }

        [DisableFormValueModelBinding]
        [HttpPost]
        public async Task<IActionResult> Upload()
        {
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
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                    {
                        ModelState.AddModelError("File",
                            $"The request couldn't be processed (Error 2).");
                        // Log error

                        return BadRequest(ModelState);
                    }

                    var fileBytes = await ProcessFile(
                        section.Body,
                        ModelState,
                        contentDisposition.FileName.Value);

                    if (ModelState.IsValid == false || fileBytes.Length == 0)
                    {
                        return BadRequest(ModelState);
                    }

                    try
                    {
                        var uploadStream = new MemoryStream(fileBytes);

                        await UploadSongToAms(
                            uploadStream,
                            contentDisposition.FileName.Value,
                            section.ContentType);

                        return Ok();
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("AMS Upload Failed", e.Message);
                        return BadRequest(ModelState);
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            return Created(nameof(UploadController), null);
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

        //TODO: fileLength parameter is possibly redundant, can most likely be obtained from fileStream fairly easily
        private async Task UploadSongToAms(Stream fileStream, string fileName, string contentType)
        {
            await _mediaService.Initialize();

            // ensure unique asset name
            var uniqueName = $"{Guid.NewGuid():N}";
            var inputAssetName = $"{uniqueName}-input";
            var outputAssetName = $"{uniqueName}-output";
            var jobName = $"{uniqueName}-job";

            //TODO: run async operations concurrently. will be a fun "Before and After" comparison
            await _mediaService.CreateAndUploadInputAssetAsync(fileStream, inputAssetName, contentType);
            await _mediaService.CreateOutputAssetAsync(outputAssetName);
            await _mediaService.SubmitJobAsync(inputAssetName, outputAssetName, jobName);

            Console.WriteLine("Job Submitted");
            
        }


    }
}