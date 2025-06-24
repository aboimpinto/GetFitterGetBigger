using System;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExerciseMovementPattern
{
    public ExerciseId ExerciseId { get; init; }
    public MovementPatternId PatternId { get; init; }
    
    // Navigation properties
    public Exercise Exercise { get; init; } = null!;
    public MovementPattern Pattern { get; init; } = null!;
    
    private ExerciseMovementPattern() { }
    
    public static class Handler
    {
        public static ExerciseMovementPattern Create(ExerciseId exerciseId, MovementPatternId patternId) =>
            new()
            {
                ExerciseId = exerciseId,
                PatternId = patternId
            };
    }
}
