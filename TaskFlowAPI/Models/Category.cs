using System.ComponentModel.DataAnnotations;

namespace TaskFlowAPI.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public ICollection<TaskItem> Tasks{get;set;}=new List<TaskItem>();
    }
}
