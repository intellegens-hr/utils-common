using Intellegens.Commons.Search.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellegens.Commons.Search
{
    public interface IGenericSearchService<T> where T : class, new()
    {
        /// <summary>
        /// When using full-text search, this list will contain all required paths to compare
        /// </summary>
        IEnumerable<string> FullTextSearchPaths { get; set; }

        /// <summary>
        /// Build search query from SearchRequest object
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        IQueryable<T> FilterQuery(IQueryable<T> sourceData, SearchRequest searchRequest);

        /// <summary>
        /// Find index of given key element
        /// </summary>
        /// <param name="keyColumn"></param>
        /// <param name="sourceData"></param>
        /// <param name="entity"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        Task<int> IndexOf(string keyColumn, IQueryable<T> sourceData, T entity, SearchRequest searchRequest);

        /// <summary>
        /// Search on Enumerable type
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        Task<IList<T>> Search(IEnumerable<T> sourceData, SearchRequest searchRequest);

        /// <summary>
        /// Search on Queryable data source
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        Task<IList<T>> Search(IQueryable<T> sourceData, SearchRequest searchRequest);

        /// <summary>
        /// Search and return total matching record count
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        Task<(int count, IEnumerable<T> data)> SearchAndCount(IEnumerable<T> sourceData, SearchRequest searchRequest);

        /// <summary>
        /// Search and return total matching record count
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        Task<(int count, IEnumerable<T> data)> SearchAndCount(IQueryable<T> sourceData, SearchRequest searchRequest);
    }
}