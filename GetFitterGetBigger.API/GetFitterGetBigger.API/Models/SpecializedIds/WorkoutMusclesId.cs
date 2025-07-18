using System;
using GetFitterGetBigger.API.Models.Interfaces;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct WorkoutMusclesId : ISpecializedId<WorkoutMusclesId>
{
    private readonly Guid _value;
    private const string Prefix = "workoutmuscles";
    
    private WorkoutMusclesId(Guid value)
    {
        this._value = value;
    }
    
    public static WorkoutMusclesId New() => new(Guid.NewGuid());
    
    public static WorkoutMusclesId From(Guid guid) => new(guid);
    
    public static WorkoutMusclesId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    // ISpecializedId<WorkoutMusclesId> implementation
    public Guid ToGuid() => _value;
    
    private static bool TryParse(string? input, out WorkoutMusclesId result)
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
    
    public static WorkoutMusclesId ParseOrEmpty(string? input)
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
    public static implicit operator Guid(WorkoutMusclesId id) => id._value;
}