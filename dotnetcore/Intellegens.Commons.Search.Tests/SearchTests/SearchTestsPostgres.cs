using Intellegens.Commons.Search.Tests.SearchTests.Setup;

namespace Intellegens.Commons.Search.Tests.SearchTests
{
    public class SearchTestsPostgres : SearchTestsAbstract
    {
        public SearchTestsPostgres() : base(
            new SearchDbContextPostgres(),
            new PostgresSearchService<SearchTestEntity>(),
            new PostgresSearchService<SearchTestChildEntity>())
        {
        }
    }
}