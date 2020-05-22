using Intellegens.Commons.Types;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Reflection;
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
            var isNameValid = key.All(c => Char.IsLetterOrDigit(c) || c.Equals('_') || c.Equals('.'));
            if (!isNameValid)
                throw new Exception("Possible SQL Injection!");
        }

        private int parameterCounter = 0;

        private string GetOrderingString(SearchOrder order)
            => $"{order.Key} {(order.Ascending ? "ascending" : "descending")}";

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

        private static (bool isInvalid, object value) ParseFilterValue(Type filterValueType, string filterValue)
        {
            object filterValueParsed = filterValue;
            bool filterInvalid = false;

            // try parse filter value. Parsed filter value is used for exact search
            if (filterValueType == typeof(System.Guid))
            {
                filterInvalid = !Guid.TryParse(filterValue, out Guid _);
            }
            else if (filterValueType == typeof(DateTime))
            {
                filterInvalid = !DateTime.TryParse(filterValue, out DateTime parsedDate);
                if (!filterInvalid)
                    filterValueParsed = parsedDate;
            }
            else if (filterValueType == typeof(bool))
            {
                filterInvalid = !bool.TryParse(filterValue, out bool parsedPool);
                if (!filterInvalid)
                    filterValueParsed = parsedPool;
            }
            else if (IntTypes.Contains(filterValueType))
            {
                filterInvalid = !Int32.TryParse(filterValue, out int parsedInt);
                if (!filterInvalid)
                    filterValueParsed = parsedInt;
            }
            else if (DecimalTypes.Contains(filterValueType))
            {
                filterInvalid = !Decimal.TryParse(filterValue, out decimal parsedDecimal);
                if (!filterInvalid)
                    filterValueParsed = parsedDecimal;
            }

            return (filterInvalid, filterValueParsed);
        }

        protected IQueryable<T> OrderQuery(IQueryable<T> sourceData, SearchRequest searchRequest)
        {
            var order = searchRequest
                .Ordering
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .ToList();

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

        private (string expression, object[] arguments) GetFilterExpression(SearchFilter filter, PropertyInfo filteredProperty)
        {
            var filteredPropertyType = filteredProperty.PropertyType;

            var nullableType = Nullable.GetUnderlyingType(filteredPropertyType);
            if (nullableType != null)
            {
                // filterPropTypeIsNullable = true;
                filteredPropertyType = nullableType;
            }

            (bool filterHasInvalidValue, object filterValue) = ParseFilterValue(filteredPropertyType, filter.Value);
            var arguments = new List<object>();
            string expression;

            switch (filter.Type)
            {
                case FilterMatchTypes.EXACT_MATCH:
                    if (filterHasInvalidValue)
                        throw new Exception("Invalid filter value!");

                    expression = $"{filteredProperty.Name} == @{parameterCounter++}";
                    arguments.Add(filterValue);
                    break;

                case FilterMatchTypes.PARTIAL_MATCH:
                    if (filteredProperty.PropertyType == typeof(bool))
                    {
                        throw new Exception("Bool value can't be partially matched!");
                    }

                    // https://stackoverflow.com/a/56718249
                    expression = $"(({filteredProperty.Name} != null) AND (DbFunctionsExtensions.Like(EF.Functions, string(object({filteredProperty.Name})), \"%{filter.Value}%\")))";
                    break;

                default:
                    throw new NotImplementedException();
            }

            return (expression, arguments.ToArray());
        }

        // TODO: When we stabilize search, this needs to be cleaned up and tests added
        protected IQueryable<T> BuildSearchQuery(IQueryable<T> sourceData, SearchRequest searchRequest)
        {
            // get all defined filters
            var filters = searchRequest.Filters.Where(x => !string.IsNullOrEmpty(x.Value)).ToList();

            // for each filter, get WHERE clause part and query parameters (if any)
            var queryParams = new List<(string query, object[] queryParams)>();
            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];

                // check if query contains any SQL injection potential
                ValidateDynamicLinqFieldName(filter.Key);

                // get property for given filter key
                var prop = TypeUtils.GetProperty<T>(filter.Key, StringComparison.OrdinalIgnoreCase);
                queryParams.Add(GetFilterExpression(filter, prop));
            };

            // get all WHERE parts, define separator and join them together
            var queryParts = queryParams.Select(x => x.query).Where(x => !string.IsNullOrEmpty(x)).ToList();
            if (queryParts.Any())
            {
                var separator = searchRequest.Type == FilterTypes.OR ? "||" : "&&";
                var query = string.Join($" {separator} ", queryParts);

                // get all query parameters and add them to parameters list
                var parameters = new List<object>();
                queryParams.Select(x => x.queryParams).ToList().ForEach(x => parameters.AddRange(x));

                sourceData = sourceData.Where(parsingConfig, query, parameters.ToArray());
            }

            sourceData = OrderQuery(sourceData, searchRequest);
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