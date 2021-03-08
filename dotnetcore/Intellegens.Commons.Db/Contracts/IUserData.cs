namespace Intellegens.Commons.Db.Contracts
{
    /// <summary>
    /// This interface is used by database context to set tracking data to entity
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IUserData<TKey>
    {
        /// <summary>
        /// Function returning current user id
        /// </summary>
        /// <returns></returns>
        TKey GetUserId();
    }
}