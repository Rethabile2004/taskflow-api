using System.ComponentModel.DataAnnotations;

namespace TaskFlowAPI.DTOs
{
    // All properties and nullable, "null" means don't change this field
    // Only the fields the client explicitly sends will be updated
    public class TaskPatchDto
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
        public string? Title { get; set; }
        [StringLength(500,ErrorMessage ="Description must not exceed 500 characters.")]
        public string? Description { get; set; }
        public bool? IsCompleted { get; set; }
    }
}
