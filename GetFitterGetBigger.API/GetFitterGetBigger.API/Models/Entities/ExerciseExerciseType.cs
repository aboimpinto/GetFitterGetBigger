using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExerciseExerciseType
{
    public ExerciseId ExerciseId { get; init; }
    public ExerciseTypeId ExerciseTypeId { get; init; }
    
    // Navigation properties
    public Exercise? Exercise { get; init; }
    public ExerciseType? ExerciseType { get; init; }
    
    // Private constructor to force usage of Handler
    private ExerciseExerciseType() { }
    
    public static class Handler
    {
        public static ExerciseExerciseType Create(ExerciseId exerciseId, ExerciseTypeId exerciseTypeId)
        {
            return new ExerciseExerciseType
            {
                ExerciseId = exerciseId,
                ExerciseTypeId = exerciseTypeId
            };
        }
    }
}