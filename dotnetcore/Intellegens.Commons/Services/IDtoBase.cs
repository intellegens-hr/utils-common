namespace Intellegens.Commons.Services
{
    /// <summary>
    /// Basic DTO (domain transfer object) interface, used by IRepositoryBase
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IDtoBase<TKey>
    {
        /// <summary>
        /// Fetch Id property value
        /// </summary>
        /// <returns></returns>
        public TKey GetIdValue();

        /// <summary>
        /// Get Id property name (should be something like => nameof(IdField))
        /// </summary>
        /// <returns></returns>
        public string GetIdPropertyName();
    }
}