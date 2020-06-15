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
        public async Task Partial_text_filtering_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entity = await query.FirstAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Filters = new List<SearchFilter>
                {
                    SearchFilter.PartialMatch(nameof(SearchTestEntity.Text), entity.Text.Substring(0, 2))
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count >= 1);
        }

        [Fact]
        public async Task Exact_text_filtering_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entity = await query.FirstAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Filters = new List<SearchFilter>
                {
                    SearchFilter.ExactMatch(nameof(SearchTestEntity.Text), entity.Text)
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count == 1);
        }

        [Fact]
        public async Task Exact_text_filtering_by_nested_property_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entity = await query.FirstAsync();
            var child = entity.Children.First();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Filters = new List<SearchFilter>
                {
                    SearchFilter.ExactMatch("Parent.TestingSessionId", entity.TestingSessionId)
                }
            };

            var childQueries = dbContext.SearchTestChildEntities
                .Include(x => x.Parent)
                .Where(x => x.TestingSessionId == entity.TestingSessionId);

            var data = await searchServiceChildren.Search(childQueries, searchRequest);
            Assert.True(data.Count == 5);
        }

        [Fact]
        public async Task Exact_text_filtering_by_nested_property_should_work_with_camel_case()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entity = await query.FirstAsync();
            var child = entity.Children.First();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Filters = new List<SearchFilter>
                {
                    SearchFilter.ExactMatch("parent.testingSessionId", entity.TestingSessionId)
                }
            };

            var childQueries = dbContext.SearchTestChildEntities
                .Include(x => x.Parent)
                .Where(x => x.TestingSessionId == entity.TestingSessionId);

            var data = await searchServiceChildren.Search(childQueries, searchRequest);
            Assert.True(data.Count == 5);
        }

        [Fact]
        public async Task Exact_text_filtering_by_nested_collection_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entity = await query.FirstAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Filters = new List<SearchFilter>
                {
                    SearchFilter.ExactMatch("Children.TestingSessionId", entity.TestingSessionId),
                    SearchFilter.ExactMatch("Numeric", entity.Numeric.ToString())
                }
            };

            var querySearch = dbContext.SearchTestEntities
                .Include(x => x.Children)
                .Where(x => x.TestingSessionId == entity.TestingSessionId);

            var data = await searchService.Search(querySearch, searchRequest);
            Assert.True(data.Count >= 1);
        }

        [Fact]
        public async Task Partial_match_text_filtering_by_nested_collection_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entity = await query.FirstAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Filters = new List<SearchFilter>
                {
                    SearchFilter.PartialMatch("Children.TestingSessionId", entity.TestingSessionId),
                    SearchFilter.PartialMatch("Numeric", entity.Numeric.ToString())
                }
            };

            var querySearch = dbContext.SearchTestEntities
                .Include(x => x.Children)
                .Where(x => x.TestingSessionId == entity.TestingSessionId);

            var data = await searchService.Search(querySearch, searchRequest);
            Assert.True(data.Count >= 1);
        }

        [Fact]
        public async Task Null_params_should_be_handled_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entity = await query.FirstAsync();
            var child = entity.Children.First();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Filters = new List<SearchFilter>
                {
                    new SearchFilter
                    {
                        Key = "Parent.TestingSessionId",
                        Values = null
                    }
                }
            };

            var childQueries = dbContext.SearchTestChildEntities
                .Include(x => x.Parent)
                .Where(x => x.TestingSessionId == entity.TestingSessionId);

            var data = await searchServiceChildren.Search(childQueries, searchRequest);
            Assert.True(data.Count == 5);
        }
    }
}