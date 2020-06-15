using Intellegens.Commons.Search;
using Intellegens.Commons.Tests.SearchTests.Setup;

namespace Intellegens.Commons.Tests.SearchTestsAutoMapper
{
    public class SearchTestsAutomapperSqlite : SearchTestsAutomapperAbstract
    {
        public SearchTestsAutomapperSqlite() : base(new SearchDbContextSqlite(), SearchDatabaseProviders.SQLITE)
        {
        }
    }
}