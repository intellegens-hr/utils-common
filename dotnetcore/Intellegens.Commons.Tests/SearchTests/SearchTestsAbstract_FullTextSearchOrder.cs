using Intellegens.Commons.Search.Models;
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
        [Fact]
        public async Task Fulltext_search_should_order_by_number_of_hits()
        {
            const string textSuffix = "Filodendron";
            string testingSessionId = Guid.NewGuid().ToString();

            // entity 1 - contains two hits and starts with AAA
            var entity1 = GetTestEntity(testingSessionId);
            entity1.Text = $"AAA{entity1.Text}{textSuffix}";
            entity1.Children.ToList()[0].Text += textSuffix;
            dbContext.SearchTestEntities.Add(entity1);

            // entity 2 - contains three hits
            var entity2 = GetTestEntity(testingSessionId);
            entity2.Text = $"{entity2.Text}{textSuffix}";
            entity2.Children.ToList()[0].Text += textSuffix;
            entity2.Children.ToList()[1].Text += textSuffix;
            dbContext.SearchTestEntities.Add(entity2);

            // entity 3 - contains zero hits
            var entity3 = GetTestEntity(testingSessionId);
            dbContext.SearchTestEntities.Add(entity3);

            // entity 4 - contains two hits and starts with ZZZ
            var entity4 = GetTestEntity(testingSessionId);
            entity4.Text = $"ZZZ{entity4.Text}{textSuffix}";
            entity4.Children.ToList()[0].Text += textSuffix;
            dbContext.SearchTestEntities.Add(entity4);

            await dbContext.SaveChangesAsync();

            var query = dbContext.SearchTestEntities.AsQueryable();

            var request = new SearchRequest
            {
                Keys = new List<string> { nameof(SearchTestEntity.TestingSessionId) },
                Values = new List<string> { testingSessionId },
                Operator = Operators.EQUALS,

                Criteria = new List<SearchCriteria>
                {
                    new SearchCriteria
                    {
                        Keys = new List<string>(),
                        Values = new List<string>{textSuffix},
                        Operator = Operators.STRING_CONTAINS
                    }
                },
                CriteriaLogic = LogicOperators.ALL,

                Order = new List<SearchOrder>
                {
                    new SearchOrder{Key = nameof(SearchTestEntity.Text)}
                },
                OrderByMatchCount = true,

                Offset = 0,
                Limit = 10
            };

            var searchResult = await searchService.Search(query, request);

            Assert.Equal(3, searchResult.Count);
            Assert.Equal(entity2.Id, searchResult[0].Id);
            Assert.Equal(entity1.Id, searchResult[1].Id);
            Assert.Equal(entity4.Id, searchResult[2].Id);
        }

        [Fact]
        public async Task Fulltext_search_on_multiple_values_should_order_by_number_of_hits()
        {
            const string textSuffix = "Filodendron";
            string testingSessionId = Guid.NewGuid().ToString();

            // entity 1 - contains two hits and starts with AAA
            var entity1 = GetTestEntity(testingSessionId);
            entity1.Text = $"AAA{entity1.Text}{textSuffix}";
            entity1.Children.ToList()[0].Text += textSuffix;
            dbContext.SearchTestEntities.Add(entity1);

            // entity 2 - contains three hits
            var entity2 = GetTestEntity(testingSessionId);
            entity2.Text = $"{entity2.Text}{textSuffix}";
            entity2.Children.ToList()[0].Text += textSuffix;
            entity2.Children.ToList()[1].Text += textSuffix;
            dbContext.SearchTestEntities.Add(entity2);

            // entity 3 - contains zero hits
            var entity3 = GetTestEntity(testingSessionId);
            dbContext.SearchTestEntities.Add(entity3);

            // entity 4 - contains two hits and starts with ZZZ
            var entity4 = GetTestEntity(testingSessionId);
            entity4.Text = $"ZZZ{entity4.Text}{textSuffix}";
            entity4.Children.ToList()[0].Text += textSuffix;
            dbContext.SearchTestEntities.Add(entity4);

            await dbContext.SaveChangesAsync();

            var query = dbContext.SearchTestEntities.AsQueryable();

            var request = new SearchRequest
            {
                Keys = new List<string> { nameof(SearchTestEntity.TestingSessionId) },
                Values = new List<string> { testingSessionId },
                Operator = Operators.EQUALS,

                Criteria = new List<SearchCriteria>
                {
                    new SearchCriteria
                    {
                        Keys = new List<string>(),
                        Values = new List<string>{textSuffix, "DummyData1", "DummyData2"},
                        Operator = Operators.STRING_CONTAINS
                    }
                },
                CriteriaLogic = LogicOperators.ALL,

                Order = new List<SearchOrder>
                {
                    new SearchOrder{Key = nameof(SearchTestEntity.Text)}
                },
                OrderByMatchCount = true,

                Offset = 0,
                Limit = 10
            };

            var searchResult = await searchService.Search(query, request);

            Assert.Equal(3, searchResult.Count);
            Assert.Equal(entity2.Id, searchResult[0].Id);
            Assert.Equal(entity1.Id, searchResult[1].Id);
            Assert.Equal(entity4.Id, searchResult[2].Id);
        }

        [Fact]
        public async Task Fulltext_search_on_multiple_values_should_order_by_number_of_hits_when_numeric()
        {
            string testingSessionId = Guid.NewGuid().ToString();

            // entity 1 - contains two hits and starts with AAA
            var entity1 = GetTestEntity(testingSessionId);
            dbContext.SearchTestEntities.Add(entity1);

            // entity 2 - contains three hits
            var entity2 = GetTestEntity(testingSessionId);
            dbContext.SearchTestEntities.Add(entity2);

            // entity 3 - contains zero hits
            var entity3 = GetTestEntity(testingSessionId);
            dbContext.SearchTestEntities.Add(entity3);

            // entity 4 - contains two hits and starts with ZZZ
            var entity4 = GetTestEntity(testingSessionId);
            dbContext.SearchTestEntities.Add(entity4);

            await dbContext.SaveChangesAsync();

            var query = dbContext.SearchTestEntities.AsQueryable();

            var request = new SearchRequest
            {
                Keys = new List<string> { nameof(SearchTestEntity.TestingSessionId) },
                Values = new List<string> { testingSessionId },
                Operator = Operators.EQUALS,

                Criteria = new List<SearchCriteria>
                {
                    new SearchCriteria
                    {
                        Keys = new List<string>(){ "Id", "SiblingId", "Children.ParentId" },
                        Values = new List<string>{ entity1.Id.ToString(), "1", "2", "3"},
                        ValuesLogic = LogicOperators.ANY,
                        Operator = Operators.EQUALS
                    }
                },
                CriteriaLogic = LogicOperators.ALL,

                Order = new List<SearchOrder>
                {
                    new SearchOrder{Key = nameof(SearchTestEntity.Text)}
                },
                OrderByMatchCount = true,

                Offset = 0,
                Limit = 10
            };

            var searchResult = await searchService.Search(query, request);

            Assert.Equal(1, searchResult.Count);
            Assert.Equal(entity1.Id, searchResult[0].Id);
        }
    }
}