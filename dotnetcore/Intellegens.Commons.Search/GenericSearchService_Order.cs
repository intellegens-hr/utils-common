using Intellegens.Commons.Search.Models;
using Intellegens.Commons.Utils;
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
        private const string exprIfTrueThen0 = " ? 0 : 1 ";

        // this is used for order by match count. If matched - add -1, else 0 since ascending sort is used
        // for some reason, sort didn't work when DESC was used in combination with AutoMapper
        private const string exprIfTrueThen1 = " ? 1 : 0 ";

        /// <summary>
        /// Apply OrderBy to query
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        protected IQueryable<T> OrderQuery(IQueryable<T> sourceData, SearchRequest searchRequest)
        {
            List<string> orderByExpressions = new();
            List<object> orderByParameters = new();

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
                sourceData = sourceData.OrderBy(ParsingConfig, string.Join(", ", orderByExpressions), orderByParameters.ToArray());

            return sourceData;
        }

        /// <summary>
        /// Combines multiple query parts into one in order to use them in order by match count.
        /// Different query parts should be connected with "+" sign
        ///
        /// DISCLAIMER: this method looks like hell, but it's job is simple, please follow comments beneath
        ///
        /// These call arguments (List<(List<string> expressions, object[] arguments)>) are used since they
        /// match output form when parsing entire Criteria object
        ///
        /// </summary>
        /// <param name="queryPartsSplit">these are </param>
        /// <param name="logicalOperator"></param>
        /// <returns></returns>
        private (string expression, object[] arguments) CombineQueryPartsAndArgumentsAsHitCount(IEnumerable<(IEnumerable<string> expressions, object[] arguments)> queryPartsSplit, LogicOperators logic, bool negate = false)
        {
            // output params
            var parameters = new List<object>();

            // list of queries to join as output
            List<string> queryParts = new List<string>();

            foreach (var (expressions, arguments) in queryPartsSplit)
            {
                // remove empty expressions (if any)
                var expressionsFiltered = expressions.Where(x => !string.IsNullOrEmpty(x));

                // this method can be called on expressions that were already combined. Since we manipulate expressions (Any -> Sum, ...), these manipulations
                // need to be done only once, on first run
                // if expressions contain .Sum or "? 1 : 0" expressions, they were definitely manipulated
                var alreadyProcessed = expressionsFiltered.All(x => x.Contains(".Sum(") || (x.Contains("int?") && (x.Contains(exprIfTrueThen0) || x.Contains(exprIfTrueThen1))));

                // if expressions were already manipulated, they should simple be joined by + operator between them and wrapped in brackets
                if (alreadyProcessed)
                {
                    string joinExpression = $"({string.Join(")+(", expressionsFiltered)})";
                    queryParts.Add(joinExpression);
                    parameters.AddRange(arguments);
                }
                // if not, processing must be done
                // entire idea is to go from:
                // (it.SomeAttribute == @@PARAMETER@@) && (it.SomeAttribute == @@PARAMETER2@@)
                // to
                // ((it.SomeAttribute == @@PARAMETER@@) && (it.SomeAttribute == @@PARAMETER2@@)) ? (((it.SomeAttribute == @@PARAMETER@@) ? 1 : 0) + ((it.SomeAttribute == @@PARAMETER2@@) ? 1 : 0))
                else
                {
                    // determine operator to use
                    var operatorValues = csharpOperatorsMap[(LogicOperators)logic];

                    // this is same expression that would go into where clause
                    var countIfExpression = string.Join(operatorValues, expressionsFiltered.Select(x => $"({x})"));

                    // to get actual match count, things are bit more complicated since child queries can occur
                    var countExpressions = new List<string>();
                    foreach (var expressionIter in expressionsFiltered)
                    {
                        var expressionItem = expressionIter;
                        // by default, we simply take expression by expression and add (? 1 : 0) to them
                        // all expressions are wrapped into int? so generated SQL would have coallesce to zero if for
                        // any reason expression would turn out to be null.
                        // In case that happened, all subsequent sums would be null as well
                        string expressionToAdd = $"(int?(({expressionItem}){exprIfTrueThen1}) ?? 0)";

                        // so, if we have nested query, it'll always look like
                        // (it.Children.Any(xyz0 =>  (xyz0.Text != null &&  NpgsqlDbFunctionsExtensions.ILike(EF.Functions, xyz0.Text, @@Parameter@@))))
                        // and we want to get from that to:
                        // ((it.Children.Any(xyz0 =>  (xyz0.Text != null &&  NpgsqlDbFunctionsExtensions.ILike(EF.Functions, xyz0.Text, @@Parameter@@)))))
                        // ? ((it.Children.Sum(xyz0 =>  (xyz0.Text != null &&  NpgsqlDbFunctionsExtensions.ILike(EF.Functions, xyz0.Text, @@Parameter@@)) ? 1 : 0)))
                        // : 0
                        if (expressionItem.Contains(".Any("))
                        {
                            // first, remove outer brackets
                            while (expressionItem.StartsWith('('))
                                expressionItem = expressionItem[1..^1].Trim();

                            // last open bracket is part of Any() expression so:
                            // concatenate expression up to last bracket, add "? 1: 0" expression, close bracket and then wrap entire expression in brackets
                            // in case SUM turns out as null, results will be invalid (entire mathc count chain will be null).
                            // For that reason, we cast sum result as int? and coallesce it to 0
                            expressionItem = $"(int?({expressionItem[0..^1]} {exprIfTrueThen1})) ?? 0)";

                            // replace ".Any(" with ".Sum(" - these expressions will never occur in string in any other way (parameters are bound)
                            // so it's safe just to replace them
                            expressionItem = expressionItem.Replace(".Any(", ".Sum(");

                            expressionToAdd = expressionItem;
                        }

                        countExpressions.Add(expressionToAdd);
                    };

                    // now all that's left is to join expressions and we're done
                    var countExpression = string.Join('+', countExpressions);

                    var expression = $"(({countIfExpression}) ? ({countExpression}) : 0)";
                    List<object> argsDouble = new();
                    parameters.AddRange(arguments);
                    parameters.AddRange(arguments);
                    queryParts.Add(expression);
                }
            }

            // Combines multiple query parts with + operator
            string query = "";
            if (queryParts.Any())
            {
                query = string.Join($" + ", queryParts.Select(x => x));
                query = $"( {query} )";
            }

            return (query, parameters.ToArray());
        }

        /// <summary>
        /// This overload will be used when parsing filter as it's for is List of expressions (KEY1 == XXX, KEY2 == XXX, ...)
        /// </summary>
        /// <param name="expressions"></param>
        /// <param name="arguments"></param>
        /// <param name="logic"></param>
        /// <param name="negate"></param>
        /// <returns></returns>
        private (string expression, object[] arguments) CombineQueryPartsAndArgumentsAsHitCount(IEnumerable<string> expressions, object[] arguments, LogicOperators logic, bool negate = false)
        {
            var queryPartsTransformed = new (IEnumerable<string>, object[])[] { (expressions, arguments) };
            return CombineQueryPartsAndArgumentsAsHitCount(queryPartsTransformed, logic, negate);
        }

        /// <summary>
        /// This overload will be called on already parsed expressions which will always have one expressions and arguments array
        /// </summary>
        /// <param name="queryPartsSplit"></param>
        /// <param name="logic"></param>
        /// <param name="negate"></param>
        /// <returns></returns>
        private (string expression, object[] arguments) CombineQueryPartsAndArgumentsAsHitCount(IEnumerable<(string expressions, object[] arguments)> queryPartsSplit, LogicOperators logic, bool negate = false)
        {
            var argumentTransformed = queryPartsSplit
                .Select(x => (Enumerable.Empty<string>().Append(x.expressions), x.arguments));

            return CombineQueryPartsAndArgumentsAsHitCount(argumentTransformed, logic, negate);
        }

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
            List<string> pathSegmentsResolved = new();
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
            // if nested filters are defined, process them recursively
            if (nestedFiltersDefined)
            {
                var criterias = searchCriteria.Criteria
                    .Select(x => ProcessCriteriaOrderBy(x));

                // there is no values logic
                combinedQueryParts = CombineQueryPartsAndArgumentsAsHitCount(criterias, searchCriteria.CriteriaLogic);
            }
            // if keys and values are defined (but not nested filters)
            // build expression for given keys, values and operator
            else if (keysOrValuesDefined)
            {
                var values = searchCriteria.Values ?? new List<string>();

                var keys = searchCriteria.Keys ?? new List<string>();

                var expressions = keys
                    .Select(key => GetFilterExpressions(key, values, searchCriteria.Operator));

                var expressionsCombined = expressions
                    .Select(x => CombineQueryPartsAndArgumentsAsHitCount(x.expressions, x.arguments, searchCriteria.ValuesLogic));

                combinedQueryParts = CombineQueryPartsAndArgumentsAsHitCount(expressionsCombined, searchCriteria.KeysLogic);
            }

            if (!string.IsNullOrEmpty(combinedQueryParts.expression))
            {
                combinedQueryParts.expression = $" ({combinedQueryParts.expression}) ";

                // when switching one expression to other and back, one of these expressions must be stored as something else
                const string switchReplacementValue = ">>*?TempReplacement?*<<";

                // if entire expression must be negated, this means that expression inside brackets needs to be
                // inverted: "? 1 : 0" to "? 0 : 1" and vice-versa
                if (searchCriteria.Negate)
                {
                    combinedQueryParts.expression = combinedQueryParts
                        .expression
                        .Replace(exprIfTrueThen1, switchReplacementValue) // replacement value is not a valid string and will not already be inside expression
                        .Replace(exprIfTrueThen0, exprIfTrueThen1)
                        .Replace(switchReplacementValue, exprIfTrueThen0);
                }
            }

            return combinedQueryParts;
        }
    }
}