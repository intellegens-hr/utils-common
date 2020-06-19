using Intellegens.Commons.Search;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Intellegens.Commons.Tests.SearchTests
{
    public abstract partial class SearchTestsAbstract
    {
        [Fact]
        public async Task Fulltext_search_should_work_with_any_class()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entity = await query.FirstAsync();

            var textToSearch = entity.Text.Substring(0, 4);

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Filters = new List<SearchFilter>
                {
                    new SearchFilter
                    {
                        Operator = FilterMatchOperators.FULL_TEXT_SEARCH,
                        Values = new List<string>{ textToSearch }
                    }
                }
            };

            var expectedCount = query.Where(x => x.Text.Contains(textToSearch) || x.TestingSessionId.Contains(textToSearch)).Count();
            var data = await searchService.Search(query, searchRequest);

            Assert.True(data.Count == expectedCount);
        }
    }
}