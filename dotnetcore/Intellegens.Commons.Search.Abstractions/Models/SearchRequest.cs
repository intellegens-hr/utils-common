using System.Collections.Generic;
using System.Linq;

namespace Intellegens.Commons.Search.Models
{
    public class SearchRequest : SearchCriteria
    {
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 10;
        public IEnumerable<SearchOrder> Order { get; set; } = Enumerable.Empty<SearchOrder>();
        public bool OrderByMatchCount { get; set; }
    }
}