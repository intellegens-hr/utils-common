using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Threading.Tasks;

namespace Intellegens.Commons.Search
{
    public class GenericSearchService<T>
        where T : class
    {
        // Using dynamic query eyposes a possibility of sql injection.
        // If fieldname contains anything but underscore, letters and numbers - it's invalid
        private void ValidateDynamicLinqFieldName(string key)
        {
            var isNameValid = key.All(c => Char.IsLetterOrDigit(c) || c.Equals('_'));
            if (!isNameValid)
                throw new Exception("Possible SQL Injection!");
        }

        private string GetOrderingString(SearchOrder order)
            => $"{order.Key} {(order.Ascending ? "asc" : "desc")}";

        private static readonly ParsingConfig parsingConfig = new ParsingConfig
        {
            CustomTypeProvider = new DynamicLinqProvider()
        };

        private static readonly HashSet<Type> IntTypes = new HashSet<Type>
        {
            typeof(short), typeof(ushort), typeof(int), typeof(uint)
        };

        private static readonly HashSet<Type> DecimalTypes = new HashSet<Type>
        {
            typeof(decimal), typeof(float)
        };

        // TODO: When we stabilize search, this needs to be cleaned up and tests added
        private IQueryable<T> BuildSearchQuery(IQueryable<T> sourceData, SearchRequest searchRequest)
        {
            var order = searchRequest
                .Ordering
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .ToList();

            var filters = searchRequest.Filters.Where(x => !string.IsNullOrEmpty(x.Value)).ToList();

            // build where
            filters.ForEach(filter =>
            {
                ValidateDynamicLinqFieldName(filter.Key);

                // TODO: Cache
                var prop = typeof(T).GetProperties().Where(x => x.Name.Equals(filter.Key, StringComparison.OrdinalIgnoreCase)).First();
                var filterPropType = prop.PropertyType;
                var filterPropTypeIsNullable = false;

                // in case prop type is DateTime?, we flag it as nullable and set type to DateTime
                var nullableType = Nullable.GetUnderlyingType(filterPropType);
                if (nullableType != null)
                {
                    filterPropTypeIsNullable = true;
                    filterPropType = nullableType;
                }

                object filterValue = filter.Value;
                bool filterInvalid = false;

                // try parse filter value. Parsed filter value is used for exact search
                if (filterPropType == typeof(System.Guid))
                {
                    filterInvalid = !Guid.TryParse(filter.Value, out Guid _);
                }
                else if (filterPropType == typeof(DateTime))
                {
                    filterInvalid = !DateTime.TryParse(filter.Value, out DateTime parsedDate);
                    if (!filterInvalid)
                        filterValue = parsedDate;
                }
                else if (filterPropType == typeof(bool))
                {
                    filterInvalid = !bool.TryParse(filter.Value, out bool parsedPool);
                    if (!filterInvalid)
                        filterValue = parsedPool;
                }
                else if (IntTypes.Contains(filterPropType))
                {
                    filterInvalid = !Int32.TryParse(filter.Value, out int parsedInt);
                    if (!filterInvalid)
                        filterValue = parsedInt;
                }
                else if (DecimalTypes.Contains(filterPropType))
                {
                    filterInvalid = !Decimal.TryParse(filter.Value, out decimal parsedDecimal);
                    if (!filterInvalid)
                        filterValue = parsedDecimal;
                }

                switch (filter.Type)
                {
                    case FilterTypes.EXACT_MATCH:
                        if (filterInvalid)
                            throw new Exception("Invalid filter value!");

                        sourceData = sourceData.Where($"{prop.Name} == @0", filterValue);
                        break;

                    case FilterTypes.PARTIAL_MATCH:
                        if (filterPropType == typeof(bool))
                        {
                            throw new Exception("Bool value can't be partially matched!");
                        }

                        // https://stackoverflow.com/a/56718249
                        sourceData = sourceData.Where($"{prop.Name} != null");
                        sourceData = sourceData.Where(parsingConfig, $"DbFunctionsExtensions.Like(EF.Functions, string(object({prop.Name})), \"%{filter.Value}%\")");
                        break;

                    default:
                        throw new NotImplementedException();
                }
            });

            // build order by
            if (order.Any())
            {
                var firstOrdering = GetOrderingString(order[0]);
                sourceData = sourceData.OrderBy(firstOrdering);

                for (var i = 1; i < order.Count; i++)
                    sourceData = (sourceData as IOrderedQueryable<T>).ThenBy(GetOrderingString(order[i]));
            }

            return sourceData;
        }

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
    }

    public class DynamicLinqProvider : IDynamicLinkCustomTypeProvider
    {
        public HashSet<Type> GetCustomTypes()
        {
            HashSet<Type> types = new HashSet<Type>
            {
                typeof(EF),
                typeof(DbFunctionsExtensions)
            };
            return types;
        }

        public Type ResolveType(string typeName)
        {
            throw new NotImplementedException();
        }

        public Type ResolveTypeBySimpleName(string simpleTypeName)
        {
            throw new NotImplementedException();
        }
    }
}