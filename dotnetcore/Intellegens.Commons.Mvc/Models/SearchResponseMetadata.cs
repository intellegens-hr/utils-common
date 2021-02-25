using Intellegens.Commons.Search.Models;

namespace Intellegens.Commons.Mvc.Models
{
    public class SearchResponseMetadata
    {
        // echo of sent request
        public SearchRequest Request { get; set; }

        // total record count, nullable in case we do queries on large tables and don't want to calculate row count
        public int? TotalRecordCount { get; set; }
    }
}