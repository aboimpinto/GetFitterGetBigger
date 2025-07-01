using System.Text.Json.Serialization;

namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class EquipmentDto
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateEquipmentDto
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }
    }

    public class UpdateEquipmentDto
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }
    }
}