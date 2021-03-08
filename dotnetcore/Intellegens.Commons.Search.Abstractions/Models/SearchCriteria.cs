using System.Collections.Generic;
using System.Linq;

namespace Intellegens.Commons.Search.Models
{
    public enum LogicOperators
    {
        ANY = 0,
        ALL = 1
    }

    /// <summary>
    /// Enum with all possible (implemented) filter operators
    /// </summary>
    public enum Operators
    {
        EQUALS,
        STRING_CONTAINS,
        STRING_WILDCARD,
        LESS_THAN,
        LESS_THAN_OR_EQUAL_TO,
        GREATER_THAN,
        GREATER_THAN_OR_EQUAL_TO
    }

    public class SearchCriteria
    {
        public IEnumerable<SearchCriteria> Criteria { get; set; } = Enumerable.Empty<SearchCriteria>();

        public LogicOperators CriteriaLogic { get; set; } = LogicOperators.ALL;

        // Keys and what operator to place between them (key1 == xyz) AND/OR (key2 == xyz)
        public IEnumerable<string> Keys { get; set; } = Enumerable.Empty<string>();

        public LogicOperators KeysLogic { get; set; } = LogicOperators.ALL;

        public bool Negate { get; set; }

        public Operators Operator { get; set; } = Operators.STRING_CONTAINS;

        // Values and what operator to place between them ((key1 == xyz AND/OR key1 == xyz2) AND/OR (key2 == xyz))
        public IEnumerable<string> Values { get; set; } = Enumerable.Empty<string>();

        public LogicOperators ValuesLogic { get; set; } = LogicOperators.ANY;

        public static SearchCriteria Equal(string key, string value)
            => new SearchCriteria
            {
                Keys = new string[] { key },
                Values = new string[] { value },
                Operator = Operators.EQUALS
            };

        public static SearchCriteria PartialMatch(string key, string value)
            => new SearchCriteria
            {
                Keys = new string[] { key },
                Values = new string[] { value },
                Operator = Operators.STRING_CONTAINS
            };
    }
}