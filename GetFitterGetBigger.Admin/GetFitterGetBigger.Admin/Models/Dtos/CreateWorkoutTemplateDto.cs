using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class CreateWorkoutTemplateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required]
        public string CategoryId { get; set; } = string.Empty;

        [Required]
        public string DifficultyId { get; set; } = string.Empty;

        [Required]
        [Range(5, 300, ErrorMessage = "Duration must be between 5 and 300 minutes")]
        public int EstimatedDurationMinutes { get; set; }

        public bool IsPublic { get; set; }

        public List<string> Tags { get; set; } = new();
    }
}