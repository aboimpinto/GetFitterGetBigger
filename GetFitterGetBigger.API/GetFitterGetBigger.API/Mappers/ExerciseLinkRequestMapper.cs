using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;

namespace GetFitterGetBigger.API.Mappers;

/// <summary>
/// Maps between web request DTOs (with string IDs) and service commands (with specialized IDs).
/// This enforces proper separation between web layer and service layer concerns.
/// Handles all ID parsing at the web boundary as per architecture standards.
/// </summary>
public static class ExerciseLinkRequestMapper
{
    /// <summary>
    /// Maps CreateExerciseLinkDto (web DTO) to CreateExerciseLinkCommand (service command)
    /// Parses string IDs to specialized ID types at the web boundary
    /// </summary>
    public static CreateExerciseLinkCommand ToCommand(this CreateExerciseLinkDto request, string sourceExerciseId)
    {
        return new CreateExerciseLinkCommand
        {
            SourceExerciseId = ExerciseId.ParseOrEmpty(sourceExerciseId ?? string.Empty),
            TargetExerciseId = ExerciseId.ParseOrEmpty(request?.TargetExerciseId ?? string.Empty),
            LinkType = request?.LinkType ?? string.Empty,
            DisplayOrder = request?.DisplayOrder ?? 1
        };
    }
    
    /// <summary>
    /// Maps GetExerciseLinksCommand from controller parameters
    /// Parses string IDs to specialized ID types at the web boundary
    /// </summary>
    public static GetExerciseLinksCommand ToGetLinksCommand(string exerciseId, string? linkType, bool includeExerciseDetails)
    {
        return new GetExerciseLinksCommand
        {
            ExerciseId = ExerciseId.ParseOrEmpty(exerciseId ?? string.Empty),
            LinkType = linkType,
            IncludeExerciseDetails = includeExerciseDetails
        };
    }
    
    /// <summary>
    /// Maps UpdateExerciseLinkCommand from controller parameters
    /// Parses string IDs to specialized ID types at the web boundary
    /// </summary>
    public static UpdateExerciseLinkCommand ToUpdateCommand(string exerciseId, string linkId, UpdateExerciseLinkDto dto)
    {
        return new UpdateExerciseLinkCommand
        {
            ExerciseId = ExerciseId.ParseOrEmpty(exerciseId ?? string.Empty),
            LinkId = ExerciseLinkId.ParseOrEmpty(linkId ?? string.Empty),
            DisplayOrder = dto?.DisplayOrder ?? 0,
            IsActive = dto?.IsActive ?? true
        };
    }
    
    /// <summary>
    /// Maps GetSuggestedLinksCommand from controller parameters
    /// Parses string ID to specialized ID type at the web boundary
    /// </summary>
    public static GetSuggestedLinksCommand ToGetSuggestedLinksCommand(string exerciseId, int count)
    {
        return new GetSuggestedLinksCommand
        {
            ExerciseId = ExerciseId.ParseOrEmpty(exerciseId ?? string.Empty),
            Count = count
        };
    }
    
    /// <summary>
    /// Maps DeleteExerciseLinkCommand from controller parameters
    /// Parses string IDs to specialized ID types at the web boundary
    /// </summary>
    public static DeleteExerciseLinkCommand ToDeleteCommand(string exerciseId, string linkId, bool deleteReverse)
    {
        return new DeleteExerciseLinkCommand
        {
            ExerciseId = ExerciseId.ParseOrEmpty(exerciseId ?? string.Empty),
            LinkId = ExerciseLinkId.ParseOrEmpty(linkId ?? string.Empty),
            DeleteReverse = deleteReverse
        };
    }
}