using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class ChangeWorkoutStateDto
    {
        [Required]
        [JsonPropertyName("workoutStateId")]
        public string WorkoutStateId { get; set; } = string.Empty;
    }
}