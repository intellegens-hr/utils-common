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
        public async Task Filter_in_not_in_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entities = await query.ToListAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Filters = new List<SearchFilter>
                {
                    SearchFilter.ExactMatch(nameof(SearchTestEntity.TestingSessionId), entities[0].TestingSessionId),

                    // NOT IN
                    new SearchFilter
                    {
                        Key = nameof(SearchTestEntity.Id),
                        Values = new List<string>{ entities[2].Id.ToString() },
                        Type = FilterMatchTypes.EXACT_MATCH,
                        ComparisonType = ComparisonTypes.NOT_EQUAL
                    },
                    new SearchFilter
                    {
                        Key = nameof(SearchTestEntity.Id),
                        Values = new List<string>{ entities[3].Id.ToString() },
                        Type = FilterMatchTypes.EXACT_MATCH,
                        ComparisonType = ComparisonTypes.NOT_EQUAL
                    }
                },
                Search = new List<SearchFilter>
                {
                    // IN values
                    new SearchFilter
                    {
                        Key = nameof(SearchTestEntity.Id),
                        Values = new List<string>{ entities[0].Id.ToString()},
                        Type = FilterMatchTypes.EXACT_MATCH
                    },
                    new SearchFilter
                    {
                        Key = nameof(SearchTestEntity.Id),
                        Values = new List<string>{ entities[1].Id.ToString() },
                        Type = FilterMatchTypes.EXACT_MATCH
                    }
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count == 2);
        }

        [Fact]
        public async Task Filter_in_not_in_should_work_with_or()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entities = await query.ToListAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Search = new List<SearchFilter>
                {
                    new SearchFilter
                    {
                        Key = nameof(SearchTestEntity.Id),
                        Type = FilterMatchTypes.EXACT_MATCH,
                        Values = new List<string>{ entities[0].Id.ToString(), entities[1].Id.ToString() },
                    },
                    new SearchFilter
                    {
                        Key = nameof(SearchTestEntity.Id),
                        Values = new List<string>{ entities[2].Id.ToString(), entities[3].Id.ToString() },
                        Type = FilterMatchTypes.EXACT_MATCH,
                        ComparisonType = ComparisonTypes.NOT_EQUAL
                    }
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count == 3);
        }

        [Fact]
        public async Task Filter_not_in_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entities = await query.ToListAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Search = new List<SearchFilter>
                {
                    new SearchFilter
                    {
                        Key = nameof(SearchTestEntity.Id),
                        Type = FilterMatchTypes.EXACT_MATCH,
                        Values = new List<string>{ entities[2].Id.ToString(), entities[3].Id.ToString() },
                        ComparisonType = ComparisonTypes.NOT_EQUAL
                    }
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count == 3);
        }

        [Fact]
        public async Task Filter_in_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var entities = await query.ToListAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Search = new List<SearchFilter>
                {
                    new SearchFilter
                    {
                        Key = nameof(SearchTestEntity.Id),
                        Type = FilterMatchTypes.EXACT_MATCH,
                        Values = new List<string>{ entities[2].Id.ToString(), entities[3].Id.ToString() }
                    }
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count == 2);
        }
    }
}