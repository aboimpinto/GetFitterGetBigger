using System;
using System.Text.Json.Serialization;
using GetFitterGetBigger.API.Models.SpecializedIds.JsonConverters;

namespace GetFitterGetBigger.API.Models.SpecializedIds
{
    [JsonConverter(typeof(UserIdJsonConverter))]
    public readonly record struct UserId
    {
        private readonly Guid _value;

        private UserId(Guid value)
        {
            _value = value;
        }

        public static UserId New() => new(Guid.NewGuid());

        public static UserId From(Guid guid) => new(guid);

        public static bool TryParse(string? input, out UserId result)
        {
            result = default;
            if (string.IsNullOrEmpty(input) || !input.StartsWith("user-"))
                return false;

            string guidPart = input["user-".Length..];
            if (Guid.TryParse(guidPart, out Guid guid))
            {
                result = From(guid);
                return true;
            }

            return false;
        }

        public override string ToString() => $"user-{_value}";

        public static implicit operator Guid(UserId id) => id._value;
    }
}
