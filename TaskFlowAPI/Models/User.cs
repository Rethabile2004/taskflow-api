using System.ComponentModel.DataAnnotations;

namespace TaskFlowAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        [MaxLength(100)]
        [Required]
        public string Email { get; set; }=string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        [MaxLength(100)]
        [Required]
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        // Navigation Property
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
