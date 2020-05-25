using Bogus;
using Intellegens.Commons.Search;
using Intellegens.Commons.Tests.SearchTests.Setup;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Intellegens.Commons.Tests.SearchTests
{
    public abstract partial class SearchTestsAbstract
    {
        public SearchTestsAbstract(SearchDbContext dbContext, SearchDatabaseProviders databaseProvider)
        {
            this.dbContext = dbContext;
            this.dbContext.Database.Migrate();

            var config = new GenericSearchConfig { DatabaseProvider = databaseProvider };
            searchService = new GenericSearchService<SearchTestEntity>(config);
            searchServiceChildren = new GenericSearchService<SearchTestChildEntity>(config);
        }

        private readonly SearchDbContext dbContext;
        protected readonly GenericSearchService<SearchTestEntity> searchService;
        protected readonly GenericSearchService<SearchTestChildEntity> searchServiceChildren;

        private static SearchTestChildEntity GetTestChildEntity(string testingSessionId, int parentId)
        {
            return new Faker<SearchTestChildEntity>()
                .RuleFor(u => u.TestingSessionId, f => testingSessionId)
                .RuleFor(u => u.ParentId, f => parentId);
        }

        private static SearchTestEntity GetTestEntity(string testingSessionId)
        {
            SearchTestEntity entity = new Faker<SearchTestEntity>()
                .RuleFor(u => u.TestingSessionId, f => testingSessionId)
                .RuleFor(u => u.Text, f => f.Lorem.Paragraph())
                .RuleFor(u => u.Numeric, f => f.Random.Int())
                .RuleFor(u => u.Guid, f => Guid.NewGuid())
                .RuleFor(u => u.Decimal, f => f.Random.Decimal())
                .RuleFor(u => u.Date, f => f.Date.Between(DateTime.Now.AddMonths(-12), DateTime.Now.AddMonths(12)));

            entity.Children = new List<SearchTestChildEntity>();

            for (int i = 0; i < 5; i++)
                entity.Children.Add(GetTestChildEntity(entity.TestingSessionId, entity.Id));

            return entity;
        }

        private async Task<IQueryable<SearchTestEntity>> GenerateTestDataAndFilterQuery(int count = 20)
        {
            var testingSessionId = Guid.NewGuid().ToString();

            for (int i = 0; i < count; i++)
            {
                var entity = GetTestEntity(testingSessionId);
                dbContext.Add(entity);
            }

            await dbContext.SaveChangesAsync();
            return dbContext.SearchTestEntities.Where(x => x.TestingSessionId == testingSessionId);
        }

        [Fact]
        public async Task Search_limit_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var searchRequest = new SearchRequest
            {
                Limit = 5
            };

            var result = await searchService.Search(query, searchRequest);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task Search_offset_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Offset = 1
            };

            var result = await searchService.Search(query, searchRequest);
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public async Task Search_count_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var searchRequest = new SearchRequest
            {
                Limit = 5
            };

            var (count, data) = await searchService.SearchAndCount(query, searchRequest);
            Assert.Equal(5, count);
            Assert.Equal(5, data.Count);
        }
    }
}