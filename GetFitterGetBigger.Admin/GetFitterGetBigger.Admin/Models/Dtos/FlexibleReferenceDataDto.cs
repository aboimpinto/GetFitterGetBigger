using System.Text.Json.Serialization;

namespace GetFitterGetBigger.Admin.Models.Dtos
{
    /// <summary>
    /// A flexible DTO that can handle both 'value' and 'name' property names from the API
    /// </summary>
    public class FlexibleReferenceDataDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets the display value - prefers 'value' over 'name'
        /// </summary>
        public string GetDisplayValue() => Value ?? Name ?? string.Empty;

        /// <summary>
        /// Converts to standard ReferenceDataDto
        /// </summary>
        public ReferenceDataDto ToReferenceDataDto()
        {
            return new ReferenceDataDto
            {
                Id = Id ?? string.Empty,
                Value = GetDisplayValue(),
                Description = Description
            };
        }
    }
}