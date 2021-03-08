using Intellegens.Commons.Search;
using Intellegens.Commons.Search.Tests.SearchTests.Setup;

namespace Intellegens.Commons.Search.Tests.Entity2DtoSearchServiceTests
{
    public class SearchTestsAutomapperSqlite : Entity2DtoSearchServiceTestAbstract
    {
        public SearchTestsAutomapperSqlite() : base(new SearchDbContextSqlite(), SearchDatabaseProviders.SQLITE)
        {
        }
    }
}