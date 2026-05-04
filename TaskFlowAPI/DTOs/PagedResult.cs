namespace TaskFlowAPI.DTOs
{
    public class PagedResult<T>
    {
        // Current page number
        public int Page { get; set; }
        //Number of items per page
        public int PageSize { get;set; }
        // Total number of matching records across all pages
        public int TotalCount { get; set; }
        // Total number of pages calculated from TotalCount and PageSize
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        // Whether a previous page exists
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public IEnumerable<T> Data { get; set; } = new List<T>();
    }
}
