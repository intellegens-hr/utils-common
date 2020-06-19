using Intellegens.Commons.Search.FullTextSearch;
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
            var filtersParsed = GetQueryPartsAndParams(searchRequest.Filters, LogicalOperators.AND);
            var searchParsed = GetQueryPartsAndParams(searchRequest.Search, LogicalOperators.OR);

            // combine all query parts into single
            var filtersCombined = CombineQueryPartsAndArguments(filtersParsed, LogicalOperators.AND);
            var searchCombined = CombineQueryPartsAndArguments(searchParsed, LogicalOperators.OR);

            // combine filter and search
            var queryParts = new List<(string expression, object[] arguments)> { filtersCombined, searchCombined };
            var (expression, arguments) = CombineQueryPartsAndArguments(queryParts, LogicalOperators.AND);

            if (!string.IsNullOrEmpty(expression))
            {
                // expression contains parameter placeholder defined in const parameterPlaceholder
                // each query must contain parameters in following pattern: @0, @1, ...
                // we need to replace placeholder with this kind of expression
                var expressionParamParts = expression.Split(parameterPlaceholder);
                var expressionWithParamsReplaced = expressionParamParts[0];

                for (int i = 1; i < expressionParamParts.Length; i++)
                {
                    string expr = expressionParamParts[i];
                    expressionWithParamsReplaced += $"@{i - 1}{expr}"; // parameters start from @0
                }

                sourceData = sourceData.Where(parsingConfig, expressionWithParamsReplaced, arguments);
            }

            sourceData = OrderQuery(sourceData, searchRequest);

            return sourceData;
        }

        public Task<List<T>> Search(IQueryable<T> sourceData, SearchRequest searchRequest)
            => FilterQuery(sourceData, searchRequest)
                .Skip(searchRequest.Offset)
                .Take(searchRequest.Limit)
                .ToListAsync();

        public async Task<(int count, List<T> data)> SearchAndCount(IQueryable<T> sourceData, SearchRequest searchRequest)
        {
            var count = await FilterQuery(sourceData, searchRequest).CountAsync();
            var data = await Search(sourceData, searchRequest);

            return (count, data);
        }

        public Task<(int count, List<T> data)> SearchAndCount(IEnumerable<T> sourceData, SearchRequest searchRequest)
            => SearchAndCount(sourceData.AsQueryable(), searchRequest);

        private object GetPropertyValue(PropertyInfo[] properties, string propertyName, T entity)
        => properties.Where(p => p.Name.ToLower() == propertyName.ToLower()).First().GetValue(entity);

        public async Task<int> IndexOf(string keyColumn, IQueryable<T> sourceData, T entity, SearchRequest searchRequest)
        {
            var query = FilterQuery(sourceData, searchRequest);

            var properties = typeof(T).GetProperties();

            var entityKeyValue = GetPropertyValue(properties, keyColumn, entity);

            if (searchRequest.Ordering.Any())
            {
                var order = searchRequest.Ordering.First();
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