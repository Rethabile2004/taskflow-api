using System.ComponentModel.DataAnnotations;

namespace TaskFlowAPI.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
