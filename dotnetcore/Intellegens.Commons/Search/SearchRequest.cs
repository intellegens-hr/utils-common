//using Intellegens.Commons.Search.Models;
//using System.Collections.Generic;

//namespace Intellegens.Commons.Search
//{
//    /// <summary>
//    /// Used to specify filter, search and order parameteres when using GenericSearchService
//    /// </summary>
//    public class SearchRequest
//    {
//        /// <summary>
//        /// Number of record to skip
//        /// </summary>
//        public int Offset { get; set; } = 0;

//        /// <summary>
//        /// Number of records to return
//        /// </summary>
//        public int Limit { get; set; } = 10;

//        /// <summary>
//        /// Filters have AND operator between them
//        /// </summary>
//        public List<SearchFilter> Filters { get; set; } = new List<SearchFilter>();

//        /// <summary>
//        /// Search have OR operator between them
//        /// </summary>
//        public List<SearchFilter> Search { get; set; } = new List<SearchFilter>();

//        /// <summary>
//        /// Sort definition, currently only 1 order is possible, others are ignored
//        /// </summary>
//        public List<SearchOrder> Ordering { get; set; } = new List<SearchOrder>();
//    }

//    public class SearchFilter
//    {
//        public static SearchFilter PartialMatch(string key, string value)
//            => new SearchFilter { Keys = new List<string> { key }, Values = new List<string> { value }, Operator = SearchOperators.STRING_CONTAINS };

//        public static SearchFilter Equal(string key, string value)
//            => new SearchFilter { Keys = new List<string> { key }, Values = new List<string> { value }, Operator = SearchOperators.EQUALS };

//        /// <summary>
//        /// List of keys (properties) to match
//        /// </summary>
//        public List<string> Keys { get; set; }

//        /// <summary>
//        /// Filter operator
//        /// </summary>
//        public SearchOperators Operator { get; set; } = SearchOperators.STRING_CONTAINS;

//        /// <summary>
//        /// Should entire filter expression be negated
//        /// </summary>
//        public bool NegateExpression { get; set; } = false;

//        /// <summary>
//        /// Values to match
//        /// </summary>
//        public List<string> Values { get; set; }
//    }


//}