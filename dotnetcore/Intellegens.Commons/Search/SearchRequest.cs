using System.Collections.Generic;

namespace Intellegens.Commons.Search
{
    

    public enum FilterMatchTypes { EXACT_MATCH, PARTIAL_MATCH, WILDCARD, REGEX }

    public enum ComparisonTypes { EQUAL, NOT_EQUAL }

    public class SearchRequest
    {
        public int Offset { get; set; } = 0; // Starting record
        public int Limit { get; set; } = 10; // Number of records to return

        public List<SearchFilter> Filters { get; set; } = new List<SearchFilter>(); // AND
        public List<SearchFilter> Search { get; set; } = new List<SearchFilter>(); // (OR)
        public List<SearchOrder> Ordering { get; set; } = new List<SearchOrder>();
    }

    public class SearchFilter
    {
        public static SearchFilter PartialMatch(string key, string value)
            => new SearchFilter { Key = key, Values = new List<string> { value }, Type = FilterMatchTypes.PARTIAL_MATCH };

        public static SearchFilter ExactMatch(string key, string value)
            => new SearchFilter { Key = key, Values = new List<string> { value }, Type = FilterMatchTypes.EXACT_MATCH };

        public string Key { get; set; }

        //For future's sake, will define type of filtering
        public FilterMatchTypes Type { get; set; } = FilterMatchTypes.PARTIAL_MATCH;

        public ComparisonTypes ComparisonType { get; set; } = ComparisonTypes.EQUAL;

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