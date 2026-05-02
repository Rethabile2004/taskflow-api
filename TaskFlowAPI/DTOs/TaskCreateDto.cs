using System.ComponentModel.DataAnnotations;

namespace TaskFlowAPI.DTOs
{
    //contains only the fields the client is allowed to set on creation
    // Id and CreatedAt are internally excluded. the API sets those
    public class TaskCreateDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
        public string Title { get; set; } = string.Empty;
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public int? CategoryId { get; set; }
    }
}
