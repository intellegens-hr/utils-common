using System.Collections.Generic;

namespace Intellegens.Commons.Search
{
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

    public class SearchRequest
    {
        public int Offset { get; set; } = 0; // Starting record
        public int Limit { get; set; } = 10; // Number of records to return

        public List<SearchFilter> Filters { get; set; } = new List<SearchFilter>(); // AND
        public List<SearchFilter> Search { get; set; } = new List<SearchFilter>(); // OR
        public List<SearchOrder> Ordering { get; set; } = new List<SearchOrder>();
    }

    public class SearchFilter
    {
        public static SearchFilter PartialMatch(string key, string value)
            => new SearchFilter { Keys = new List<string> { key }, Values = new List<string> { value }, Operator = FilterMatchOperators.STRING_CONTAINS };

        public static SearchFilter Equal(string key, string value)
            => new SearchFilter { Keys = new List<string> { key }, Values = new List<string> { value }, Operator = FilterMatchOperators.EQUALS };

        public List<string> Keys { get; set; }

        //For future's sake, will define type of filtering
        public FilterMatchOperators Operator { get; set; } = FilterMatchOperators.STRING_CONTAINS;

        public bool NegateExpression { get; set; } = false;

        // search value(s)
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

        public string Key { get; set; }
        public bool Ascending { get; set; } = true;
    }
}