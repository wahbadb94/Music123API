using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SongUploadAPI.Domain;

namespace SongUploadAPI.Services
{
    public class AmsSongUploadService : IAmsSongUploadService
    {
        private const string DashStreamingSuffix = "(format=mpd-time-csf)";
        private readonly IMediaService _mediaService;
        private readonly IJobNotificationService _jobNotificationService;

        public AmsSongUploadService(IMediaService mediaService, IJobNotificationService jobNotificationService)
        {
            _mediaService = mediaService;
            _jobNotificationService = jobNotificationService;
        }

        public async Task<Result<string>> Upload(string userId, Stream fileStream, string contentType, EventHandler<long> uploadProgressChanged)
        {
            await _mediaService.Initialize();

            // ensure unique asset name
            // this will also be returned as a unique identifier
            var uniqueness = Guid.NewGuid();
            var inputAssetName = $"{uniqueness:N}-input";
            var outputAssetName = $"{uniqueness:N}-output";
            var jobName = $"{uniqueness:N}-job";
            var locatorName = $"{uniqueness:N}-locator";

            var uploadProgressHandler = new Progress<long>();
            uploadProgressHandler.ProgressChanged += uploadProgressChanged;

            try
            {
                // upload assets
                await _jobNotificationService.NotifyUserJobStateChange(userId, JobState.Uploading);
                await _mediaService.CreateAndUploadInputAssetAsync(fileStream, inputAssetName, contentType,
                    uploadProgressHandler);
                var outputAsset = await _mediaService.CreateOutputAssetAsync(outputAssetName);

                // encode input asset (uncompressed wav) as a compressed 128AAC file and store in output asset
                await _jobNotificationService.NotifyUserJobStateChange(userId, JobState.Encoding);
                await _mediaService.SubmitJobAsync(inputAssetName, outputAssetName, jobName);

                // generate streaming locator
                await _jobNotificationService.NotifyUserJobStateChange(userId, JobState.Finalizing);
                var locator = await _mediaService.CreateStreamingLocatorAsync(locatorName, outputAsset.Name);
                var urls = await _mediaService.GetStreamingUrlsAsync(locator.Name);

                // need DASH compatible url, since our client application uses the DASH streaming protocol
                var dashUrl = urls.Where(url => url.EndsWith(DashStreamingSuffix)).ToList()[0];

                await _jobNotificationService.NotifyUserJobStateChange(userId, JobState.Finished);

                return dashUrl;

            }
            catch (Exception e)
            {
                return new Error(e.Message);
            }
        }
    }
}