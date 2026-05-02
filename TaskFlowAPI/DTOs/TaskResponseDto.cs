namespace TaskFlowAPI.DTOs
{
    // Contains only the fields we want the outside world to see
    // internal or sensitive fields would be excluded here
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } 
        public DateTime CreatedAt { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
