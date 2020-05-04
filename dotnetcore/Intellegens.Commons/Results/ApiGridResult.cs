using Intellegens.Commons.Search;

namespace Intellegens.Commons.Results
{
    public class ApiGridResult<T> : ApiResult<T>
        where T : class
    {
        public SearchResponseMetadata Metadata { get; set; }
    }

    public class SearchResponseMetadata
    {
        // total record count, nullable in case we do queries on large tables and don't want to calculate row count
        public int? TotalRecordCount { get; set; }

        // echo of sent request
        public SearchRequest Request { get; set; }
    }
}