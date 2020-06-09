using Intellegens.Commons.Search;
using Intellegens.Commons.Tests.SearchTests.Setup;

namespace Intellegens.Commons.Tests.Entity2DtoSearchServiceTests
{
    public class SearchTestsAutomapperSqlite : Entity2DtoSearchServiceTestAbstract
    {
        public SearchTestsAutomapperSqlite() : base(new SearchDbContextSqlite(), SearchDatabaseProviders.SQLITE)
        {
        }
    }
}