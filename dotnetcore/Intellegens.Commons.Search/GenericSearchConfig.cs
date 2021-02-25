namespace Intellegens.Commons.Search
{
    /// <summary>
    /// Supported database providers
    /// </summary>
    public enum SearchDatabaseProviders { SQLITE, POSTGRES }

    /// <summary>
    /// Search configuration interface
    /// </summary>
    public interface IGenericSearchConfig
    {
        SearchDatabaseProviders DatabaseProvider { get; }
    }

    /// <summary>
    /// Search configuration default implementation
    /// </summary>
    public class GenericSearchConfig : IGenericSearchConfig
    {
        public SearchDatabaseProviders DatabaseProvider { get; set; }
    }
}