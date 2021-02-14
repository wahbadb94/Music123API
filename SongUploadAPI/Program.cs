using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


namespace SongUploadAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        // Increase request size to 635040000 bytes or 605.62MB
                        // this equivalent to 2 hours of music sampled at 44.1kHz/16bit
                        options.Limits.MaxRequestBodySize = 635040000;
                    })
                    .UseStartup<Startup>();
                });
    }
}
