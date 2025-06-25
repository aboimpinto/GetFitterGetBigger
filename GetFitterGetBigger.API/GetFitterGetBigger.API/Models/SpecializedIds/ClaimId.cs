using System;
using System.Text.Json.Serialization;
using GetFitterGetBigger.API.Models.SpecializedIds.JsonConverters;

namespace GetFitterGetBigger.API.Models.SpecializedIds
{
    [JsonConverter(typeof(ClaimIdJsonConverter))]
    public readonly record struct ClaimId
    {
        private readonly Guid _value;

        private ClaimId(Guid value)
        {
            _value = value;
        }

        public static ClaimId New() => new(Guid.NewGuid());

        public static ClaimId From(Guid guid) => new(guid);

        public static bool TryParse(string? input, out ClaimId result)
        {
            result = default;
            if (string.IsNullOrEmpty(input) || !input.StartsWith("claim-"))
                return false;

            string guidPart = input["claim-".Length..];
            if (Guid.TryParse(guidPart, out Guid guid))
            {
                result = From(guid);
                return true;
            }

            return false;
        }

        public override string ToString() => $"claim-{_value}";

        public static implicit operator Guid(ClaimId id) => id._value;
    }
}
