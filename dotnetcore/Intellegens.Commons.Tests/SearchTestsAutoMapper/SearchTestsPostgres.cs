using Intellegens.Commons.Search;
using Intellegens.Commons.Tests.SearchTests.Setup;

namespace Intellegens.Commons.Tests.SearchTestsAutoMapper
{
    public class SearchTestsAutomapperPostgres : SearchTestsAutomapperAbstract
    {
        public SearchTestsAutomapperPostgres() : base(new SearchDbContextPostgres(), SearchDatabaseProviders.POSTGRES)
        {
        }
    }
}