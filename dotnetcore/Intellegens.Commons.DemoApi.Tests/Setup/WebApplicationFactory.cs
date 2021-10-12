using Intellegens.Tests.Commons.WebApps;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intellegens.Commons.DemoApi.Tests.Setup
{
    public class WebApplicationFactory : CustomWebApplicationFactoryBase<StartupTest, Startup>
    {
        protected override StartupTest ConstructStartup(IWebHostEnvironment webHostEnvironment)
        {
            return new StartupTest(null);
        }
    }
}
