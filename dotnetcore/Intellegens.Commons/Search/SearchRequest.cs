using System.Collections.Generic;

namespace Intellegens.Commons.Search
{
    public class SearchRequest
    {
        public int Offset { get; set; } = 0; // Starting record
        public int Limit { get; set; } = 10; // Number of records to return

        public List<SearchFilter> Filters { get; set; } = new List<SearchFilter>();
        public List<SearchOrder> Ordering { get; set; } = new List<SearchOrder>();
    }

    public enum FilterTypes { EXACT_MATCH, PARTIAL_MATCH, WILDCARD, REGEX }

    public class SearchFilter
    {
        public static SearchFilter PartialMatch(string key, string value)
            => new SearchFilter { Key = key, Value = value, Type = FilterTypes.PARTIAL_MATCH };

        public static SearchFilter ExactMatch(string key, string value)
            => new SearchFilter { Key = key, Value = value, Type = FilterTypes.EXACT_MATCH };

        public string Key { get; set; }

        //For future's sake, will define type of filtering
        public FilterTypes Type { get; set; } = FilterTypes.PARTIAL_MATCH;

        // search value
        public string Value { get; set; }
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