using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SongUploadAPI.Installers;

namespace SongUploadAPI.Extensions
{
    public static class InstallerExtensions    
    {
        public static void InstallServicesInAssembly(this IServiceCollection services, IConfiguration configuration)
        {
            var installers = typeof(Startup).Assembly.GetExportedTypes()
                .Where(type =>
                    typeof(IInstaller).IsAssignableFrom(type)
                    && !type.IsInterface
                    && !type.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<IInstaller>()
                .ToList();

            installers.ForEach(i => i.InstallServices(services, configuration));
        }
    }
}
