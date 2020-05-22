using Intellegens.Commons.Tests.SearchTests.Setup;

namespace Intellegens.Commons.Tests.SearchTests
{
    public class SearchTestsSqlite : SearchTestsAbstract
    {
        public SearchTestsSqlite() : base(new SearchDbContextSqlite())
        {
        }
    }
}