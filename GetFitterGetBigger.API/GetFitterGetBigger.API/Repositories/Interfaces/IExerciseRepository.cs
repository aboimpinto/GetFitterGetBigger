using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for Exercise data with advanced querying capabilities
/// </summary>
public interface IExerciseRepository : IRepository
{
    /// <summary>
    /// Gets a paginated list of exercises with optional filtering
    /// </summary>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="name">Filter by exercise name (partial match, use string.Empty for no filter)</param>
    /// <param name="difficultyId">Filter by difficulty level (use DifficultyLevelId.Empty for no filter)</param>
    /// <param name="muscleGroupIds">Filter by muscle groups (use empty list for no filter)</param>
    /// <param name="equipmentIds">Filter by equipment (use empty list for no filter)</param>
    /// <param name="movementPatternIds">Filter by movement patterns (use empty list for no filter)</param>
    /// <param name="bodyPartIds">Filter by body parts (use empty list for no filter)</param>
    /// <param name="includeInactive">Whether to include inactive exercises</param>
    /// <returns>A tuple containing the exercises and total count</returns>
    Task<(IEnumerable<Exercise> exercises, int totalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string name,
        DifficultyLevelId difficultyId,
        IEnumerable<MuscleGroupId> muscleGroupIds,
        IEnumerable<EquipmentId> equipmentIds,
        IEnumerable<MovementPatternId> movementPatternIds,
        IEnumerable<BodyPartId> bodyPartIds,
        bool includeInactive = false);
    
    /// <summary>
    /// Gets an exercise by its ID with all related data
    /// </summary>
    /// <param name="id">The ID of the exercise to retrieve</param>
    /// <returns>The exercise if found, null otherwise</returns>
    Task<Exercise?> GetByIdAsync(ExerciseId id);
    
    /// <summary>
    /// Gets an exercise by its name
    /// </summary>
    /// <param name="name">The name of the exercise to retrieve</param>
    /// <returns>The exercise if found, null otherwise</returns>
    Task<Exercise?> GetByNameAsync(string name);
    
    /// <summary>
    /// Checks if an exercise name already exists (case-insensitive)
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
    /// <returns>True if the name exists, false otherwise</returns>
    Task<bool> ExistsAsync(string name, ExerciseId? excludeId = null);
    
    /// <summary>
    /// Checks if an exercise has any references in workouts or user data
    /// </summary>
    /// <param name="id">The ID of the exercise to check</param>
    /// <returns>True if references exist, false otherwise</returns>
    Task<bool> HasReferencesAsync(ExerciseId id);
    
    /// <summary>
    /// Adds a new exercise to the repository
    /// </summary>
    /// <param name="exercise">The exercise to add</param>
    /// <returns>The added exercise</returns>
    Task<Exercise> AddAsync(Exercise exercise);
    
    /// <summary>
    /// Updates an existing exercise
    /// </summary>
    /// <param name="exercise">The exercise with updated data</param>
    /// <returns>The updated exercise</returns>
    Task<Exercise> UpdateAsync(Exercise exercise);
    
    /// <summary>
    /// Deletes an exercise from the repository
    /// </summary>
    /// <param name="id">The ID of the exercise to delete</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteAsync(ExerciseId id);
}