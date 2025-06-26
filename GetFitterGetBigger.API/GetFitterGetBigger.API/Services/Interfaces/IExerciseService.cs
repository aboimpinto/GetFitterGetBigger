using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for Exercise business logic
/// </summary>
public interface IExerciseService
{
    /// <summary>
    /// Gets a paginated list of exercises with filtering
    /// </summary>
    /// <param name="filterParams">The filtering and pagination parameters</param>
    /// <returns>A paged response containing exercises</returns>
    Task<PagedResponse<ExerciseDto>> GetPagedAsync(ExerciseFilterParams filterParams);
    
    /// <summary>
    /// Gets an exercise by its ID
    /// </summary>
    /// <param name="id">The ID of the exercise</param>
    /// <returns>The exercise DTO if found, null otherwise</returns>
    Task<ExerciseDto?> GetByIdAsync(string id);
    
    /// <summary>
    /// Creates a new exercise
    /// </summary>
    /// <param name="request">The exercise creation request</param>
    /// <returns>The created exercise DTO</returns>
    Task<ExerciseDto> CreateAsync(CreateExerciseRequest request);
    
    /// <summary>
    /// Updates an existing exercise
    /// </summary>
    /// <param name="id">The ID of the exercise to update</param>
    /// <param name="request">The exercise update request</param>
    /// <returns>The updated exercise DTO</returns>
    Task<ExerciseDto> UpdateAsync(string id, UpdateExerciseRequest request);
    
    /// <summary>
    /// Deletes an exercise
    /// </summary>
    /// <param name="id">The ID of the exercise to delete</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    /// <remarks>
    /// If the exercise has references, it will be soft deleted (marked as inactive).
    /// If the exercise has no references, it will be hard deleted from the database.
    /// </remarks>
    Task<bool> DeleteAsync(string id);
}