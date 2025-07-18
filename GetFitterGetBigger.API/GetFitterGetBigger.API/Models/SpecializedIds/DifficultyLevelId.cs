using System;
using GetFitterGetBigger.API.Models.Interfaces;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct DifficultyLevelId : ISpecializedId<DifficultyLevelId>
{
    private readonly Guid _value;
    private const string Prefix = "difficultylevel";
    
    private DifficultyLevelId(Guid value)
    {
        this._value = value;
    }
    
    public static DifficultyLevelId New() => new(Guid.NewGuid());
    
    public static DifficultyLevelId From(Guid guid) => new(guid);
    
    public static DifficultyLevelId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    // ISpecializedId<DifficultyLevelId> implementation
    public Guid ToGuid() => _value;
    
    private static bool TryParse(string? input, out DifficultyLevelId result)
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
    
    public static DifficultyLevelId ParseOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        return TryParse(input, out var result) ? result : Empty;
    }
    
    public override string ToString() => IsEmpty ? string.Empty : $"{Prefix}-{this._value}";
    
    public string GetPrefix() => Prefix;
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(DifficultyLevelId id) => id._value;
}
