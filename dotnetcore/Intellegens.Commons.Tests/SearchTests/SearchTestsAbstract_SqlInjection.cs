using Intellegens.Commons.Search;
using Intellegens.Commons.Tests.SearchTests.Setup;
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
        public async Task Sql_injection_test_1()
        {
            var query = await GenerateTestDataAndFilterQuery(10);

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Search = new List<SearchFilter>
                {
                    SearchFilter.PartialMatch(nameof(SearchTestEntity.Text), "' SELECT 1 FROM users; "),
                    SearchFilter.PartialMatch(nameof(SearchTestEntity.Text), "\" SELECT 1 FROM users; "),
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count == 0);
        }

        [Fact]
        public async Task Sql_injection_test_2()
        {
            var query = await GenerateTestDataAndFilterQuery(10);

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Search = new List<SearchFilter>
                {
                    SearchFilter.PartialMatch(nameof(SearchTestEntity.Text), "' OR 1 = 1 "),
                    SearchFilter.PartialMatch(nameof(SearchTestEntity.Text), "\" OR 1 = 1 "),
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count == 0);
        }

        [Fact]
        public async Task Unescaped_characters()
        {
            var query = await GenerateTestDataAndFilterQuery(10);

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Search = new List<SearchFilter>
                {
                    SearchFilter.PartialMatch(nameof(SearchTestEntity.Text), "?(/&%$#$%&/()(/&%$#$%&/("),
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count == 0);
        }

    }
}