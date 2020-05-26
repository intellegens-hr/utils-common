using Intellegens.Commons.Search;
using Intellegens.Commons.Tests.SearchTests.Setup;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Intellegens.Commons.Tests.SearchTests
{
    public abstract partial class SearchTestsAbstract
    {
        [Fact]
        public async Task Ordering_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(10);
            var entity = await query.FirstAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Filters = new List<SearchFilter>
                {
                    SearchFilter.ExactMatch(nameof(SearchTestEntity.TestingSessionId), entity.TestingSessionId)
                },
                Ordering = new List<SearchOrder>
                {
                    SearchOrder.AsDescending(nameof(SearchTestEntity.Numeric))
                }
            };

            var data = await searchService.Search(query, searchRequest);

            for (var i = 0; i < 3; i++)
                Assert.True(data[i].Numeric > data[i + 1].Numeric);
        }

        [Fact]
        public async Task Ordering_nested_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(20);
            var entity = await query.FirstAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 20,
                Filters = new List<SearchFilter>
                {
                    SearchFilter.PartialMatch(nameof(SearchTestChildEntity.TestingSessionId), entity.TestingSessionId)
                },
                Ordering = new List<SearchOrder>
                {
                    SearchOrder.AsDescending("Parent.Id")
                }
            };

            var data = await searchServiceChildren.Search(dbContext.SearchTestChildEntities, searchRequest);

            for (var i = 0; i < 19; i++)
                Assert.True(data[i].Parent.Id >= data[i + 1].Parent.Id);
        }
    }
}