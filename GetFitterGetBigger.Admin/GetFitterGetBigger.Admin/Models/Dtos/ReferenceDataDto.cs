using System.Text.Json.Serialization;

namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class ReferenceDataDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
