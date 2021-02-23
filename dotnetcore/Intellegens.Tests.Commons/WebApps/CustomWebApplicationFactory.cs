using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Intellegens.Tests.Commons.WebApps
{
    public abstract class CustomWebApplicationFactoryBase<TStartup, TStartupBase> : WebApplicationFactory<TStartup>
        where TStartup : class
        where TStartupBase : class
    {
        private Assembly baseAssembly;

        public CustomWebApplicationFactoryBase()
        {
            baseAssembly = typeof(TStartupBase).Assembly;
        }

        public string BaseUrl { get; protected set; } = "https://localhost:5001";
        public string EnvironmentToUse { get; set; } = "Development";
        public string SolutionRelativeContentDirectory { get; set; }
        public bool UseBaseProjectUserSecrets { get; set; } = false;
        public TStartup Startup { get; private set; }

        protected abstract TStartup ConstructStartup(IWebHostEnvironment webHostEnvironment);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (!string.IsNullOrEmpty(SolutionRelativeContentDirectory))
                builder.UseSolutionRelativeContentRoot(SolutionRelativeContentDirectory);

            builder
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    if (UseBaseProjectUserSecrets)
                        builder.AddUserSecrets(baseAssembly);
                })
                .UseEnvironment(EnvironmentToUse)
                .ConfigureServices(b =>
                {

                })
                .UseStartup<TStartup>((x) =>
                {
                    Startup = ConstructStartup(x.HostingEnvironment);
                    return Startup;
                })
                .UseUrls(BaseUrl);
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .ConfigureTestServices((services) =>
                    {
                        services
                            .AddControllers()
                            .AddApplicationPart(baseAssembly);
                    });
            });
        }
    }
}