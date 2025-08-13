using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Infrastructure;
using GetFitterGetBigger.API.Services.Results;
using ExerciseEntity = GetFitterGetBigger.API.Models.Entities.Exercise;

namespace GetFitterGetBigger.API.Services.Exercise.DataServices;

/// <summary>
/// Data service interface for Exercise write operations.
/// Handles all database modifications and entity-to-DTO mapping.
/// </summary>
public interface IExerciseCommandDataService
{
    /// <summary>
    /// Creates a new exercise
    /// </summary>
    /// <param name="entity">The exercise entity to create</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result containing the created exercise DTO</returns>
    Task<ServiceResult<ExerciseDto>> CreateAsync(
        ExerciseEntity entity,
        ITransactionScope? scope = null);
    
    /// <summary>
    /// Updates an existing exercise
    /// </summary>
    /// <param name="id">The exercise ID</param>
    /// <param name="updateAction">Function to apply updates to the entity</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result containing the updated exercise DTO</returns>
    Task<ServiceResult<ExerciseDto>> UpdateAsync(
        ExerciseId id,
        Func<ExerciseEntity, ExerciseEntity> updateAction,
        ITransactionScope? scope = null);
    
    /// <summary>
    /// Soft deletes an exercise (marks as inactive)
    /// </summary>
    /// <param name="id">The exercise ID</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result indicating success or failure</returns>
    Task<ServiceResult<BooleanResultDto>> SoftDeleteAsync(
        ExerciseId id,
        ITransactionScope? scope = null);
    
    /// <summary>
    /// Hard deletes an exercise from the database
    /// </summary>
    /// <param name="id">The exercise ID</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result indicating success or failure</returns>
    Task<ServiceResult<BooleanResultDto>> HardDeleteAsync(
        ExerciseId id,
        ITransactionScope? scope = null);
}