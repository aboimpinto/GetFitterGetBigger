using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct MuscleRoleId
{
    private readonly Guid _value;
    
    private MuscleRoleId(Guid value)
    {
        this._value = value;
    }
    
    public static MuscleRoleId New() => new(Guid.NewGuid());
    
    public static MuscleRoleId From(Guid guid) => new(guid);
    
    public static MuscleRoleId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    public static bool TryParse(string? input, out MuscleRoleId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("musclerole-"))
            return false;
            
        string guidPart = input["musclerole-".Length..];
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
    
    public override string ToString() => IsEmpty ? string.Empty : $"musclerole-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(MuscleRoleId id) => id._value;
}
