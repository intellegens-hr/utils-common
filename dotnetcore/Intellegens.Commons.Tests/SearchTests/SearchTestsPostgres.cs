using Intellegens.Commons.Search;
using Intellegens.Commons.Tests.SearchTests.Setup;

namespace Intellegens.Commons.Tests.SearchTests
{
    public class SearchTestsPostgres : SearchTestsAbstract
    {
        public SearchTestsPostgres() : base(new SearchDbContextPostgres(), DatabaseProviders.POSTGRES)
        {
        }
    }
}