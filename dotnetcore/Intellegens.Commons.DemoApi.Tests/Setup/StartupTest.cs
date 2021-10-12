using Intellegens.Tests.Commons.WebApps;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace Intellegens.Commons.DemoApi.Tests.Setup
{
    public class StartupTest : Startup, ITestStartup
    {
        public StartupTest(IConfiguration configuration) : base(configuration)
        {
        }

        public HttpMessageHandler BackchannelHttpHandler { get; set; }
        public IWebHostEnvironment WebHostEnvironment { get; set; }
    }
}