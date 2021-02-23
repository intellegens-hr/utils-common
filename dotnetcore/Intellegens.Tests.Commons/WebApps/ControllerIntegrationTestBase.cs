using IdentityModel.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Intellegens.Tests.Commons.WebApps
{
    public abstract class ControllerIntegrationTestBase<TStartup, TStartupBase, TFactory> : IClassFixture<TFactory>
        where TStartup : class, ITestStartup
        where TStartupBase : class
        where TFactory : CustomWebApplicationFactoryBase<TStartup, TStartupBase>
    {
        protected CustomWebApplicationFactoryBase<TStartup, TStartupBase> Factory { get;}
        protected string BaseUrl => Factory.BaseUrl;

        protected ControllerIntegrationTestBase(
            CustomWebApplicationFactoryBase<TStartup, TStartupBase> factory,
            bool useUserSecrets = false,
            string solutionRelativeDirectory = null)
        {
            Factory = factory;
            factory.UseBaseProjectUserSecrets = useUserSecrets;
            factory.SolutionRelativeContentDirectory = solutionRelativeDirectory;

            OnPreInit();

            var webhost = factory.WithWebHostBuilder(
                builder => builder
                .ConfigureServices(ConfigureTestServices)
            );

            webhost.ClientOptions.BaseAddress = new Uri(factory.BaseUrl);
            webhost.Server.BaseAddress = new Uri(factory.BaseUrl);

            factory.Startup.BackchannelHttpHandler = webhost.Server.CreateHandler();
            Server = webhost.Server;
        }

        protected virtual void OnPreInit()
        {

        }

        protected IServiceProvider Services => Server.Services;
        protected string DefaultClientId { get; set; }
        protected string DefaultClientSecret { get; set; }
        protected string DefaultScope { get; set; }
        protected TestServer Server { get; }

        protected virtual void ConfigureTestServices(IServiceCollection services)
        {
        }

        protected async Task<string> GetAccessTokenFromPassword(string username, string password, string clientId = null, string scope = null)
        {
            using var httpClient = Server.CreateClient();
            var disco = await httpClient.GetDiscoveryDocumentAsync();

            var tokenResponse = await httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId ?? DefaultClientId,
                Scope = scope ?? DefaultScope,
                UserName = username,
                Password = password
            });

            return tokenResponse.AccessToken;
        }

        protected async Task<string> GetAccessTokenFromSecret(string clientId = null, string scope = null, string clientSecret = null)
        {
            using var httpClient = Server.CreateClient();
            var disco = await httpClient.GetDiscoveryDocumentAsync();

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId ?? DefaultClientId,
                Scope = scope ?? DefaultScope,
                ClientSecret = clientSecret ?? DefaultClientSecret
            });

            return tokenResponse.AccessToken;
        }

        protected HttpClient GetHttpClient(string bearerToken = null)
        {
            var client = Server.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(1);
            if (!string.IsNullOrEmpty(bearerToken))
            {
                client.SetBearerToken(bearerToken);
            }
            return client;
        }
    }
}