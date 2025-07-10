using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GetFitterGetBigger.API.Attributes;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Request DTO for updating an existing exercise
/// </summary>
public class UpdateExerciseRequest
{
    /// <summary>
    /// The name of the exercise
    /// </summary>
    [Required(ErrorMessage = "Exercise name is required")]
    [StringLength(200, ErrorMessage = "Exercise name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// A concise summary of the exercise
    /// </summary>
    [Required(ErrorMessage = "Description is required")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Ordered list of coach notes providing step-by-step instructions
    /// </summary>
    public List<CoachNoteRequest> CoachNotes { get; set; } = new();
    
    /// <summary>
    /// The IDs of exercise types (Warmup, Workout, Cooldown, Rest)
    /// </summary>
    public List<string> ExerciseTypeIds { get; set; } = new();
    
    /// <summary>
    /// A link to a hosted video demonstrating the exercise
    /// </summary>
    [Url(ErrorMessage = "Video URL must be a valid URL")]
    public string? VideoUrl { get; set; }
    
    /// <summary>
    /// A link to a hosted image of the exercise
    /// </summary>
    [Url(ErrorMessage = "Image URL must be a valid URL")]
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Indicates if the exercise is performed on one side of the body at a time
    /// </summary>
    public bool? IsUnilateral { get; set; }
    
    /// <summary>
    /// Indicates if the exercise is active
    /// </summary>
    public bool? IsActive { get; set; }
    
    /// <summary>
    /// The ID of the difficulty level
    /// </summary>
    [Required(ErrorMessage = "Difficulty level is required")]
    public string DifficultyId { get; set; } = string.Empty;
    
    /// <summary>
    /// The ID of the kinetic chain type
    /// Required for non-rest exercises, must be null for rest exercises
    /// Validation is handled at the service layer to access database for exercise type detection
    /// </summary>
    public string? KineticChainId { get; set; }
    
    /// <summary>
    /// The ID of the exercise weight type
    /// Optional - defines how weight is handled for this exercise
    /// </summary>
    public string? ExerciseWeightTypeId { get; set; }
    
    /// <summary>
    /// The muscle groups targeted by the exercise with their roles
    /// Optional for REST exercises, required for all other exercise types
    /// Validation is handled at the service layer to access database for exercise type detection
    /// </summary>
    public List<MuscleGroupWithRoleRequest> MuscleGroups { get; set; } = new();
    
    /// <summary>
    /// The IDs of equipment required for the exercise
    /// </summary>
    public List<string> EquipmentIds { get; set; } = new();
    
    /// <summary>
    /// The IDs of movement patterns associated with the exercise
    /// </summary>
    public List<string> MovementPatternIds { get; set; } = new();
    
    /// <summary>
    /// The IDs of body parts involved in the exercise
    /// </summary>
    public List<string> BodyPartIds { get; set; } = new();
}