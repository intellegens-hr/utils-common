using Intellegens.Commons.Search;
using Intellegens.Commons.Search.Models;
using Intellegens.Commons.Tests.SearchTests.Setup;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Intellegens.Commons.Tests.SearchTests
{
    public class SearchTestsSqlite : SearchTestsAbstract
    {
        public SearchTestsSqlite() : base(new SearchDbContextSqlite(), SearchDatabaseProviders.SQLITE)
        {
        }

        [Fact]
        public async override Task Ordering_nested_on_nullable_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(10);

            var entity = await query.FirstAsync();
            entity.Sibling = GetTestEntity(entity.TestingSessionId);
            await dbContext.SaveChangesAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 100,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.PartialMatch(nameof(SearchTestEntity.TestingSessionId), entity.TestingSessionId)
                },
                Order = new List<SearchOrder>
                {
                    SearchOrder.AsAscending("Sibling.Id")
                }
            };

            var data = await searchService.Search(dbContext.SearchTestEntities, searchRequest);

            Assert.NotNull(data[10].SiblingId);
            Assert.Null(data[9].SiblingId);
        }

        public override async Task Comparison_operator_should_work_with_numbers()
        {
        }
    }
}