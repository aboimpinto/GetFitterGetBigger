using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;

/// <summary>
/// Data service interface for ExerciseLink read operations.
/// Encapsulates all database queries and entity-to-DTO mapping.
/// </summary>
public interface IExerciseLinkQueryDataService
{
    /// <summary>
    /// Gets all links for a specific exercise
    /// </summary>
    Task<ServiceResult<ExerciseLinksResponseDto>> GetLinksAsync(GetExerciseLinksCommand command);
    
    /// <summary>
    /// Gets a specific exercise link by ID
    /// </summary>
    Task<ServiceResult<ExerciseLinkDto>> GetByIdAsync(ExerciseLinkId id);
    
    /// <summary>
    /// Checks if a link already exists between two exercises with a specific type
    /// </summary>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseId sourceId, ExerciseId targetId, string linkType);
    
    /// <summary>
    /// Gets links by source exercise for circular reference checking
    /// </summary>
    Task<ServiceResult<List<ExerciseLinkDto>>> GetBySourceExerciseAsync(ExerciseId sourceId, string? linkType = null);
    
    /// <summary>
    /// Gets the count of links for a specific exercise and type
    /// </summary>
    Task<ServiceResult<int>> GetLinkCountAsync(ExerciseId sourceId, string linkType);
    
    /// <summary>
    /// Gets suggested links based on common usage patterns
    /// </summary>
    Task<ServiceResult<List<ExerciseLinkDto>>> GetSuggestedLinksAsync(ExerciseId exerciseId, int count);
    
    // ===== ENHANCED BIDIRECTIONAL QUERY METHODS =====
    
    /// <summary>
    /// Gets all links where the specified exercise is the TARGET (reverse links)
    /// This enables efficient bidirectional link queries
    /// </summary>
    Task<ServiceResult<List<ExerciseLinkDto>>> GetByTargetExerciseAsync(ExerciseId targetExerciseId);
    
    /// <summary>
    /// Gets bidirectional links for an exercise (both source and target links of specified type)
    /// Useful for finding all ALTERNATIVE links or checking bidirectional relationships
    /// </summary>
    Task<ServiceResult<List<ExerciseLinkDto>>> GetBidirectionalLinksAsync(
        ExerciseId exerciseId, 
        ExerciseLinkType linkType);
    
    /// <summary>
    /// Checks if bidirectional links exist between two exercises for the specified type
    /// Used to prevent duplicate bidirectional link creation
    /// </summary>
    Task<ServiceResult<BooleanResultDto>> ExistsBidirectionalAsync(
        ExerciseId sourceId, 
        ExerciseId targetId, 
        ExerciseLinkType linkType);
    
    /// <summary>
    /// Gets links by source exercise using enum-based filtering
    /// </summary>
    Task<ServiceResult<List<ExerciseLinkDto>>> GetBySourceExerciseWithEnumAsync(ExerciseId sourceId, ExerciseLinkType? linkType = null);
    
    /// <summary>
    /// Checks if a link exists using enum-based type matching (overload for enum support)
    /// </summary>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseId sourceId, ExerciseId targetId, ExerciseLinkType linkType);
    
    /// <summary>
    /// Gets links by source exercise and type for display order calculation
    /// Used by bidirectional creation algorithm to calculate next available display order
    /// </summary>
    Task<ServiceResult<List<ExerciseLinkDto>>> GetBySourceAndTypeAsync(
        ExerciseId sourceId, 
        ExerciseLinkType linkType);
    
    // ===== EXERCISE VALIDATION METHODS (To comply with Single Repository Rule) =====
    
    /// <summary>
    /// Validates and retrieves an exercise by ID for link validation purposes
    /// Returns the exercise DTO if valid and active, Empty otherwise
    /// </summary>
    Task<ServiceResult<ExerciseDto>> GetAndValidateExerciseAsync(ExerciseId exerciseId);
}