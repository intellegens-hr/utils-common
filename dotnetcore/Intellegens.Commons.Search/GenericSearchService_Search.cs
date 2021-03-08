using Intellegens.Commons.Search.Models;
using Intellegens.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Intellegens.Commons.Search
{
    public partial class GenericSearchService<T>
        where T : class, new()
    {
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

            // ignore empty expression (they should not be here anyway)
            var queryPartsFiltered = queryParts.Where(x => !string.IsNullOrEmpty(x.expression)).ToList();

            // in case query parts are present - join them depending on operator
            if (queryPartsFiltered.Any())
            {
                query = string.Join($" {csharpOperatorsMap[logicalOperator]} ", queryPartsFiltered.Select(x => x.expression));
                queryPartsFiltered.Select(x => x.arguments).ToList().ForEach(x => parameters.AddRange(x));
                query = $"( {query} )";
            }

            return (query, parameters.ToArray());
        }

        /// <summary>
        /// Method to use when parsing SearhRequest from beginning
        /// </summary>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        private (string expression, object[] parameters) GenerateWhereCriteria(SearchRequest searchRequest)
        {
            // if no filters specified
            if (IsSearchCriteriaEmpty(searchRequest))
                return ("", null);

            return ProcessCriteriaSearch(searchRequest);
        }

        /// <summary>
        /// Retursn single expression and arguments to use
        /// </summary>
        /// <param name="filterKey"></param>
        /// <param name="values"></param>
        /// <param name="searchOperator"></param>
        /// <param name="logicOperator"></param>
        /// <returns></returns>
        private (string expression, object[] arguments) GetFilterExpression(string filterKey, IEnumerable<string> values, Operators searchOperator, LogicOperators logicOperator)
        {
            var (expressions, arguments) = GetFilterExpressions(filterKey, values, searchOperator);

            // concatenate all expressions
            var expressionsConcatenated = string.Join($" {csharpOperatorsMap[logicOperator]} ", expressions);
            return (expressionsConcatenated, arguments.ToArray());
        }

        /// <summary>
        /// Returns dynamic query expressions and parameters for given filter and it's property
        /// </summary>
        /// <param name="filterKey">SearchFilter has multiple keys, this method parses only given key</param>
        /// <param name="values"></param>
        /// <param name="searchOperator"></param>
        /// <returns>List of expressions and arguments to be used</returns>
        private (List<string> expressions, object[] arguments) GetFilterExpressions(string filterKey, IEnumerable<string> values, Operators searchOperator)
        {
            // Check for SQL injection
            ValidateDynamicLinqFieldName(filterKey);

            var arguments = new List<object>();
            var expressions = new List<string>();

            // go through all values and based on operator and logic operator, build expression
            foreach (string filterStringValue in values)
            {
                (string expression, object argument) matchResult;

                switch (searchOperator)
                {
                    // uses like
                    case Operators.STRING_CONTAINS:
                        string likeExpression = $"%{filterStringValue}%";
                        matchResult = GetLikeExpression(filterKey, likeExpression);
                        break;

                    // uses like
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

            return (expressions, arguments.ToArray());
        }

        /// <summary>
        /// Get filter expression for given operator and parameter
        /// </summary>
        /// <param name="filterKey"></param>
        /// /// <param name="matchType"></param>
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

            // get c# operator
            var matchOperator = filterMatchTypeToOperatorMap[matchType];
            var expression = $"it.{string.Join(".", pathSegmentsResolved)} {matchOperator} {parameterPlaceholder} {new String(')', bracketsOpen)}";

            return (expression, filterValue);
        }

        /// <summary>
        /// Check if given criteria is empty (no keys or values present, no filters present)
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        private bool IsSearchCriteriaEmpty(SearchCriteria searchCriteria)
        {
            var keysOrValuesEmpty = !((searchCriteria.Keys?.Any() ?? false) || (searchCriteria.Values?.Any() ?? false));
            var nestedFiltersEmpty = !(searchCriteria.Criteria?.Any() ?? false);

            return keysOrValuesEmpty && nestedFiltersEmpty;
        }

        /// <summary>
        /// Process Criteria - called recursively if needed
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        private (string expression, object[] parameters) ProcessCriteriaSearch(SearchCriteria searchCriteria)
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
                searchCriteria.Criteria = searchCriteria.Criteria.Append(new SearchCriteria
                {
                    Keys = searchCriteria.Keys,
                    KeysLogic = searchCriteria.KeysLogic,
                    Operator = searchCriteria.Operator,
                    Values = searchCriteria.Values,
                    ValuesLogic = searchCriteria.ValuesLogic
                });
            }
            (string expression, object[] arguments) combinedQueryParts = ("", null);
            // if nested filters are defined - process them
            if (nestedFiltersDefined)
            {
                var criterias = searchCriteria.Criteria.Select(x => ProcessCriteriaSearch(x));
                combinedQueryParts = CombineQueryPartsAndArguments(criterias, searchCriteria.CriteriaLogic);
            }
            // when no nested filters are defined - build filter expressions and combine them
            else if (keysOrValuesDefined)
            {
                var values = searchCriteria.Values ?? Enumerable.Empty<string>();
                var keys = searchCriteria.Keys ?? Enumerable.Empty<string>();

                var expressions = keys
                    .Select(key => GetFilterExpression(key, values, searchCriteria.Operator, searchCriteria.ValuesLogic))
                    .ToList();

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
    }
}