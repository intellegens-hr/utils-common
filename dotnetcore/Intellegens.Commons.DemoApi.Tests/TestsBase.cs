using Intellegens.Commons.DemoApi.Tests.Setup;
using Intellegens.Commons.Mvc.Models;
using Intellegens.Commons.Rest;
using Intellegens.Commons.Search.Models;
using Intellegens.Tests.Commons.WebApps;
using System;
using System.Threading.Tasks;

namespace Intellegens.Commons.DemoApi.Tests
{
    public class TestsBase<TDto> : ControllerIntegrationTestBase<StartupTest, Startup, WebApplicationFactory>
        where TDto : class
    {
        protected readonly string baseUrl;
        protected readonly Random random = new();
        protected readonly RestClient restClient;

        public TestsBase(string baseUrl) : base(new WebApplicationFactory())
        {
            this.baseUrl = baseUrl;
            var httpClient = GetHttpClient();
            this.restClient = new RestClient(httpClient);
        }

        public async Task<RestResult<ApiResult<TDto>>> Create(TDto data)
            => await restClient.Post<ApiResult<TDto>>($"{baseUrl}", data);

        public async Task<RestResult<ApiResult<TDto>>> Get(int id)
            => await restClient.Get<ApiResult<TDto>>($"{baseUrl}/{id}");

        public async Task<RestResult<ApiResult<TDto>>> IndexOf(int id, SearchRequest searchRequest)
            => await restClient.Post<ApiResult<TDto>>($"{baseUrl}/indexof/{id}", searchRequest);

        public async Task<RestResult<ApiResult<TDto>>> Search(SearchRequest searchRequest)
            => await restClient.Post<ApiResult<TDto>>($"{baseUrl}/search", searchRequest);

        public async Task<RestResult<ApiResult<TDto>>> Update(int id, TDto data)
            => await restClient.Put<ApiResult<TDto>>($"{baseUrl}/{id}", data);
    }
}