namespace TaskFlowAPI.DTOs
{
    public class TaskQueryParameters
    {
        public bool? IsCompleted { get; set; }
        public int? CategoryId { get; set; }
        public string? SearchString { get; set; }
        public string? SortBy { get; set; }
        private int _page = 1;
        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }
        private int _pageSize;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : value > 50 ? 50 : value;
        }
    }
}
