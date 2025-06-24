using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct DifficultyLevelId
{
    private readonly Guid _value;
    
    private DifficultyLevelId(Guid value)
    {
        this._value = value;
    }
    
    public static DifficultyLevelId New() => new(Guid.NewGuid());
    
    public static DifficultyLevelId From(Guid guid) => new(guid);
    
    public static bool TryParse(string? input, out DifficultyLevelId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("difficultylevel-"))
            return false;
            
        string guidPart = input["difficultylevel-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public override string ToString() => $"difficultylevel-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(DifficultyLevelId id) => id._value;
}
