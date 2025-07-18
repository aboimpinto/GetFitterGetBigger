using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GetFitterGetBigger.API.Models.SpecializedIds.JsonConverters
{
    public class UserIdJsonConverter : JsonConverter<UserId>
    {
        public override UserId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Expected string value for UserId.");
            }

            var value = reader.GetString();
            var userId = UserId.ParseOrEmpty(value);
            if (!userId.IsEmpty)
            {
                return userId;
            }

            throw new JsonException($"Invalid UserId format: {value}");
        }

        public override void Write(Utf8JsonWriter writer, UserId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
