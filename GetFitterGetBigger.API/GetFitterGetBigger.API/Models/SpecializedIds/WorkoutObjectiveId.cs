using System;
using GetFitterGetBigger.API.Models.Interfaces;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct WorkoutObjectiveId : ISpecializedId<WorkoutObjectiveId>
{
    private readonly Guid _value;
    private const string Prefix = "workoutobjective";
    
    private WorkoutObjectiveId(Guid value)
    {
        this._value = value;
    }
    
    public static WorkoutObjectiveId New() => new(Guid.NewGuid());
    
    public static WorkoutObjectiveId From(Guid guid) => new(guid);
    
    
    public static WorkoutObjectiveId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    // ISpecializedId<WorkoutObjectiveId> implementation
    public Guid ToGuid() => _value;
    
    private static bool TryParse(string? input, out WorkoutObjectiveId result)
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
    
    public static WorkoutObjectiveId ParseOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        return TryParse(input, out var result) ? result : Empty;
    }
    
    public override string ToString() => IsEmpty ? string.Empty : $"{Prefix}-{this._value}";
    
    public string GetPrefix() => Prefix;
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(WorkoutObjectiveId id) => id._value;
}