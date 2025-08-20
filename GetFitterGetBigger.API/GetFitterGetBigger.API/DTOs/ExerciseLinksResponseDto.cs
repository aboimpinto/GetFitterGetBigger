using System.Collections.Generic;
using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Response DTO for getting exercise links
/// </summary>
public class ExerciseLinksResponseDto : IEmptyDto<ExerciseLinksResponseDto>
{
    /// <summary>
    /// The ID of the exercise in the format "exercise-{guid}"
    /// </summary>
    public string ExerciseId { get; set; } = string.Empty;
    
    /// <summary>
    /// The list of linked exercises
    /// </summary>
    public List<ExerciseLinkDto> Links { get; set; } = new();
    
    /// <summary>
    /// Gets the static empty instance of ExerciseLinksResponseDto
    /// </summary>
    public static ExerciseLinksResponseDto Empty => new()
    {
        ExerciseId = string.Empty,
        Links = new List<ExerciseLinkDto>()
    };
    
    /// <summary>
    /// Gets a value indicating whether this instance represents an empty DTO
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(ExerciseId) || ExerciseId == string.Empty;
}