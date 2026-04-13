namespace TaskFlowAPI.DTOs
{
    //contains only the fields the client is allowed to set on creation
    // Id and CreatedAt are internally excluded. the API sets those
    public class TaskCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}
