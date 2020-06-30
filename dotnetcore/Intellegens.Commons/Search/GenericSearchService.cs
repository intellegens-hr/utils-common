using Intellegens.Commons.Search.FullTextSearch;
using Intellegens.Commons.Search.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Threading.Tasks;

namespace Intellegens.Commons.Search
{
    /// <summary>
    /// Generic search services works on any IQueryable and provides simple (dynamic) filtering, search and ordering features on it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class GenericSearchService<T>
        where T : class, new()
    {
        private readonly IGenericSearchConfig genericSearchConfig;

        public GenericSearchService()
        {
            genericSearchConfig = new GenericSearchConfig { DatabaseProvider = SearchDatabaseProviders.SQLITE };
        }

        public GenericSearchService(IGenericSearchConfig genericSearchConfig)
        {
            this.genericSearchConfig = genericSearchConfig;
        }

        /// <summary>
        /// Search on Enumerable type
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        public Task<List<T>> Search(IEnumerable<T> sourceData, SearchRequest searchRequest)
            => Search(sourceData.AsQueryable(), searchRequest);

        /// <summary>
        /// When using full-text search, this list will contain all required paths to compare
        /// </summary>
        public List<string> FullTextSearchPaths { get; internal set; } = FullTextSearchExtensions.GetFullTextSearchPaths<T>();

        /// <summary>
        /// Build search query from SearchRequest object
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        public IQueryable<T> FilterQuery(IQueryable<T> sourceData, SearchRequest searchRequest)
        {
            // get all defined filters/search
            var (expression, arguments) = GenerateWhereCriteria(searchRequest);

            if (!string.IsNullOrEmpty(expression))
            {
                string expressionWithParamsReplaced = ReplaceParametersPlaceholder(expression);
                sourceData = sourceData.Where(parsingConfig, expressionWithParamsReplaced, arguments);
            }

            sourceData = OrderQuery(sourceData, searchRequest);

            return sourceData;
        }

        /// <summary>
        /// Search on Queryable data source
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        public Task<List<T>> Search(IQueryable<T> sourceData, SearchRequest searchRequest)
            => FilterQuery(sourceData, searchRequest)
                .Skip(searchRequest.Offset)
                .Take(searchRequest.Limit)
                .ToListAsync();

        /// <summary>
        /// Search and return total matching record count
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        public async Task<(int count, List<T> data)> SearchAndCount(IQueryable<T> sourceData, SearchRequest searchRequest)
        {
            var count = await FilterQuery(sourceData, searchRequest).CountAsync();
            var data = await Search(sourceData, searchRequest);

            return (count, data);
        }

        /// <summary>
        /// Search and return total matching record count
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        public Task<(int count, List<T> data)> SearchAndCount(IEnumerable<T> sourceData, SearchRequest searchRequest)
            => SearchAndCount(sourceData.AsQueryable(), searchRequest);

        /// <summary>
        /// Get value for specified property from entity
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="propertyName"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private object GetPropertyValue(PropertyInfo[] properties, string propertyName, T entity)
            => properties.Where(p => p.Name.ToLower() == propertyName.ToLower()).First().GetValue(entity);

        /// <summary>
        /// Find index of given key element
        /// </summary>
        /// <param name="keyColumn"></param>
        /// <param name="sourceData"></param>
        /// <param name="entity"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        public async Task<int> IndexOf(string keyColumn, IQueryable<T> sourceData, T entity, SearchRequest searchRequest)
        {
            var query = FilterQuery(sourceData, searchRequest);

            var properties = typeof(T).GetProperties();

            var entityKeyValue = GetPropertyValue(properties, keyColumn, entity);

            if (searchRequest.Order.Any())
            {
                var order = searchRequest.Order.First();
                var queryOperator = order.Ascending ? "<" : ">";

                var entityOrderColValue = GetPropertyValue(properties, order.Key, entity);
                query = query.Where($"(it.{order.Key} {queryOperator} @0) || (it.{order.Key} == @0 && it.{keyColumn} < @1)", entityOrderColValue, entityKeyValue);
            }
            else
            {
                query = query.Where($" it.{keyColumn} < @0", entityKeyValue);
            }

            return await query.CountAsync();
        }
    }
}