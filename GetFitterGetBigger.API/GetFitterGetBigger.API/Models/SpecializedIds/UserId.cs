using System;
using GetFitterGetBigger.API.Models.Interfaces;
using System.Text.Json.Serialization;
using GetFitterGetBigger.API.Models.SpecializedIds.JsonConverters;

namespace GetFitterGetBigger.API.Models.SpecializedIds
{
    [JsonConverter(typeof(UserIdJsonConverter))]
    public readonly record struct UserId : ISpecializedId<UserId>
    {
        private readonly Guid _value;
        private const string Prefix = "user";

        private UserId(Guid value)
        {
            _value = value;
        }

        public static UserId New() => new(Guid.NewGuid());

        public static UserId From(Guid guid) => new(guid);
        
        public static UserId Empty => new(Guid.Empty);
        
        public bool IsEmpty => _value == Guid.Empty;
        
        // ISpecializedId<UserId> implementation
        public Guid ToGuid() => _value;
        
        public static UserId ParseOrEmpty(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return Empty;
                
            return TryParse(input, out var result) ? result : Empty;
        }

        private static bool TryParse(string? input, out UserId result)
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

        public static implicit operator Guid(UserId id) => id._value;
    }
}
