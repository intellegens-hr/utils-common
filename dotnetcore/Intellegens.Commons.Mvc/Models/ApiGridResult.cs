namespace Intellegens.Commons.Mvc.Models
{
    /// <summary>
    /// Used by endpoints which provide data pagination
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiGridResult<T> : ApiResult<T>
        where T : class
    {
        public SearchResponseMetadata Metadata { get; set; }
    }
}