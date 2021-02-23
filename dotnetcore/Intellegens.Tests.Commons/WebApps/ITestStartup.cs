using Microsoft.AspNetCore.Hosting;
using System.Net.Http;

namespace Intellegens.Tests.Commons.WebApps
{
    public interface ITestStartup
    {
        public HttpMessageHandler BackchannelHttpHandler { get; set; }
        public IWebHostEnvironment WebHostEnvironment { get; set; }
    }
}