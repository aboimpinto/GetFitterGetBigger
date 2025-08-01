using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for Exercise entity responses
/// </summary>
public record ExerciseDto : IEmptyDto<ExerciseDto>
{
    /// <summary>
    /// The ID of the exercise in the format "exercise-{guid}"
    /// </summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// The name of the exercise
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// A concise summary of the exercise
    /// </summary>
    public string Description { get; init; } = string.Empty;
    
    /// <summary>
    /// Ordered list of coach notes providing step-by-step instructions
    /// </summary>
    public List<CoachNoteDto> CoachNotes { get; init; } = new();
    
    /// <summary>
    /// The types of this exercise (Warmup, Workout, Cooldown, Rest)
    /// </summary>
    public List<ReferenceDataDto> ExerciseTypes { get; init; } = new();
    
    /// <summary>
    /// A link to a hosted video demonstrating the exercise
    /// </summary>
    public string? VideoUrl { get; init; }
    
    /// <summary>
    /// A link to a hosted image of the exercise
    /// </summary>
    public string? ImageUrl { get; init; }
    
    /// <summary>
    /// Indicates if the exercise is performed on one side of the body at a time
    /// </summary>
    public bool IsUnilateral { get; init; }
    
    /// <summary>
    /// Indicates if the exercise is active
    /// </summary>
    public bool IsActive { get; init; }
    
    /// <summary>
    /// The difficulty level of the exercise
    /// </summary>
    public ReferenceDataDto Difficulty { get; init; } = null!;
    
    /// <summary>
    /// The kinetic chain type of the exercise (Open Chain, Closed Chain)
    /// </summary>
    public ReferenceDataDto? KineticChain { get; init; }
    
    /// <summary>
    /// The weight type of the exercise (Bodyweight Only, Weight Required, etc.)
    /// </summary>
    public ReferenceDataDto? ExerciseWeightType { get; init; }
    
    /// <summary>
    /// The muscle groups targeted by the exercise
    /// </summary>
    public List<MuscleGroupWithRoleDto> MuscleGroups { get; init; } = new();
    
    /// <summary>
    /// The equipment required for the exercise
    /// </summary>
    public List<ReferenceDataDto> Equipment { get; init; } = new();
    
    /// <summary>
    /// The movement patterns associated with the exercise
    /// </summary>
    public List<ReferenceDataDto> MovementPatterns { get; init; } = new();
    
    /// <summary>
    /// The body parts involved in the exercise
    /// </summary>
    public List<ReferenceDataDto> BodyParts { get; init; } = new();
    
    /// <summary>
    /// Indicates if this is an empty/null object instance
    /// </summary>
    public bool IsEmpty => Id == string.Empty;
    
    /// <summary>
    /// Static factory for creating an empty ExerciseDto instance
    /// </summary>
    public static ExerciseDto Empty => new() 
    { 
        Id = string.Empty,
        Difficulty = ReferenceDataDto.Empty // Use the Empty pattern for ReferenceDataDto
    };
}

/// <summary>
/// DTO for muscle group with its role in the exercise
/// </summary>
public record MuscleGroupWithRoleDto
{
    /// <summary>
    /// The muscle group details
    /// </summary>
    public ReferenceDataDto MuscleGroup { get; init; } = null!;
    
    /// <summary>
    /// The role of the muscle group in the exercise (Primary, Secondary, Stabilizer)
    /// </summary>
    public ReferenceDataDto Role { get; init; } = null!;
}