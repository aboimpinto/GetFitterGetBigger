using System.Text.Json;
using System.Text.Json.Serialization;
using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.JsonConverters
{
    public class ExerciseWeightTypeJsonConverter : JsonConverter<ExerciseWeightTypeDto>
    {
        public override ExerciseWeightTypeDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            // The API returns the weight type as a reference data object with id, value, and description
            if (!root.TryGetProperty("id", out var idElement) || 
                !root.TryGetProperty("value", out var valueElement))
            {
                return null;
            }

            var id = idElement.GetString();
            var value = valueElement.GetString();
            var description = root.TryGetProperty("description", out var descElement) 
                ? descElement.GetString() 
                : null;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(value))
            {
                return null;
            }

            // Extract the GUID from the specialized ID format
            var guidStr = id.StartsWith("exerciseweighttype-") 
                ? id.Substring("exerciseweighttype-".Length)
                : id;

            if (!Guid.TryParse(guidStr, out var guid))
            {
                return null;
            }

            // Derive the code from the value
            var code = value.ToUpper().Replace(" ", "_");

            return new ExerciseWeightTypeDto
            {
                Id = guid,
                Code = code,
                Name = value,
                Description = description,
                IsActive = true,
                DisplayOrder = 0
            };
        }

        public override void Write(Utf8JsonWriter writer, ExerciseWeightTypeDto value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            writer.WriteString("id", $"exerciseweighttype-{value.Id}");
            writer.WriteString("value", value.Name);
            writer.WriteString("description", value.Description);
            writer.WriteEndObject();
        }
    }
}