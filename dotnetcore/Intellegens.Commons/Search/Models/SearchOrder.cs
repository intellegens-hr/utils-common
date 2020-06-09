namespace Intellegens.Commons.Search.Models
{
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