using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class ChangeWorkoutStateDto
    {
        [Required]
        public string NewStateId { get; set; } = string.Empty;
    }
}