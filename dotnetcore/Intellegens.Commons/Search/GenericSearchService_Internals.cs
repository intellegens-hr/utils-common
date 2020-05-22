using Intellegens.Commons.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Intellegens.Commons.Search
{
    public partial class GenericSearchService<T>
        where T : class
    {
        // Using dynamic query exposes a possibility of sql injection.
        // If fieldname contains anything but underscore, letters and numbers - it's invalid
        private void ValidateDynamicLinqFieldName(string key)
        {
            var isNameValid = key.All(c => Char.IsLetterOrDigit(c) || c.Equals('_') || c.Equals('.'));
            if (!isNameValid)
                throw new Exception("Possible SQL Injection!");
        }

        /// <summary>
        /// Used to name SQL parameters
        /// </summary>
        private int parameterCounter = 0;

        private string GetOrderingString(SearchOrder order)
            => $"{order.Key} {(order.Ascending ? "ascending" : "descending")}";

        /// <summary>
        /// Dynamic Linq needs this to know where to look for EF functions
        /// </summary>
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

        /// <summary>
        /// If needed, casts filter value to property type.
        /// Doesn't throw exception in case of invalid values since some search methods don't mind it. For example,
        /// 123 is not valid Guid in exact match, but is in partial match
        /// </summary>
        /// <param name="filterValueType">Filtered property type</param>
        /// <param name="filterValue">Filter value</param>
        /// <returns>Tuple containing casted filter value and info if conversion was successful</returns>
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

        /// <summary>
        /// Apply OrderBy to query
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns dynamic query expression and parameters for given filter and it's property
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="filteredProperty"></param>
        /// <returns></returns>
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
                    // NpgsqlDbFunctionsExtensions.ILike
                    expression = $"(({filteredProperty.Name} != null) AND (DbFunctionsExtensions.Like(EF.Functions, string(object({filteredProperty.Name})), \"%{filter.Value}%\")))";
                    break;

                default:
                    throw new NotImplementedException();
            }

            return (expression, arguments.ToArray());
        }

        /// <summary>
        /// Combine multiple expressions and parameter arrays into single
        /// </summary>
        /// <param name="queryParts">List containing expression/arguments pairs</param>
        /// <param name="separator">Separator to use for queries (AND/OR)</param>
        /// <returns>Single tuple containing expression and all arguments</returns>
        private (string expression, object[] arguments) CombineQueryPartsAndArguments(List<(string expression, object[] arguments)> queryParts, string separator)
        {
            var parameters = new List<object>();
            string query = "";

            if (queryParts.Any())
            {
                query = string.Join($" {separator} ", queryParts.Select(x => x.expression));
                queryParts.Select(x => x.arguments).ToList().ForEach(x => parameters.AddRange(x));
            }

            return (query, parameters.ToArray());
        }

        /// <summary>
        /// Build search query from SearchRequest object
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        protected IQueryable<T> BuildSearchQuery(IQueryable<T> sourceData, SearchRequest searchRequest)
        {
            // get all defined filters
            var filters = searchRequest.Filters.ToList();

            // for each filter, get WHERE clause part and query parameters (if any)
            var queryParams = new List<(string query, object[] queryParams)>();
            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];

                // check if query contains any SQL injection potential
                ValidateDynamicLinqFieldName(filter.Key);

                // get property for given filter key
                var prop = TypeUtils.GetProperty<T>(filter.Key, StringComparison.OrdinalIgnoreCase);

                // resolve IN as multiple equal
                if (filter.ValuesIn?.Any() ?? false)
                {
                    var queryInListParams = filter.ValuesIn?
                        .Select(x => GetFilterExpression(SearchFilter.ExactMatch(filter.Key, x), prop))
                        .ToList();
                    var (expression, arguments) = CombineQueryPartsAndArguments(queryInListParams, "||");
                    queryParams.Add(($"({expression})", arguments));
                }

                // resolve NOT IN as multiple not equal
                if (filter.ValuesNotIn?.Any() ?? false)
                {
                    var queryNotInListParams = filter.ValuesNotIn?
                        .Select(x => GetFilterExpression(SearchFilter.ExactMatch(filter.Key, x), prop))
                        .ToList();
                    var (expression, arguments) = CombineQueryPartsAndArguments(queryNotInListParams, "&&");
                    queryParams.Add(($"({expression.Replace("==", "!=")})", arguments));
                }

                if (string.IsNullOrEmpty(filter.Value))
                    continue;

                queryParams.Add(GetFilterExpression(filter, prop));
            };

            // get all WHERE parts, define separator and join them together
            var separator = searchRequest.Type == FilterTypes.OR ? "||" : "&&";

            if (queryParams.Any())
            {
                var (expression, arguments) = CombineQueryPartsAndArguments(queryParams, separator);
                sourceData = sourceData.Where(parsingConfig, expression, arguments);
            }

            sourceData = OrderQuery(sourceData, searchRequest);
            return sourceData;
        }
    }
}