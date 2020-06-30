using Intellegens.Commons.Search.Models;
using Intellegens.Commons.Types;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Intellegens.Commons.Search
{
    public partial class GenericSearchService<T>
        where T : class, new()
    {
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

            return ProcessCriteriaSearch(searchRequest);
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
        ///
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="queryCombineFunction">Function used to combine multiple queries into one</param>
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
                var criterias = searchCriteria.Criteria.Select(x => ProcessCriteriaSearch(x));
                combinedQueryParts = CombineQueryPartsAndArguments(criterias, searchCriteria.CriteriaLogic);
            }
            else if (keysOrValuesDefined)
            {
                var values = searchCriteria.Values ?? new List<string>();

                var keys = searchCriteria.Keys ?? new List<string>();

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