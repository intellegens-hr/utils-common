namespace Intellegens.Commons.Search
{
    public enum SearchDatabaseProviders { SQLITE, POSTGRES }

    public interface IGenericSearchConfig
    {
        SearchDatabaseProviders DatabaseProvider { get; }
    }

    public class GenericSearchConfig : IGenericSearchConfig
    {
        public SearchDatabaseProviders DatabaseProvider { get; set; }
    }
}