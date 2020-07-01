using Intellegens.Commons.Search.Models;
using Intellegens.Commons.Types;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Intellegens.Commons.Search
{
    public partial class GenericSearchService<T>
        where T : class, new()
    {
        // this is used for order by match count. If matched - add -1, else 0 since ascending sort is used
        // for some reason, sort didn't work when DESC was used in combination with AutoMapper
        private const string exprIfTrueThen1 = " ? 1 : 0 ";
        private const string exprIfTrueThen0 = " ? 0 : 1 ";

        /// <summary>
        /// For given SearchOrder model, generates string to place in OrderBy
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
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

            return $"it.{string.Join(".", pathSegmentsResolved)}{new String(')', bracketsOpen)} {(order.Ascending ? "ASC" : "DESC")}";
        }

        /// <summary>
        /// Combines multiple query parts into one in order to use them in order by match count.
        /// Different query parts should be connected with "+" sign
        /// </summary>
        /// <param name="queryParts"></param>
        /// <param name="logicalOperator"></param>
        /// <returns></returns>
        private (string expression, object[] arguments) CombineQueryPartsAndArgumentsAsHitCount(IEnumerable<(string expression, object[] arguments)> queryParts, LogicOperators logicalOperator)
        {
            var parameters = new List<object>();
            string query = "";

            // Take all expressions and add IIF( ? 1 : 0) if IIF is not already present (? 1 : 0 or ? 0 : 1 if entire expression was negated
            // at some point
            List<(string expression, object[] arguments)> queryPartsFiltered = new List<(string expression, object[] arguments)>();
            foreach(var (expression, arguments) in queryParts)
            {
                var expressionReplaced = expression.Trim();

                if (expressionReplaced.Contains(".Any("))
                {
                    // Child queries will always look like:
                    // (it.Children.Any(xyz0 => (xyz0.Text != null && NpgsqlDbFunctionsExtensions.ILike(EF.Functions, xyz0.Text, @@Parameter@@))))

                    // first, remove outer brackets
                    while (expressionReplaced.StartsWith('('))
                        expressionReplaced = expressionReplaced[1..^1].Trim();

                    // last open bracket is part of Any() expression so:
                    // concatenate expression up to last bracket, add "? 1: 0" expression, close bracket and then wrap entire expression in brackets
                    // in case SUM turns out as null, results will be invalid (entire mathc count chain will be null). 
                    // For that reason, we cast sum result as int? and coallesce it to 0
                    expressionReplaced = $"(int?({expressionReplaced[0..^1]} {exprIfTrueThen1})) ?? 0)";


                    // replace ".Any(" with ".Sum(" - these expressions will never occur in string in any other way (parameters are bound)
                    // so it's safe just to replace them
                    expressionReplaced = expressionReplaced.Replace(".Any(", ".Sum(");
                }

                // if expression doesn't have coallesce (? 1: 0 / ? 0 : 1) or .Sum operator -> add colaesce operator
                if (!(expressionReplaced.Contains(exprIfTrueThen0) || expressionReplaced.Contains(exprIfTrueThen1) || expressionReplaced.Contains(".Sum(")))
                {
                    expressionReplaced = $"({expressionReplaced} {exprIfTrueThen1})";
                }

                queryPartsFiltered.Add((expressionReplaced, arguments));
            }

            // Combines multiple query parts with + operator
            if (queryPartsFiltered.Any())
            {
                query = string.Join($" + ", queryPartsFiltered.Select(x => x.expression));
                queryPartsFiltered.Select(x => x.arguments).ToList().ForEach(x => parameters.AddRange(x));
                query = $"( {query} )";
            }

            return (query, parameters.ToArray());
        }

        /// <summary>
        /// Process criteria for Order by match count.
        /// Very similar to ProcessCriteria but uses different method for combining multiple criteria
        /// and has different negation logic
        /// </summary>
        /// <param name="searchCriteria"></param>
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
            // if nested filters are defined, process them recursively
            if (nestedFiltersDefined)
            {
                var criterias = searchCriteria.Criteria.Select(x => ProcessCriteriaOrderBy(x));
                combinedQueryParts = CombineQueryPartsAndArgumentsAsHitCount(criterias, searchCriteria.CriteriaLogic);
            }
            // if keys and values are defined (but not nested filters)
            // build expression for given keys, values and operator
            else if (keysOrValuesDefined)
            {
                var values = searchCriteria.Values ?? new List<string>();

                var keys = searchCriteria.Keys ?? new List<string>();

                var expressions = keys
                    .Select(key => GetFilterExpression(key, values, searchCriteria.Operator, searchCriteria.ValuesLogic))
                    .ToList();

                combinedQueryParts = CombineQueryPartsAndArgumentsAsHitCount(expressions, searchCriteria.KeysLogic);
            }

            if (!string.IsNullOrEmpty(combinedQueryParts.expression))
            {
                combinedQueryParts.expression = $" ({combinedQueryParts.expression}) ";

                // when switching one expression to other and back, one of these expressions must be stored as something else
                const string switchReplacementValue = ">>*?TempReplacement?*<<";

                // if entire expression must be negated, this means that expression inside brackets needs to be
                // inverted: "? 1 : 0" to "? 0 : 1" and vice-versa
                if (searchCriteria.Negate) { 
                    combinedQueryParts.expression = combinedQueryParts
                        .expression
                        .Replace(exprIfTrueThen1, switchReplacementValue) // replacement value is not a valid string and will not already be inside expression
                        .Replace(exprIfTrueThen0, exprIfTrueThen1)
                        .Replace(switchReplacementValue, exprIfTrueThen0);
                }
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
            List<string> orderByExpressions = new List<string>();
            List<object> orderByParameters = new List<object>();

            // if order by math count was set, generate expression and use it as first order by
            if (searchRequest.OrderByMatchCount)
            {
                var (expression, parameters) = ProcessCriteriaOrderBy(searchRequest);
                if (!string.IsNullOrEmpty(expression))
                {
                    string expressionWithParamsReplaced = ReplaceParametersPlaceholder(expression).Trim();
                    orderByExpressions.Add($"{expressionWithParamsReplaced} DESC");
                    orderByParameters.AddRange(parameters);
                }
            }

            // add all other orderbys specified in request model
            var orderByItems = searchRequest
                .Order
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .Select(x => GetOrderingString(x));

            orderByExpressions.AddRange(orderByItems);

            // dynamic Linq enables multiple order bys as comma separated values
            if (orderByExpressions.Any())
                sourceData = sourceData.OrderBy(parsingConfig, string.Join(", ", orderByExpressions), orderByParameters.ToArray());

            return sourceData;
        }
    }
}