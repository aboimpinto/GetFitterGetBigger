using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GetFitterGetBigger.API.Models.SpecializedIds.JsonConverters
{
    public class ClaimIdJsonConverter : JsonConverter<ClaimId>
    {
        public override ClaimId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Expected string value for ClaimId.");
            }

            var value = reader.GetString();
            var claimId = ClaimId.ParseOrEmpty(value);
            if (!claimId.IsEmpty)
            {
                return claimId;
            }

            throw new JsonException($"Invalid ClaimId format: {value}");
        }

        public override void Write(Utf8JsonWriter writer, ClaimId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
