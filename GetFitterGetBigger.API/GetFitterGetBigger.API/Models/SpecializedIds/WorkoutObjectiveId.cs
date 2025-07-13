using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct WorkoutObjectiveId
{
    private readonly Guid _value;
    
    private WorkoutObjectiveId(Guid value)
    {
        this._value = value;
    }
    
    public static WorkoutObjectiveId New() => new(Guid.NewGuid());
    
    public static WorkoutObjectiveId From(Guid guid) => new(guid);
    
    public static WorkoutObjectiveId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    public static bool TryParse(string? input, out WorkoutObjectiveId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("workoutobjective-"))
            return false;
            
        string guidPart = input["workoutobjective-".Length..];
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
            
        if (TryParse(input, out var result))
            return result;
            
        return Empty;
    }
    
    public override string ToString() => IsEmpty ? string.Empty : $"workoutobjective-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(WorkoutObjectiveId id) => id._value;
}