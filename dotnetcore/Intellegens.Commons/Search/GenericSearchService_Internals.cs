using Intellegens.Commons.Search.Models;
using Intellegens.Commons.Types;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Intellegens.Commons.Search
{
    public partial class GenericSearchService<T>
        where T : class, new()
    {
        private readonly Dictionary<LogicOperators, string> csharpOperatorsMap = new Dictionary<LogicOperators, string>
        {
            { LogicOperators.ALL, "&&" },
            { LogicOperators.ANY, "||" }
        };

        /// <summary>
        /// Map between FilterMatchTypes enum and c sharp logical operators. Used when building expressions
        /// </summary>
        private readonly Dictionary<Operators, string> filterMatchTypeToOperatorMap = new Dictionary<Operators, string>
        {
            { Operators.EQUALS, "==" },
            { Operators.LESS_THAN, "<" },
            { Operators.LESS_THAN_OR_EQUAL_TO, "<=" },
            { Operators.GREATER_THAN, ">" },
            { Operators.GREATER_THAN_OR_EQUAL_TO, ">=" }
        };

        private const string parameterPlaceholder = "@@Parameter@@";

        // Using dynamic query exposes a possibility of sql injection.
        // If fieldname contains anything but underscore, letters and numbers - it's invalid
        private void ValidateDynamicLinqFieldName(string key)
        {
            var isNameValid = key.All(c => Char.IsLetterOrDigit(c) || c.Equals('_') || c.Equals('.'));
            if (!isNameValid)
                throw new Exception("Possible SQL Injection!");
        }

        private string GetOrderingString(SearchOrder order)
        {
            var propertyChainInfo = TypeUtils.GetPropertyInfoPerPathSegment<T>(order.Key).ToList();
            // this part will split entire path:
            // if input path is a.b.c -> output will be it.a.b.c == value
            // if input path is a.b[].c -> output will be it.a.b.Any(xyz1 => xyz1.c == value)
            List<string> pathSegmentsResolved = new List<string>();
            int bracketsOpen = 0;
            for (int i = 0; i < propertyChainInfo.Count(); i++)
            {
                var (_, propertyInfo, isCollectionType) = propertyChainInfo[i];
                if (isCollectionType)
                {
                    pathSegmentsResolved.Add($"{propertyInfo.Name}.Min(xyz{i} => xyz{i}");
                    bracketsOpen++;
                }
                else
                {
                    pathSegmentsResolved.Add(propertyInfo.Name);
                }
            }

            return $"it.{string.Join(".", pathSegmentsResolved)}{new String(')', bracketsOpen)} {(order.Ascending ? "ascending" : "descending")}";
        }

        /// <summary>
        /// Dynamic Linq needs this to know where to look for EF functions
        /// </summary>
        private static readonly ParsingConfig parsingConfig = new ParsingConfig
        {
            CustomTypeProvider = new DynamicLinqProvider()
        };

        private static readonly HashSet<Type> IntTypes = new HashSet<Type>
        {
            typeof(short), typeof(ushort), typeof(int), typeof(uint),
             typeof(sbyte), typeof(byte)
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
        private static (bool isInvalid, dynamic value) ParseFilterValue(Type filterValueType, string filterValue)
        {
            dynamic filterValueParsed = filterValue;
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
                filterInvalid = !Decimal.TryParse(filterValue, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal parsedDecimal);
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
                .Order
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

        private bool IsSearchCriteriaEmpty(SearchCriteria searchCriteria)
        {
            var keysOrValuesEmpty = !((searchCriteria.Keys?.Any() ?? false) || (searchCriteria.Values?.Any() ?? false));
            var nestedFiltersEmpty = !(searchCriteria.Criteria?.Any() ?? false);

            return keysOrValuesEmpty && nestedFiltersEmpty;
        }

        private (string expression, object[] parameters) GenerateWhereCriteria(SearchRequest searchRequest)
        {
            // if no filters specified
            if (IsSearchCriteriaEmpty(searchRequest))
                return ("", null);

            return ProcessSearchCriteria(searchRequest);
        }

        private (string expression, object[] parameters) ProcessSearchCriteria(SearchCriteria searchCriteria)
        {
            var keysOrValuesDefined = (searchCriteria.Keys?.Any() ?? false) || (searchCriteria.Values?.Any() ?? false);
            var nestedFiltersDefined = searchCriteria.Criteria?.Any() ?? false;

            // if keys are not defined and values are - this is full text search
            if (!searchCriteria.Keys.Any() && searchCriteria.Values.Any())
            {
                searchCriteria.Keys = FullTextSearchPaths;
                searchCriteria.KeysLogic = LogicOperators.ANY;
            }

            // if nested filters are defined and keys/values as well - keys and values will be treated as another SearchCriteria
            if (keysOrValuesDefined && nestedFiltersDefined)
            {
                searchCriteria.Criteria.Add(new SearchCriteria
                {
                    Keys = searchCriteria.Keys,
                    KeysLogic = searchCriteria.KeysLogic,
                    Operator = searchCriteria.Operator,
                    Values = searchCriteria.Values,
                    ValuesLogic = searchCriteria.ValuesLogic
                });
            }
            (string expression, object[] arguments) combinedQueryParts = ("", null);
            if (nestedFiltersDefined)
            {
                var criterias = searchCriteria.Criteria.Select(x => ProcessSearchCriteria(x));
                combinedQueryParts = CombineQueryPartsAndArguments(criterias, searchCriteria.CriteriaLogic);
            }
            else if (keysOrValuesDefined)
            {
                var values = searchCriteria.Values ?? new List<string>();

                var keys = searchCriteria.Keys ?? new List<string>();

                var expressions = keys
                    .Select(key => GetFilterExpression(key, values, searchCriteria.Operator, searchCriteria.ValuesLogic));

                combinedQueryParts = CombineQueryPartsAndArguments(expressions, searchCriteria.KeysLogic);
            }

            // When expression is concatenated, it must be wrapped in brackets with optional NOT (!) in front
            if (!string.IsNullOrEmpty(combinedQueryParts.expression))
            {
                var operatorEquality = searchCriteria.Negate ? "!" : "";
                combinedQueryParts.expression = $" {operatorEquality}({combinedQueryParts.expression}) ";
            }

            return combinedQueryParts;
        }

        /// <summary>
        /// Get exact match filter expression and parameter
        /// </summary>
        /// <param name="filterKey"></param>
        /// <param name="currentFilterStringValue">if filter has multiple values, this is current</param>
        /// <returns></returns>
        private (string expression, object parameter) GetOperatorMatchExpression(string filterKey, Operators matchType, string currentFilterStringValue)
        {
            var propertyChainInfo = TypeUtils.GetPropertyInfoPerPathSegment<T>(filterKey).ToList();
            var filteredPropertyType = propertyChainInfo.Last().propertyInfo.PropertyType;

            var (isNullable, resolvedType) = filteredPropertyType.ResolveNullableType();
            filteredPropertyType = resolvedType;

            (bool filterHasInvalidValue, dynamic filterValue) = ParseFilterValue(filteredPropertyType, currentFilterStringValue);
            if (filterHasInvalidValue)
                throw new Exception("Invalid filter value!");

            // this part will split entire path:
            // if input path is a.b.c -> output will be it.a.b.c == value
            // if input path is a.b[].c -> output will be it.a.b.Any(xyz1 => xyz1.c == value)
            List<string> pathSegmentsResolved = new List<string>();
            int bracketsOpen = 0;
            for (int i = 0; i < propertyChainInfo.Count(); i++)
            {
                var (_, propertyInfo, isCollectionType) = propertyChainInfo[i];
                if (isCollectionType)
                {
                    pathSegmentsResolved.Add($"{propertyInfo.Name}.Any(xyz{i} => xyz{i}");
                    bracketsOpen++;
                }
                else
                {
                    pathSegmentsResolved.Add(propertyInfo.Name);
                }
            }

            var matchOperator = filterMatchTypeToOperatorMap[matchType];
            var expression = $"it.{string.Join(".", pathSegmentsResolved)} {matchOperator} {parameterPlaceholder} {new String(')', bracketsOpen)}";

            return (expression, filterValue);
        }

        /// <summary>
        /// Get exact match filter expression and parameter
        /// </summary>
        /// <param name="filterKey"></param>
        /// <param name="currentFilterStringValue">if filter has multiple values, this is current</param>
        /// <param name="likeArgument"></param>
        /// <returns></returns>
        private (string expression, string parameter) GetLikeExpression(string filterKey, string likeString)
        {
            // for entire property path, get type data
            var propertyChainInfo = TypeUtils.GetPropertyInfoPerPathSegment<T>(filterKey).ToList();
            var filteredPropertyType = propertyChainInfo.Last().propertyInfo.PropertyType;

            // check if type can be null and resolve it's underlying type (in case Null<T>)
            var (isNullableType, resolvedType) = filteredPropertyType.ResolveNullableType();
            filteredPropertyType = resolvedType;

            if (filteredPropertyType == typeof(bool))
            {
                throw new Exception("Bool value can't be partially matched!");
            }

            // define like function
            string likeFunction = "DbFunctionsExtensions.Like";

            // in case of string search, postgres uses ILIKE operator to do case insensitive search
            if (filteredPropertyType == typeof(string) && genericSearchConfig.DatabaseProvider == SearchDatabaseProviders.POSTGRES)
            {
                likeFunction = "NpgsqlDbFunctionsExtensions.ILike";
            }

            // this part will split entire path:
            // if input path is a.b.c -> output will be DBFunctions.Like(EFFunction, it.a.b.c, expression)
            // if input path is a.b[].c -> output will be it.a.b.Any(x => DBFunctions.Like(EFFunction, x.c, expression))
            // if input path is a.b[].c.d -> output will be it.a.b.Any(x => DBFunctions.Like(EFFunction, x.c.d, expression))
            string lastExpressionVariable = "it."; // contains last expression variable use - this will be used in DbFunction.Like call
            var lastCollectionIndex = propertyChainInfo.Select(x => x.isCollectionType).ToList().LastIndexOf(true); // build .Any up to last collection, path segments after that will go into DbFunctions call
            int bracketsOpen = 0;
            string likeExpression = "";

            // if collection exists, we must build expression which starts at "it." (current instance)
            // if collection does not exist, this will go directly into DbFunction call (DbFunctions.Like(it....))
            if (lastCollectionIndex > -1)
                likeExpression = "it.";

            // if input path is a.b[].c -> this part will produce it.a.b.Any(x =>
            // this loop will build entire path, up to last collection. If collection is not in path - this will be skipped
            for (int i = 0; i <= lastCollectionIndex; i++)
            {
                var (_, propertyInfo, isCollectionType) = propertyChainInfo[i];
                if (isCollectionType)
                {
                    var expr = $"{propertyInfo.Name}.Any(xyz{i} => ";
                    lastExpressionVariable = $"xyz{i}.";

                    // last segment will contain DbFunctions call which has expression variable sa argument
                    if (i != lastCollectionIndex)
                        expr += lastExpressionVariable;

                    likeExpression += expr;
                    bracketsOpen++;
                }
                else
                {
                    likeExpression += $"{propertyInfo.Name}.";
                }
            }

            // this part will add DBFunctions.Like(EFFunction, x.c, expression))
            // segments after last collection (or all segments if there is no collection) must go inside DbFunctions call
            var segmentPathAfterLastCollection = string.Join(".", propertyChainInfo.Skip(lastCollectionIndex + 1).Select(x => x.propertyInfo.Name));

            // specify argument for LIKE function. By default its it./xyz0./xyz1./... + segment after last collection in path (or
            // entire path if there is no collection)
            // if we don't deal with string value, we must pack/unpack it force EF to pass it as an argument. LIKE function on database
            // must work with any value, not just string. More details:
            // https://stackoverflow.com/a/56718249
            var likeArgument = $"{lastExpressionVariable}{segmentPathAfterLastCollection}";
            if (filteredPropertyType != typeof(string))
            {
                likeArgument = $"string(object({likeArgument}))";
            }

            //
            string notNullExpression = "";
            if (isNullableType)
                notNullExpression = $"{likeArgument} != null && ";

            // at this point like expression will either be empty string or something like "it.a.b.Any(xyz0 => "
            // we'll add to id:
            // - likeFunction- Like function (Like/ILike)
            // - likeArgument - expression
            // - parameterPlaceholder - search expression
            // - brackets - number of brackets that need to be closed
            string expression = likeExpression + $" {notNullExpression} {likeFunction}(EF.Functions, {likeArgument}, {parameterPlaceholder}){new string(')', bracketsOpen)}";

            return (expression, likeString);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filterStringValue"></param>
        /// <returns></returns>
        private string GetWildcardLikeExpression(string filterStringValue)
        {
            return filterStringValue
                .Replace("%", "\\%")
                .Replace("_", "\\_")
                .Replace("*", "%")
                .Replace("?", "_");
        }

        /// <summary>
        /// Returns dynamic query expression and parameters for given filter and it's property
        /// </summary>
        /// <param name="currentKey">SearchFilter has multiple keys, this method parses only given key</param>
        /// <param name="filter"></param>
        /// <param name="filteredProperty"></param>
        /// <returns></returns>
        private (string expression, object[] arguments) GetFilterExpression(string filterKey, List<string> values, Operators searchOperator, LogicOperators logicOperator)
        {
            ValidateDynamicLinqFieldName(filterKey);

            var arguments = new List<object>();
            var expressions = new List<string>();

            foreach (string filterStringValue in values)
            {
                (string expression, object argument) matchResult;

                switch (searchOperator)
                {
                    case Operators.STRING_CONTAINS:
                        string likeExpression = $"%{filterStringValue}%";
                        matchResult = GetLikeExpression(filterKey, likeExpression);
                        break;

                    case Operators.STRING_WILDCARD:
                        string wildcardExpression = GetWildcardLikeExpression(filterStringValue);
                        matchResult = GetLikeExpression(filterKey, wildcardExpression);
                        break;

                    case Operators.EQUALS:
                    case Operators.GREATER_THAN:
                    case Operators.GREATER_THAN_OR_EQUAL_TO:
                    case Operators.LESS_THAN:
                    case Operators.LESS_THAN_OR_EQUAL_TO:
                        matchResult = GetOperatorMatchExpression(filterKey, searchOperator, filterStringValue);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                expressions.Add(matchResult.expression);
                arguments.Add(matchResult.argument);
            }

            var expressionsConcatenated = string.Join($" {csharpOperatorsMap[logicOperator]} ", expressions);
            return (expressionsConcatenated, arguments.ToArray());
        }

        /// <summary>
        /// Combine multiple expressions and parameter arrays into single
        /// </summary>
        /// <param name="queryParts">List containing expression/arguments pairs</param>
        /// <param name="separator">Separator to use for queries (AND/OR)</param>
        /// <returns>Single tuple containing expression and all arguments</returns>
        private (string expression, object[] arguments) CombineQueryPartsAndArguments(IEnumerable<(string expression, object[] arguments)> queryParts, LogicOperators logicalOperator)
        {
            var parameters = new List<object>();
            string query = "";

            var queryPartsFiltered = queryParts.Where(x => !string.IsNullOrEmpty(x.expression)).ToList();

            if (queryPartsFiltered.Any())
            {
                query = string.Join($" {csharpOperatorsMap[logicalOperator]} ", queryPartsFiltered.Select(x => x.expression));
                queryPartsFiltered.Select(x => x.arguments).ToList().ForEach(x => parameters.AddRange(x));
                query = $"( {query} )";
            }

            return (query, parameters.ToArray());
        }
    }
}