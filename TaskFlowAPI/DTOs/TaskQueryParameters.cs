namespace TaskFlowAPI.DTOs
{
    public class TaskQueryParameters
    {
        public bool? IsCompleted { get; set; }
        public int? CategoryId { get; set; }
        public string? SearchString { get; set; }
        public string? SortBy { get; set; }
    }
}
