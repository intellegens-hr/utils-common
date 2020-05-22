namespace Intellegens.Commons.Search
{
    public enum DatabaseProviders { SQLITE, POSTGRES }

    public interface IGenericSearchConfig
    {
        DatabaseProviders DatabaseProvider { get; }
    }

    public class GenericSearchConfig : IGenericSearchConfig
    {
        public DatabaseProviders DatabaseProvider { get; set; }
    }
}