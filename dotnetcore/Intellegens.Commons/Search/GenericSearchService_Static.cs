using Intellegens.Commons.Search.Models;
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
        /// Used as placeholder when building queries
        /// </summary>
        private const string parameterPlaceholder = "@@Parameter@@";

        /// <summary>
        /// Map between logic operators and c# logical operators.
        /// </summary>
        private static readonly Dictionary<LogicOperators, string> csharpOperatorsMap = new Dictionary<LogicOperators, string>
        {
            { LogicOperators.ALL, "&&" },
            { LogicOperators.ANY, "||" }
        };

        /// <summary>
        /// Map between FilterMatchTypes enum and c# logical operators. Used when building expressions
        /// </summary>
        private static readonly Dictionary<Operators, string> filterMatchTypeToOperatorMap = new Dictionary<Operators, string>
        {
            { Operators.EQUALS, "==" },
            { Operators.LESS_THAN, "<" },
            { Operators.LESS_THAN_OR_EQUAL_TO, "<=" },
            { Operators.GREATER_THAN, ">" },
            { Operators.GREATER_THAN_OR_EQUAL_TO, ">=" }
        };

        /// <summary>
        /// Using dynamic query exposes a possibility of sql injection.
        /// If fieldname contains anything but underscore, letters and numbers - it's invalid
        /// </summary>
        /// <param name="key"></param>
        private static void ValidateDynamicLinqFieldName(string key)
        {
            var isNameValid = key.All(c => Char.IsLetterOrDigit(c) || c.Equals('_') || c.Equals('.'));
            if (!isNameValid)
                throw new Exception("Possible SQL Injection!");
        }

        /// <summary>
        /// All integer types, used when parsing filter value
        /// </summary>
        private static readonly HashSet<Type> IntTypes = new HashSet<Type>
        {
            typeof(short), typeof(ushort), typeof(int), typeof(uint),
             typeof(sbyte), typeof(byte)
        };

        /// <summary>
        /// All decimal types, used when parsing filter value
        /// </summary>
        private static readonly HashSet<Type> DecimalTypes = new HashSet<Type>
        {
            typeof(decimal), typeof(float)
        };

        /// <summary>
        /// Dynamic Linq needs this to know where to look for EF functions
        /// </summary>
        private static readonly ParsingConfig parsingConfig = new ParsingConfig
        {
            CustomTypeProvider = new DynamicLinqProvider()
        };

        /// <summary>
        /// When processing criteria, multiple criteria needs to be combined as one string
        /// This enum represents possibly combination methods
        /// </summary>
        private enum QueryCombinationTypes
        {
            /// <summary>
            /// Combines multiple queries with logic operators
            /// </summary>
            SEARCH,
            /// <summary>
            /// Combines multiple queries with (expr) ? 1 : 0 which is used in order by match count
            /// </summary>
            ORDER_BY_MATCH_COUNT
        }
    }
}