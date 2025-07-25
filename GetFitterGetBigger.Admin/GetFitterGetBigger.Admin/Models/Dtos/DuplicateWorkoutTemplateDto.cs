using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class DuplicateWorkoutTemplateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        public string NewName { get; set; } = string.Empty;
    }
}