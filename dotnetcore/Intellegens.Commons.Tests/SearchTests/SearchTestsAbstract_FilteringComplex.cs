using Intellegens.Commons.Search.Models;
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
        public async Task Partial_text_filtering_by_multiple_columns_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entity = await query.FirstAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.PartialMatch(nameof(SearchTestEntity.Text), entity.Text.Substring(0, 2)),
                    SearchCriteria.PartialMatch(nameof(SearchTestEntity.TestingSessionId), entity.TestingSessionId.Substring(0, 2))
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count >= 1);
        }

        [Fact]
        public async Task Exact_text_filtering_by_multiple_columns_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entity = await query.FirstAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.Text), entity.Text),
                    SearchCriteria.PartialMatch(nameof(SearchTestEntity.TestingSessionId), entity.TestingSessionId.Substring(0, 2))
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count == 1);
        }

        [Fact]
        public async Task Or_operator_filtering_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entities = await query.Take(2).ToListAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal("Id", entities[0].Id.ToString()),
                    SearchCriteria.Equal("Id", entities[1].Id.ToString())
                },
                CriteriaLogic = LogicOperators.ANY
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count == 2);
        }

        [Fact]
        public async Task Partial_text_filtering_should_be_case_insensitive_if_configured()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entity = await query.FirstAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.PartialMatch(nameof(SearchTestEntity.Text), entity.Text.Substring(0, 16).ToUpper()),
                    SearchCriteria.PartialMatch(nameof(SearchTestEntity.Text), entity.Text.Substring(0, 16).ToLower())
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.Contains(entity.Id, data.Select(x => x.Id));
        }
    }
}