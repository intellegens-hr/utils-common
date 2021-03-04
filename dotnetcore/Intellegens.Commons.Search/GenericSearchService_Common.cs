using Intellegens.Commons.Utils;
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Intellegens.Commons.Search
{
    public partial class GenericSearchService<T>
        where T : class, new()
    {
        protected virtual DynamicLinqProvider DynamicLinqProvider
            => new DynamicLinqProvider();

        /// <summary>
        /// Dynamic Linq needs this to know where to look for EF functions
        /// </summary>
        private ParsingConfig ParsingConfig
            => new ParsingConfig
            {
                CustomTypeProvider = DynamicLinqProvider
            };

        protected virtual string GetLikeFunctionName(Type filteredPropertyType)
        {
            return "DbFunctionsExtensions.Like";
        }

        /// <summary>
        /// If needed, casts filter value to property type.
        /// Doesn't throw exception in case of invalid values since some search methods don't mind it. For example,
        /// 123 is not valid Guid in exact match, but is in partial match
        /// </summary>
        /// <param name="filterValueType">Filtered property type</param>
        /// <param name="filterValue">Filter value</param>
        /// <returns>Tuple containing casted filter value and info if conversion was successful</returns>
        protected (bool isInvalid, dynamic value) ParseFilterValue(Type filterValueType, string filterValue)
        {
            dynamic filterValueParsed = filterValue;
            bool filterInvalid = false;

            // try parse filter value. Parsed filter value is used for exact search
            if (filterValueType == typeof(Guid))
            {
                filterInvalid = !Guid.TryParse(filterValue, out Guid _);
            }
            else if (filterValueType == typeof(DateTime))
            {
                filterInvalid = !DateTime.TryParse(filterValue, out DateTime parsedDate);
                if (!filterInvalid)
                    filterValueParsed = parsedDate.ToUniversalTime();
            }
            else if (filterValueType == typeof(bool))
            {
                filterInvalid = !bool.TryParse(filterValue, out bool parsedPool);
                if (!filterInvalid)
                    filterValueParsed = parsedPool;
            }
            else if (IntTypes.Contains(filterValueType))
            {
                filterInvalid = !int.TryParse(filterValue, out int parsedInt);
                if (!filterInvalid)
                    filterValueParsed = parsedInt;
            }
            else if (DecimalTypes.Contains(filterValueType))
            {
                filterInvalid = !decimal.TryParse(filterValue, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal parsedDecimal);
                if (!filterInvalid)
                    filterValueParsed = parsedDecimal;
            }

            return (filterInvalid, filterValueParsed);
        }

        /// <summary>
        /// Will go through entire string expression (it.prop1 == @@Parameter@@ && it.prop2 == @@Parameter@@)
        /// and replace all @@Parameter@@ with @P0, @P1, ...
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static string ReplaceParametersPlaceholder(string expression)
        {
            // expression contains parameter placeholder defined in const parameterPlaceholder
            // each query must contain parameters in following pattern: @0, @1, ...
            // we need to replace placeholder with this kind of expression
            var expressionParamParts = expression.Split(parameterPlaceholder);
            var expressionWithParamsReplaced = expressionParamParts[0];

            for (int i = 1; i < expressionParamParts.Length; i++)
            {
                string expr = expressionParamParts[i];
                expressionWithParamsReplaced += $"@{i - 1}{expr}"; // parameters start from @0
            }

            return expressionWithParamsReplaced;
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
            string likeFunction = GetLikeFunctionName(filteredPropertyType);

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
            string expression = likeExpression + $" ({notNullExpression} {likeFunction}(EF.Functions, {likeArgument}, {parameterPlaceholder}){new string(')', bracketsOpen)})";

            return ($"({expression})", likeString);
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
    }
}