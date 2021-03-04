using Intellegens.Commons.Search;
using Intellegens.Commons.Search.Models;
using Intellegens.Commons.Search.Tests.SearchTests.Setup;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Intellegens.Commons.Search.Tests.SearchTests
{
    public abstract partial class SearchTestsAbstract
    {
        [Fact]
        public async Task Comparison_operator_should_work_with_dates()
        {
            var countToGenerate = 5;
            var query = await GenerateTestDataAndFilterQuery(countToGenerate);
            var entities = await query.ToListAsync();

            var minDate = entities.Min(x => x.Date);
            var dateMask = "yyyy-MM-dd";

            var searchRequest1 = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Date ) },
                        Operator = Operators.GREATER_THAN,
                        Values = new List<string>{ minDate.AddDays(1).ToString(dateMask) }
                    }
                },
                CriteriaLogic = LogicOperators.ALL
            };

            // Empty.
            var searchRequest2 = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Date ) },
                        Operator = Operators.LESS_THAN,
                        Values = new List<string>{ minDate.ToString(dateMask) }
                    }
                },
                CriteriaLogic = LogicOperators.ALL
            };

            // Not empty.
            var searchRequest3 = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Date ) },
                        Operator = Operators.LESS_THAN_OR_EQUAL_TO,
                        Values = new List<string>{ minDate.AddDays(1).ToString(dateMask) }
                    }
                },
                CriteriaLogic = LogicOperators.ALL
            };

            var searchRequest4 = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Date ) },
                        Operator = Operators.LESS_THAN,
                        Negate = true,
                        Values = new List<string>{ minDate.ToString(dateMask) }
                    }
                },
                CriteriaLogic = LogicOperators.ALL
            };

            var data1 = await searchService.Search(query, searchRequest1);
            var data2 = await searchService.Search(query, searchRequest2);
            var data3 = await searchService.Search(query, searchRequest3);
            var data4 = await searchService.Search(query, searchRequest4);

            Assert.True(data1.Count < countToGenerate);
            Assert.Empty(data2);
            Assert.True(data3.Any());
            Assert.True(data4.Count == countToGenerate);
        }

        [Fact]
        public virtual async Task Comparison_operator_should_work_with_numbers()
        {
            var countToGenerate = 5;
            var query = await GenerateTestDataAndFilterQuery(countToGenerate);
            var entities = await query.ToListAsync();

            var minDecimal = entities.Min(x => x.Decimal).ToString(CultureInfo.InvariantCulture);

            var searchRequest1 = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Decimal ) },
                        Operator = Operators.GREATER_THAN,
                        Values = new List<string>{ minDecimal }
                    }
                }
            };

            var searchRequest2 = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Decimal) },
                        Operator = Operators.LESS_THAN,
                        Values = new List<string>{ minDecimal }
                    }
                }
            };

            var searchRequest3 = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Decimal) },
                        Operator = Operators.LESS_THAN_OR_EQUAL_TO,
                        Values = new List<string>{ minDecimal }
                    }
                }
            };

            var searchRequest4 = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Decimal) },
                        Operator = Operators.LESS_THAN,
                        Negate = true,
                        Values = new List<string>{ minDecimal }
                    }
                }
            };

            var data1 = await searchService.Search(query, searchRequest1);
            var data2 = await searchService.Search(query, searchRequest2);
            var data3 = await searchService.Search(query, searchRequest3);
            var data4 = await searchService.Search(query, searchRequest4);

            Assert.True(data1.Count < countToGenerate);
            Assert.True(data2.Count == 0);
            Assert.True(data3.Count >= 1);
            Assert.True(data4.Count == countToGenerate);
        }

        [Fact]
        public async Task Comparison_operator_should_work_with_text()
        {
            var countToGenerate = 5;
            var query = await GenerateTestDataAndFilterQuery(countToGenerate);
            var entities = await query.ToListAsync();

            var minText = entities.Min(x => x.Text);

            var searchRequest1 = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Text ) },
                        Operator = Operators.GREATER_THAN,
                        Values = new List<string>{ minText }
                    }
                }
            };

            var searchRequest2 = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Text) },
                        Operator = Operators.LESS_THAN,
                        Values = new List<string>{ minText }
                    }
                }
            };

            var searchRequest3 = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Text) },
                        Operator = Operators.LESS_THAN_OR_EQUAL_TO,
                        Values = new List<string>{ minText }
                    }
                }
            };

            var searchRequest4 = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Text) },
                        Operator = Operators.LESS_THAN,
                        Negate = true,
                        Values = new List<string>{ minText }
                    }
                }
            };

            var data1 = await searchService.Search(query, searchRequest1);
            var data2 = await searchService.Search(query, searchRequest2);
            var data3 = await searchService.Search(query, searchRequest3);
            var data4 = await searchService.Search(query, searchRequest4);

            Assert.True(data1.Count < countToGenerate);
            Assert.True(data2.Count == 0);
            Assert.True(data3.Count >= 1);
            Assert.True(data4.Count == countToGenerate);
        }

        [Fact]
        public async Task Wildcard_matching_should_work_with_text()
        {
            var countToGenerate = 10;
            var query = await GenerateTestDataAndFilterQuery(countToGenerate);
            var entities = await query.ToListAsync();

            var startsWithText = entities.First().Text.Substring(0, 3);
            var wildcardExpression = startsWithText + "*";

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Text ) },
                        Operator = Operators.STRING_WILDCARD,
                        Values = new List<string>{ wildcardExpression }
                    }
                }
            };

            var data = await searchService.Search(query, searchRequest);

            var elementsMatchingWildcard = entities.Where(x => x.Text.StartsWith(startsWithText)).Count();

            Assert.Equal(elementsMatchingWildcard, data.Count);
        }

        [Fact]
        public async Task Wildcard_matching_should_work_with_text_2()
        {
            var countToGenerate = 10;
            var query = await GenerateTestDataAndFilterQuery(countToGenerate);
            var entities = await query.ToListAsync();

            var containsText = entities.First().Text.Substring(2, 6);
            var wildcardExpression = $"*{containsText}*";

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Text ) },
                        Operator = Operators.STRING_WILDCARD,
                        Values = new List<string>{ wildcardExpression }
                    }
                }
            };

            var data = await searchService.Search(query, searchRequest);

            var elementsMatchingWildcard = entities.Where(x => x.Text.Contains(containsText)).Count();

            Assert.Equal(elementsMatchingWildcard, data.Count);
        }

        [Fact]
        public async Task Wildcard_matching_should_work_with_text_3()
        {
            var countToGenerate = 10;
            var query = await GenerateTestDataAndFilterQuery(countToGenerate);
            var entities = await query.ToListAsync();

            var containsText = entities.First().Text;
            var wildcardExpression = $"{containsText.Substring(0, 5)}?{containsText.Substring(6)}";

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Text ) },
                        Operator = Operators.STRING_WILDCARD,
                        Values = new List<string>{ wildcardExpression }
                    }
                }
            };

            var data = await searchService.Search(query, searchRequest);

            Assert.Single(data);
        }

        [Fact]
        public async Task Wildcard_matching_should_escape_percentage_character()
        {
            var countToGenerate = 10;
            var query = await GenerateTestDataAndFilterQuery(countToGenerate);
            var entities = await query.ToListAsync();

            var text = entities.First().Text;
            var wildcardExpression = $"%{text}%";

            var searchRequest = new SearchRequest
            {
                Limit = 5,
                Criteria = new List<SearchCriteria>
                {
                    SearchCriteria.Equal(nameof(SearchTestEntity.TestingSessionId), entities.First().TestingSessionId),
                    new SearchCriteria
                    {
                        Keys = new List<string>{nameof( SearchTestEntity.Text ) },
                        Operator = Operators.STRING_WILDCARD,
                        Values = new List<string>{ wildcardExpression }
                    }
                }
            };

            var data = await searchService.Search(query, searchRequest);

            Assert.Empty(data);
        }
    }
}