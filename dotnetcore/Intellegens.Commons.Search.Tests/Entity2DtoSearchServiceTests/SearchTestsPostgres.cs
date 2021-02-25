using Intellegens.Commons.Search;
using Intellegens.Commons.Search.Tests.SearchTests.Setup;

namespace Intellegens.Commons.Search.Tests.Entity2DtoSearchServiceTests
{
    public class SearchTestsAutomapperPostgres : Entity2DtoSearchServiceTestAbstract
    {
        public SearchTestsAutomapperPostgres() : base(new SearchDbContextPostgres(), SearchDatabaseProviders.POSTGRES)
        {
        }
    }
}