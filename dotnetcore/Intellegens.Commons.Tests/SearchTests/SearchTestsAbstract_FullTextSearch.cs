using Intellegens.Commons.Search.Models;
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
            var query = await GenerateTestDataAndFilterQuery(100);
            var entity = await query.FirstAsync();

            var textToSearch = entity.Text.Substring(0, 4);

            var searchRequest = new SearchRequest
            {
                Limit = 100,
                Criteria = new List<SearchCriteria>
                {
                    new SearchCriteria
                    {
                        Operator = Operators.STRING_CONTAINS,
                        Values = new List<string>{ textToSearch }
                    }
                }
            };

            var expectedCount =
                query.ToList()
                .Where(x =>
                    x.Text.Contains(textToSearch, System.StringComparison.InvariantCultureIgnoreCase)
                    || x.TestingSessionId.Contains(textToSearch, System.StringComparison.InvariantCultureIgnoreCase)
                    || (x.Sibling?.Text.Contains(textToSearch, System.StringComparison.InvariantCultureIgnoreCase) ?? false)
                    || x.Children.Any(c => c.Text != null && c.Text.Contains(textToSearch, System.StringComparison.InvariantCultureIgnoreCase))
                )
                .Count();
            var data = await searchService.Search(query, searchRequest);

            Assert.Equal(expectedCount, data.Count);
        }
    }
}