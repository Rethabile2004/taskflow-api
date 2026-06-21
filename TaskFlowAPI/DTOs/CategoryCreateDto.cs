using System.ComponentModel.DataAnnotations;

namespace TaskFlowAPI.DTOs
{
    public class CategoryCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
