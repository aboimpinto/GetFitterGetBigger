using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Extensions;

/// <summary>
/// Extension methods for mapping ExerciseLink entities to DTOs
/// </summary>
public static class ExerciseLinkMappingExtensions
{
    /// <summary>
    /// Maps an ExerciseLink entity to its DTO representation
    /// </summary>
    public static ExerciseLinkDto ToDto(this ExerciseLink exerciseLink)
    {
        if (exerciseLink == null || exerciseLink.IsEmpty)
        {
            return ExerciseLinkDto.Empty;
        }
        
        return new ExerciseLinkDto
        {
            Id = exerciseLink.Id.ToString(),
            SourceExerciseId = exerciseLink.SourceExerciseId.ToString(),
            TargetExerciseId = exerciseLink.TargetExerciseId.ToString(),
            LinkType = exerciseLink.LinkType,
            DisplayOrder = exerciseLink.DisplayOrder,
            IsActive = exerciseLink.IsActive
        };
    }
}