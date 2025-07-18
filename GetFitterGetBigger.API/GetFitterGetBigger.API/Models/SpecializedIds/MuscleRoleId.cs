using System;
using GetFitterGetBigger.API.Models.Interfaces;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct MuscleRoleId : ISpecializedId<MuscleRoleId>
{
    private readonly Guid _value;
    private const string Prefix = "musclerole";
    
    private MuscleRoleId(Guid value)
    {
        this._value = value;
    }
    
    public static MuscleRoleId New() => new(Guid.NewGuid());
    
    public static MuscleRoleId From(Guid guid) => new(guid);
    
    public static MuscleRoleId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    // ISpecializedId<MuscleRoleId> implementation
    public Guid ToGuid() => _value;
    
    private static bool TryParse(string? input, out MuscleRoleId result)
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
    
    public static MuscleRoleId ParseOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        if (TryParse(input, out var result))
            return result;
            
        return Empty;
    }
    
    public override string ToString() => IsEmpty ? string.Empty : $"{Prefix}-{this._value}";
    
    public string GetPrefix() => Prefix;
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(MuscleRoleId id) => id._value;
}
