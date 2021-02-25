using Intellegens.Commons.Search;
using Intellegens.Commons.Search.Tests.SearchTests.Setup;

namespace Intellegens.Commons.Search.Tests.SearchTests
{
    public class SearchTestsPostgres : SearchTestsAbstract
    {
        public SearchTestsPostgres() : base(new SearchDbContextPostgres(), SearchDatabaseProviders.POSTGRES)
        {
        }
    }
}