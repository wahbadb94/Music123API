using System;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SongUploadAPI.Contracts.Requests;
using SongUploadAPI.DTOs;

namespace SongUploadAPI.Installers
{
    public class MapsterInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var mapper = new Mapper(GetTypeAdapterConfig());

            services.AddSingleton<IMapper>(mapper);
        }

        private static TypeAdapterConfig GetTypeAdapterConfig()
        {
            var config = new TypeAdapterConfig();

            config.NewConfig<SongFormData, SongDto>()
                .Map(song => song.Id, _ => Guid.NewGuid())
                .Map(dest => dest.UserId,
                    src => MapContext.Current.Parameters["userId"])
                .Map(dest => dest.StreamingUrl,
                    src => MapContext.Current.Parameters["streamingUrl"]);

            return config;
        }
    }
}
