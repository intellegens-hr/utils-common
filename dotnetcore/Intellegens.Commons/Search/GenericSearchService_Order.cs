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

        private (string expression, object[] arguments) CombineQueryPartsAndArgumentsAsHitCount(IEnumerable<(string expression, object[] arguments)> queryParts, LogicOperators logicalOperator)
        {
            var parameters = new List<object>();
            string query = "";

            List<(string expression, object[] arguments)> queryPartsFiltered = queryParts
                .Where(x => !string.IsNullOrEmpty(x.expression))
                .Select(x => ((x.expression.Contains(" ? 1 : 0") || x.expression.Contains(" ? 0 : 1")) ? x.expression : $"({x.expression} ? 1 : 0)", x.arguments))
                .ToList();

            if (queryPartsFiltered.Any())
            {
                query = string.Join($" + ", queryPartsFiltered.Select(x => x.expression));
                queryPartsFiltered.Select(x => x.arguments).ToList().ForEach(x => parameters.AddRange(x));
                query = $"( {query} )";
            }

            return (query, parameters.ToArray());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="queryCombineFunction">Function used to combine multiple queries into one</param>
        /// <returns></returns>
        private (string expression, object[] parameters) ProcessCriteriaOrderBy(SearchCriteria searchCriteria)
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
                var criterias = searchCriteria.Criteria.Select(x => ProcessCriteriaOrderBy(x));
                combinedQueryParts = CombineQueryPartsAndArgumentsAsHitCount(criterias, searchCriteria.CriteriaLogic);
            }
            else if (keysOrValuesDefined)
            {
                var values = searchCriteria.Values ?? new List<string>();

                var keys = searchCriteria.Keys ?? new List<string>();

                var expressions = keys
                    .Select(key => GetFilterExpression(key, values, searchCriteria.Operator, searchCriteria.ValuesLogic))
                    .ToList();

                combinedQueryParts = CombineQueryPartsAndArgumentsAsHitCount(expressions, searchCriteria.KeysLogic);
            }

            // When expression is concatenated, it must be wrapped in brackets with optional NOT (!) in front
            if (!string.IsNullOrEmpty(combinedQueryParts.expression))
            {
                combinedQueryParts.expression = $" ({combinedQueryParts.expression}) ";
                if (searchCriteria.Negate)
                    combinedQueryParts.expression = combinedQueryParts.expression.Replace("? 1 : 0", "? 0 : 1");
            }

            return combinedQueryParts;
        }

        /// <summary>
        /// Apply OrderBy to query
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        protected IQueryable<T> OrderQuery(IQueryable<T> sourceData, SearchRequest searchRequest)
        {
            var orderByItems = searchRequest
                .Order
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .ToList();

            bool firstOrderByPassed = false;

            if (searchRequest.OrderByMatchCount)
            {
                var (expression, parameters) = ProcessCriteriaOrderBy(searchRequest);
                if (!string.IsNullOrEmpty(expression))
                {
                    string expressionWithParamsReplaced = ReplaceParametersPlaceholder(expression);
                    sourceData = sourceData.OrderBy(parsingConfig, $"{expressionWithParamsReplaced} desc", parameters);
                    firstOrderByPassed = true;
                }
            }

            foreach (var item in orderByItems)
            {
                var ordering = GetOrderingString(item);

                if (firstOrderByPassed)
                    sourceData = (sourceData as IOrderedQueryable<T>).ThenBy(ordering);
                else
                    sourceData = sourceData.OrderBy(ordering);

                firstOrderByPassed = true;
            }

            return sourceData;
        }
    }
}