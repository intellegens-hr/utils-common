using System.Collections.Generic;

namespace Intellegens.Commons.Search
{
    /// <summary>
    /// Enum with all possible (implemented) filter operators
    /// </summary>
    public enum FilterMatchOperators
    {
        EQUALS,
        STRING_CONTAINS,
        STRING_WILDCARD,
        LESS_THAN,
        LESS_THAN_OR_EQUAL_TO,
        GREATER_THAN,
        GREATER_THAN_OR_EQUAL_TO,
        FULL_TEXT_SEARCH
    }

    /// <summary>
    /// Used to specify filter, search and order parameteres when using GenericSearchService
    /// </summary>
    public class SearchRequest
    {
        /// <summary>
        /// Number of record to skip
        /// </summary>
        public int Offset { get; set; } = 0;

        /// <summary>
        /// Number of records to return
        /// </summary>
        public int Limit { get; set; } = 10;

        /// <summary>
        /// Filters have AND operator between them
        /// </summary>
        public List<SearchFilter> Filters { get; set; } = new List<SearchFilter>();
        /// <summary>
        /// Search have OR operator between them
        /// </summary>
        public List<SearchFilter> Search { get; set; } = new List<SearchFilter>(); 
        
        /// <summary>
        /// Sort definition, currently only 1 order is possible, others are ignored
        /// </summary>
        public List<SearchOrder> Ordering { get; set; } = new List<SearchOrder>();
    }


    public class SearchFilter
    {
        public static SearchFilter PartialMatch(string key, string value)
            => new SearchFilter { Keys = new List<string> { key }, Values = new List<string> { value }, Operator = FilterMatchOperators.STRING_CONTAINS };

        public static SearchFilter Equal(string key, string value)
            => new SearchFilter { Keys = new List<string> { key }, Values = new List<string> { value }, Operator = FilterMatchOperators.EQUALS };

        /// <summary>
        /// List of keys (properties) to match
        /// </summary>
        public List<string> Keys { get; set; }

        /// <summary>
        /// Filter operator
        /// </summary>
        public FilterMatchOperators Operator { get; set; } = FilterMatchOperators.STRING_CONTAINS;

        /// <summary>
        /// Should entire filter expression be negated
        /// </summary>
        public bool NegateExpression { get; set; } = false;

        /// <summary>
        /// Values to match
        /// </summary>
        public List<string> Values { get; set; }
    }

    public class SearchOrder
    {
        public static SearchOrder AsAscending(string fieldName)
            => new SearchOrder
            {
                Ascending = true,
                Key = fieldName
            };

        public static SearchOrder AsDescending(string fieldName)
            => new SearchOrder
            {
                Ascending = false,
                Key = fieldName
            };

        /// <summary>
        /// Order key (property)
        /// </summary>
        public string Key { get; set; }
        public bool Ascending { get; set; } = true;
    }
}