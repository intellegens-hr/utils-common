using System.Collections.Generic;

namespace Intellegens.Commons.Search.Models
{
    public class SearchRequest : SearchCriteria
    {
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 10;
        public List<SearchOrder> Order { get; set; } = new List<SearchOrder>();
    }
}