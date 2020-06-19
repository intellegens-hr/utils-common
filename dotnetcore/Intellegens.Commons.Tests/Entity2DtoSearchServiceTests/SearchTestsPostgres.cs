using Intellegens.Commons.Search;
using Intellegens.Commons.Tests.SearchTests.Setup;

namespace Intellegens.Commons.Tests.Entity2DtoSearchServiceTests
{
    public class SearchTestsAutomapperPostgres : Entity2DtoSearchServiceTestAbstract
    {
        public SearchTestsAutomapperPostgres() : base(new SearchDbContextPostgres(), SearchDatabaseProviders.POSTGRES)
        {
        }
    }
}