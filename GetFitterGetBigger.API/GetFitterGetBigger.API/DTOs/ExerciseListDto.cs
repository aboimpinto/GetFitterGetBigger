using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Wrapper for a list of exercises that implements IEmptyDto interface
/// </summary>
public record ExerciseListDto : IEmptyDto<ExerciseListDto>
{
    /// <summary>
    /// The list of exercises
    /// </summary>
    public List<ExerciseDto> Exercises { get; init; } = new();
    
    /// <summary>
    /// Indicates if this is an empty/null object instance
    /// </summary>
    public bool IsEmpty => Exercises.Count == 0;
    
    /// <summary>
    /// Static factory for creating an empty ExerciseListDto instance
    /// </summary>
    public static ExerciseListDto Empty => new() { Exercises = new() };
    
    /// <summary>
    /// Creates a new instance from a list of exercises
    /// </summary>
    public static ExerciseListDto Create(List<ExerciseDto> exercises)
    {
        return new ExerciseListDto { Exercises = exercises ?? new() };
    }
    
    /// <summary>
    /// Implicit conversion from List&lt;ExerciseDto&gt; to ExerciseListDto
    /// </summary>
    public static implicit operator ExerciseListDto(List<ExerciseDto> exercises)
    {
        return Create(exercises);
    }
    
    /// <summary>
    /// Implicit conversion from ExerciseListDto to List&lt;ExerciseDto&gt;
    /// </summary>
    public static implicit operator List<ExerciseDto>(ExerciseListDto dto)
    {
        return dto.Exercises;
    }
}