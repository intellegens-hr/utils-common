using Bogus;
using Intellegens.Commons.Search.Models;
using Intellegens.Commons.Search.Tests.SearchTests.Setup;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Intellegens.Commons.Search.Tests.SearchTests
{
    public abstract partial class SearchTestsAbstract
    {
        protected readonly SearchDbContext dbContext;

        protected readonly IGenericSearchService<SearchTestEntity> searchService;

        protected readonly IGenericSearchService<SearchTestChildEntity> searchServiceChildren;

        public SearchTestsAbstract(
            SearchDbContext dbContext,
            IGenericSearchService<SearchTestEntity> searchService,
            IGenericSearchService<SearchTestChildEntity> searchServiceChildren)
        {
            this.dbContext = dbContext;
            this.dbContext.Database.Migrate();

            this.searchService = searchService;
            this.searchServiceChildren = searchServiceChildren;
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
            Assert.Equal(5, data.Count());
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

        protected static SearchTestChildEntity GetTestChildEntity(string testingSessionId, int parentId)
        {
            return new Faker<SearchTestChildEntity>()
                .RuleFor(u => u.TestingSessionId, f => testingSessionId)
                .RuleFor(u => u.Text, f => f.Lorem.Paragraph())
                .RuleFor(u => u.ParentId, f => parentId);
        }

        protected static SearchTestEntity GetTestEntity(string testingSessionId)
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

        protected async Task<IQueryable<SearchTestEntity>> GenerateTestDataAndFilterQuery(int count = 20)
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
    }
}