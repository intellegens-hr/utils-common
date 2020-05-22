using System.Collections.Generic;

namespace Intellegens.Commons.Search
{
    public enum FilterTypes { AND, OR }

    public enum FilterMatchTypes { EXACT_MATCH, PARTIAL_MATCH, WILDCARD, REGEX }

    public enum CaseSensitivity { CASE_SENSITIVE, CASE_INSENSITIVE }

    public class SearchRequest<T>
    {
        public FilterTypes Type { get; set; } = FilterTypes.AND;
        public CaseSensitivity CaseSensitivity { get; set; } = CaseSensitivity.CASE_SENSITIVE;
        public int Offset { get; set; } = 0; // Starting record
        public int Limit { get; set; } = 10; // Number of records to return

        public List<SearchFilter> Filters { get; set; } = new List<SearchFilter>();
        public List<SearchOrder> Ordering { get; set; } = new List<SearchOrder>();
    }

    public class SearchRequest : SearchRequest<string> { }

    public class SearchFilter<T>
    {
        public static SearchFilter PartialMatch(string key, string value)
            => new SearchFilter { Key = key, Value = value, Type = FilterMatchTypes.PARTIAL_MATCH };

        public static SearchFilter ExactMatch(string key, string value)
            => new SearchFilter { Key = key, Value = value, Type = FilterMatchTypes.EXACT_MATCH };

        public string Key { get; set; }

        //For future's sake, will define type of filtering
        public FilterMatchTypes Type { get; set; } = FilterMatchTypes.PARTIAL_MATCH;

        // search value
        public string Value { get; set; }

        // Search only
        public List<T> ValuesIn { get; set; }

        // Values to exclude
        public List<T> ValuesNotIn { get; set; }
    }

    public class SearchFilter : SearchFilter<string> { }

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