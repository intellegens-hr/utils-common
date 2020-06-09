using System.Collections.Generic;

namespace Intellegens.Commons.Search.Models
{
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

    public enum LogicOperators
    {
        ANY = 0,
        ALL = 1
    }

    public class SearchCriteria
    {
        public static SearchCriteria PartialMatch(string key, string value)
            => new SearchCriteria { Keys = new List<string> { key }, Values = new List<string> { value }, Operator = Operators.STRING_CONTAINS };
        public static SearchCriteria Equal(string key, string value)
            => new SearchCriteria { Keys = new List<string> { key }, Values = new List<string> { value }, Operator = Operators.EQUALS };

        // Keys and what operator to place between them (key1 == xyz) AND/OR (key2 == xyz)
        public List<string> Keys { get; set; } = new List<string>();

        public LogicOperators KeysLogic { get; set; } = LogicOperators.ALL;

        // Values and what operator to place between them ((key1 == xyz AND/OR key1 == xyz2) AND/OR (key2 == xyz))
        public List<string> Values { get; set; } = new List<string>();

        public LogicOperators ValuesLogic { get; set; } = LogicOperators.ANY;

        public Operators Operator { get; set; } = Operators.STRING_CONTAINS;
        public bool Negate { get; set; }

        public List<SearchCriteria> Criteria { get; set; } = new List<SearchCriteria>();
        public LogicOperators CriteriaLogic { get; set; } = LogicOperators.ALL;
    }
}