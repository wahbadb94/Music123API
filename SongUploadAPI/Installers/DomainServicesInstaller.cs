using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SongUploadAPI.Extensions;
using SongUploadAPI.Hubs;
using SongUploadAPI.Options;
using SongUploadAPI.Services;

namespace SongUploadAPI.Installers
{
    public class DomainServicesInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(x => new BlobServiceClient(
                configuration.GetConnectionString("BlobStorageConnectionString")));

            services.AddSingleton<JobUpdateHub>();

            services.AddScoped<IJobNotificationService, JobNotificationService>();
            services.AddScoped<IMediaService, MediaService>();
            services.AddScoped<IAmsSongUploadService, AmsSongUploadService>();
            services.AddScoped<ISongsService, SongsService>();

            services.Configure<UploadSettings>(
                configuration.GetSection("UploadSettings"));

            services.Configure<MediaServiceSettings>(
                configuration.GetSection("MediaServiceSettings"));

            services.Configure<BlobStorageSettings>(
                configuration.GetSection("BlobStorageSettings"));

            services.AddSignalR();

            services.AddAzureClients(builder =>
            {
                builder.AddBlobServiceClient(configuration["ConnectionStrings:BlobStorageConnectionString:blob"], preferMsi: true);
                builder.AddQueueServiceClient(configuration["ConnectionStrings:BlobStorageConnectionString:queue"], preferMsi: true);
            });
        }
    }
}
