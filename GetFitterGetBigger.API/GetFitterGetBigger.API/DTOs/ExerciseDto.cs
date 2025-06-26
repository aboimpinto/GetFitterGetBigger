using System;
using System.Collections.Generic;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for Exercise entity responses
/// </summary>
public class ExerciseDto
{
    /// <summary>
    /// The ID of the exercise in the format "exercise-{guid}"
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// The name of the exercise
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// A concise summary of the exercise
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed, step-by-step instructions for performing the movement
    /// </summary>
    public string Instructions { get; set; } = string.Empty;
    
    /// <summary>
    /// A link to a hosted video demonstrating the exercise
    /// </summary>
    public string? VideoUrl { get; set; }
    
    /// <summary>
    /// A link to a hosted image of the exercise
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Indicates if the exercise is performed on one side of the body at a time
    /// </summary>
    public bool IsUnilateral { get; set; }
    
    /// <summary>
    /// Indicates if the exercise is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// The difficulty level of the exercise
    /// </summary>
    public ReferenceDataDto Difficulty { get; set; } = null!;
    
    /// <summary>
    /// The muscle groups targeted by the exercise
    /// </summary>
    public List<MuscleGroupWithRoleDto> MuscleGroups { get; set; } = new();
    
    /// <summary>
    /// The equipment required for the exercise
    /// </summary>
    public List<ReferenceDataDto> Equipment { get; set; } = new();
    
    /// <summary>
    /// The movement patterns associated with the exercise
    /// </summary>
    public List<ReferenceDataDto> MovementPatterns { get; set; } = new();
    
    /// <summary>
    /// The body parts involved in the exercise
    /// </summary>
    public List<ReferenceDataDto> BodyParts { get; set; } = new();
}

/// <summary>
/// DTO for muscle group with its role in the exercise
/// </summary>
public class MuscleGroupWithRoleDto
{
    /// <summary>
    /// The muscle group details
    /// </summary>
    public ReferenceDataDto MuscleGroup { get; set; } = null!;
    
    /// <summary>
    /// The role of the muscle group in the exercise (Primary, Secondary, Stabilizer)
    /// </summary>
    public ReferenceDataDto Role { get; set; } = null!;
}