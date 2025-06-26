using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExerciseBodyPart
{
    public ExerciseId ExerciseId { get; init; }
    public BodyPartId BodyPartId { get; init; }
    
    // Navigation properties
    public Exercise Exercise { get; init; } = null!;
    public BodyPart BodyPart { get; init; } = null!;
    
    private ExerciseBodyPart() { }
    
    public static class Handler
    {
        public static ExerciseBodyPart Create(
            ExerciseId exerciseId, 
            BodyPartId bodyPartId) =>
            new()
            {
                ExerciseId = exerciseId,
                BodyPartId = bodyPartId
            };
    }
}