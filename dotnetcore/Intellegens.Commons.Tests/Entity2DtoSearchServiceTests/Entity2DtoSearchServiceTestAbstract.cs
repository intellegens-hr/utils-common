using Bogus;
using Intellegens.Commons.Search;
using Intellegens.Commons.Search.Models;
using Intellegens.Commons.Tests.Entity2DtoSearchServiceTests.Setup;
using Intellegens.Commons.Tests.SearchTests.Setup;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Intellegens.Commons.Tests.Entity2DtoSearchServiceTests
{
    public abstract class Entity2DtoSearchServiceTestAbstract
    {
        public Entity2DtoSearchServiceTestAbstract(SearchDbContext dbContext, SearchDatabaseProviders databaseProvider)
        {
            this.dbContext = dbContext;
            this.dbContext.Database.Migrate();

            var config = new GenericSearchConfig { DatabaseProvider = databaseProvider };
            searchService = new Entity2DtoSearchService<SearchTestEntity, SearchTestEntityDto>(config, AutomapperConfig.Mapper);
            searchServiceChildren = new Entity2DtoSearchService<SearchTestChildEntity, SearchTestChildEntityDto>(config, AutomapperConfig.Mapper);
        }

        protected readonly SearchDbContext dbContext;
        protected readonly Entity2DtoSearchService<SearchTestEntity, SearchTestEntityDto> searchService;
        protected readonly Entity2DtoSearchService<SearchTestChildEntity, SearchTestChildEntityDto> searchServiceChildren;

        protected static SearchTestChildEntityDto GetTestChildEntity(string testingSessionId, int parentId)
        {
            SearchTestChildEntity child = new Faker<SearchTestChildEntity>()
                .RuleFor(u => u.TestingSessionId, f => testingSessionId)
                .RuleFor(u => u.ParentId, f => parentId);

            return AutomapperConfig.Mapper.Map<SearchTestChildEntityDto>(child);
        }

        protected static SearchTestEntityDto GetTestEntity(string testingSessionId)
        {
            SearchTestEntity entity = new Faker<SearchTestEntity>()
                .RuleFor(u => u.TestingSessionId, f => testingSessionId)
                .RuleFor(u => u.Text, f => f.Lorem.Paragraph())
                .RuleFor(u => u.Numeric, f => f.Random.Int())
                .RuleFor(u => u.Guid, f => Guid.NewGuid())
                .RuleFor(u => u.Decimal, f => f.Random.Decimal())
                .RuleFor(u => u.Date, f => f.Date.Between(DateTime.Now.AddMonths(-12), DateTime.Now.AddMonths(12)));

            var dto = AutomapperConfig.Mapper.Map<SearchTestEntityDto>(entity);

            dto.ChildrenDtos = new List<SearchTestChildEntityDto>();

            for (int i = 0; i < 5; i++)
                dto.ChildrenDtos.Add(GetTestChildEntity(entity.TestingSessionId, entity.Id));

            return dto;
        }

        protected async Task<IQueryable<SearchTestEntity>> GenerateTestDataAndFilterQuery(int count = 20)
        {
            var testingSessionId = Guid.NewGuid().ToString();

            for (int i = 0; i < count; i++)
            {
                var entity = GetTestEntity(testingSessionId);
                dbContext.Add(AutomapperConfig.Mapper.Map<SearchTestEntity>(entity));
            }

            await dbContext.SaveChangesAsync();
            return dbContext.SearchTestEntities
                .Where(x => x.TestingSessionId == testingSessionId);
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

        [Fact]
        public async Task Partial_text_filtering_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var dto = await query.FirstAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.PartialMatch(nameof(SearchTestEntityDto.Text), dto.Text.Substring(0, 2))
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count >= 1);
        }

        [Fact]
        public async Task Exact_text_filtering_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var dto = await query.FirstAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntityDto.Text), dto.Text)
                }
            };

            var data = await searchService.Search(query, searchRequest);
            Assert.True(data.Count == 1);
        }

        [Fact]
        public async Task Exact_text_filtering_by_nested_property_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var dto = await query.FirstAsync();
            var child = dto.Children.First();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal("ParentDto.TestingSessionId", dto.TestingSessionId)
                }
            };

            var childQueries = dbContext.SearchTestChildEntities
                .Include(x => x.Parent)
                .Where(x => x.TestingSessionId == dto.TestingSessionId);

            var data = await searchServiceChildren.Search(childQueries, searchRequest);
            Assert.True(data.Count == 5);
        }

        [Fact]
        public async Task Exact_text_filtering_by_nested_property_should_work_with_camel_case()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var dto = await query.FirstAsync();
            var child = dto.Children.First();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal("parentDto.testingSessionId", dto.TestingSessionId)
                }
            };

            var childQueries = dbContext.SearchTestChildEntities
                .Include(x => x.Parent)
                .Where(x => x.TestingSessionId == dto.TestingSessionId);

            var data = await searchServiceChildren.Search(childQueries, searchRequest);
            Assert.True(data.Count == 5);
        }

        [Fact]
        public async Task Exact_text_filtering_by_nested_collection_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var dto = await query.FirstAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal("ChildrenDtos.TestingSessionId", dto.TestingSessionId),
                    SearchCriteria.Equal("Numeric", dto.Numeric.ToString())
                }
            };

            var querySearch = dbContext.SearchTestEntities
                .Include(x => x.Children)
                .Where(x => x.TestingSessionId == dto.TestingSessionId);

            var data = await searchService.Search(querySearch, searchRequest);
            Assert.True(data.Count >= 1);
        }

        [Fact]
        public async Task Partial_match_text_filtering_by_nested_collection_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var dto = await query.FirstAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.PartialMatch("ChildrenDtos.TestingSessionId", dto.TestingSessionId),
                    SearchCriteria.PartialMatch("Numeric", dto.Numeric.ToString())
                }
            };

            var querySearch = dbContext.SearchTestEntities
                .Include(x => x.Children)
                .Where(x => x.TestingSessionId == dto.TestingSessionId);

            var data = await searchService.Search(querySearch, searchRequest);
            Assert.True(data.Count >= 1);
        }

        [Fact]
        public async Task Null_params_should_be_handled_work()
        {
            var query = await GenerateTestDataAndFilterQuery(5);
            var dto = await query.FirstAsync();
            var child = dto.Children.First();

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    new SearchCriteria
                    {
                        Keys = new List<string> { "ParentDto.TestingSessionId" },
                        Values = null,
                        Criteria = null
                    }
                }
            };

            var childQueries = dbContext.SearchTestChildEntities
                .Include(x => x.Parent)
                .Where(x => x.TestingSessionId == dto.TestingSessionId);

            var data = await searchServiceChildren.Search(childQueries, searchRequest);
            Assert.True(data.Count == 5);
        }

        [Fact]
        public virtual async Task Ordering_nested_on_collection_should_work()
        {
            var query = await GenerateTestDataAndFilterQuery(10);

            var entity = await query.FirstAsync();
            await dbContext.SaveChangesAsync();

            var searchRequest = new SearchRequest
            {
                Limit = 100,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.PartialMatch($"{nameof(SearchTestEntityDto.ChildrenDtos)}.TestingSessionId", entity.TestingSessionId)
                },
                Order = new List<SearchOrder>
                {
                    SearchOrder.AsAscending($"{nameof(SearchTestEntityDto.ChildrenDtos)}.ParentDto.Date")
                }
            };

            var data = await searchService.Search(dbContext.SearchTestEntities, searchRequest);

            for (int i = 1; i < data.Count; i++)
                Assert.True(data[i - 1].Date <= data[i].Date);
        }

        [Fact]
        public async Task Fulltext_search_should_work_with_any_class()
        {
            var query = await GenerateTestDataAndFilterQuery(20);
            var entity = await query.FirstAsync();

            var textToSearch = entity.Text.Substring(0, 4);

            var searchRequest = new SearchRequest
            {
                Limit = 20,
                Criteria = new List<SearchCriteria>
                {
                    new SearchCriteria
                    {
                        Operator = Operators.FULL_TEXT_SEARCH_CONTAINS,
                        Values = new List<string>{ textToSearch }
                    }
                }
            };

            var expectedCount = query.Where(x => x.Text.ToUpper().Contains(textToSearch.ToUpper()) || x.TestingSessionId.ToUpper().Contains(textToSearch.ToUpper())).Count();
            var data = await searchService.Search(query, searchRequest);

            Assert.True(data.Count == expectedCount);
        }
    }
}