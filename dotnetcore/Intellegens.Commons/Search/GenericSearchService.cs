using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Threading.Tasks;

namespace Intellegens.Commons.Search
{
    public partial class GenericSearchService<T>
        where T : class
    {
        public Task<List<T>> Search(IEnumerable<T> sourceData, SearchRequest searchRequest)
            => Search(sourceData.AsQueryable(), searchRequest);

        public Task<List<T>> Search(IQueryable<T> sourceData, SearchRequest searchRequest)
        {
            var queryFiltered = BuildSearchQuery(sourceData, searchRequest);

            return queryFiltered
                .Skip(searchRequest.Offset)
                .Take(searchRequest.Limit)
                .ToListAsync();
        }

        public async Task<(int count, List<T> data)> SearchAndCount(IQueryable<T> sourceData, SearchRequest searchRequest)
        {
            var count = await BuildSearchQuery(sourceData, searchRequest).CountAsync();
            var data = await Search(sourceData, searchRequest);

            return (count, data);
        }

        public Task<(int count, List<T> data)> SearchAndCount(IEnumerable<T> sourceData, SearchRequest searchRequest)
            => SearchAndCount(sourceData.AsQueryable(), searchRequest);

        private object GetPropertyValue(PropertyInfo[] properties, string propertyName, T entity)
        => properties.Where(p => p.Name.ToLower() == propertyName.ToLower()).First().GetValue(entity);

        public async Task<int> IndexOf(string keyColumn, IQueryable<T> sourceData, T entity, SearchRequest searchRequest)
        {
            var query = BuildSearchQuery(sourceData, searchRequest);

            var properties = typeof(T).GetProperties();

            var entityKeyValue = GetPropertyValue(properties, keyColumn, entity);

            if (searchRequest.Ordering.Any())
            {
                var order = searchRequest.Ordering.First();
                var queryOperator = order.Ascending ? "<" : ">";

                var entityOrderColValue = GetPropertyValue(properties, order.Key, entity);
                query = query.Where($"({order.Key} {queryOperator} @0) || ({order.Key} == @0 && {keyColumn} < @1)", entityOrderColValue, entityKeyValue);
            }
            else
            {
                query = query.Where($" {keyColumn} < @0", entityKeyValue);
            }

            return await query.CountAsync();
        }
    }
}