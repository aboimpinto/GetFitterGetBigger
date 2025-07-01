using System.Text.Json.Serialization;

namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class MuscleGroupDto
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("bodyPartId")]
        public required string BodyPartId { get; set; }

        [JsonPropertyName("bodyPartName")]
        public string? BodyPartName { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateMuscleGroupDto
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("bodyPartId")]
        public required string BodyPartId { get; set; }
    }

    public class UpdateMuscleGroupDto
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("bodyPartId")]
        public required string BodyPartId { get; set; }
    }
}