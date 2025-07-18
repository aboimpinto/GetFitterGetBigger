using System;
using GetFitterGetBigger.API.Models.Interfaces;
using System.Text.Json.Serialization;
using GetFitterGetBigger.API.Models.SpecializedIds.JsonConverters;

namespace GetFitterGetBigger.API.Models.SpecializedIds
{
    [JsonConverter(typeof(ClaimIdJsonConverter))]
    public readonly record struct ClaimId : ISpecializedId<ClaimId>
    {
        private readonly Guid _value;
        private const string Prefix = "claim";

        private ClaimId(Guid value)
        {
            _value = value;
        }

        public static ClaimId New() => new(Guid.NewGuid());

        public static ClaimId From(Guid guid) => new(guid);
        public static ClaimId Empty => new(Guid.Empty);
        
        public bool IsEmpty => _value == Guid.Empty;
        
        // ISpecializedId<ClaimId> implementation
        public Guid ToGuid() => _value;
        
        public static ClaimId ParseOrEmpty(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return Empty;
                
            return TryParse(input, out var result) ? result : Empty;
        }
        

        private static bool TryParse(string? input, out ClaimId result)
        {
            result = default;
            var prefix = $"{Prefix}-";
            if (string.IsNullOrEmpty(input) || !input.StartsWith(prefix))
                return false;

            string guidPart = input[prefix.Length..];
            if (Guid.TryParse(guidPart, out Guid guid))
            {
                result = From(guid);
                return true;
            }

            return false;
        }

        public override string ToString() => IsEmpty ? string.Empty : $"{Prefix}-{_value}";
        
        public string GetPrefix() => Prefix;

        public static implicit operator Guid(ClaimId id) => id._value;
    }
}
