using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Exercise;

/// <summary>
/// Service interface for Exercise business logic
/// </summary>
public interface IExerciseService
{
    /// <summary>
    /// Gets a paginated list of exercises with filtering
    /// </summary>
    /// <param name="filterParams">The filtering and pagination parameters</param>
    /// <returns>Service result containing a paged response of exercises</returns>
    Task<ServiceResult<PagedResponse<ExerciseDto>>> GetPagedAsync(GetExercisesCommand filterParams);
    
    /// <summary>
    /// Gets an exercise by its ID
    /// </summary>
    /// <param name="id">The ID of the exercise</param>
    /// <returns>Service result containing the exercise DTO if found, or appropriate error</returns>
    Task<ServiceResult<ExerciseDto>> GetByIdAsync(ExerciseId id);
    
    /// <summary>
    /// Creates a new exercise
    /// </summary>
    /// <param name="command">The exercise creation command</param>
    /// <returns>Service result containing the created exercise DTO or validation errors</returns>
    Task<ServiceResult<ExerciseDto>> CreateAsync(CreateExerciseCommand command);
    
    /// <summary>
    /// Updates an existing exercise
    /// </summary>
    /// <param name="id">The ID of the exercise to update</param>
    /// <param name="command">The exercise update command</param>
    /// <returns>Service result containing the updated exercise DTO or validation errors</returns>
    Task<ServiceResult<ExerciseDto>> UpdateAsync(ExerciseId id, UpdateExerciseCommand command);
    
    /// <summary>
    /// Deletes an exercise
    /// </summary>
    /// <param name="id">The ID of the exercise to delete</param>
    /// <returns>ServiceResult indicating success or failure with appropriate error messages</returns>
    /// <remarks>
    /// If the exercise has references, it will be soft deleted (marked as inactive).
    /// If the exercise has no references, it will be hard deleted from the database.
    /// </remarks>
    Task<ServiceResult<BooleanResultDto>> DeleteAsync(ExerciseId id);
}