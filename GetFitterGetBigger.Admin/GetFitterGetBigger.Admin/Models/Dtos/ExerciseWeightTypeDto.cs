using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class ExerciseWeightTypeDto
    {
        [JsonPropertyName("id")]
        [Required]
        public required Guid Id { get; set; }

        [JsonPropertyName("code")]
        [Required]
        public required string Code { get; set; }

        [JsonPropertyName("name")]
        [Required]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; }
    }
}