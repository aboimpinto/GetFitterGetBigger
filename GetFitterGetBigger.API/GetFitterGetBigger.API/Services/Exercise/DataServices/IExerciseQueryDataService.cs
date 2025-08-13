using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Exercise.DataServices;

/// <summary>
/// Data service interface for Exercise read operations.
/// Handles all database queries and entity-to-DTO mapping.
/// </summary>
public interface IExerciseQueryDataService
{
    /// <summary>
    /// Gets a paged list of exercises with filtering
    /// </summary>
    /// <param name="filterParams">Filter parameters for pagination and filtering</param>
    /// <returns>Service result containing paginated exercise DTOs</returns>
    Task<ServiceResult<PagedResponse<ExerciseDto>>> GetPagedAsync(GetExercisesCommand filterParams);
    
    /// <summary>
    /// Loads an exercise by ID with all related data
    /// </summary>
    /// <param name="id">The exercise ID</param>
    /// <returns>Service result containing the exercise DTO or Empty if not found</returns>
    Task<ServiceResult<ExerciseDto>> GetByIdAsync(ExerciseId id);
    
    /// <summary>
    /// Checks if an exercise exists by ID
    /// </summary>
    /// <param name="id">The exercise ID</param>
    /// <returns>Service result containing true if exists, false otherwise</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseId id);
    
    /// <summary>
    /// Checks if an exercise with the given name exists
    /// </summary>
    /// <param name="name">The exercise name</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
    /// <returns>Service result containing true if exists, false otherwise</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsByNameAsync(string name, ExerciseId? excludeId = null);
}