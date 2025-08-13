using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;

/// <summary>
/// Data service interface for WorkoutTemplate read operations.
/// Handles all database queries and entity-to-DTO mapping.
/// </summary>
public interface IWorkoutTemplateQueryDataService
{
    /// <summary>
    /// Loads a workout template by ID with all related data
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>Service result containing the workout template DTO or Empty if not found</returns>
    Task<ServiceResult<WorkoutTemplateDto>> GetByIdWithDetailsAsync(WorkoutTemplateId id);
    
    /// <summary>
    /// Checks if a workout template exists by ID
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>Service result containing true if exists, false otherwise</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutTemplateId id);
    
    /// <summary>
    /// Checks if a workout template with the given name exists
    /// </summary>
    /// <param name="name">The template name</param>
    /// <returns>Service result containing true if exists, false otherwise</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsByNameAsync(string name);
    
    /// <summary>
    /// Searches workout templates with multiple filter criteria
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="namePattern">Name pattern to search (empty string for no filter)</param>
    /// <param name="categoryId">Category filter (Empty for no filter)</param>
    /// <param name="objectiveId">Objective filter (Empty for no filter)</param>
    /// <param name="difficultyId">Difficulty filter (Empty for no filter)</param>
    /// <param name="stateId">State filter (Empty for no filter)</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortOrder">Sort order (asc/desc)</param>
    /// <returns>Service result containing paginated workout templates</returns>
    Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> SearchAsync(
        int page,
        int pageSize,
        string namePattern,
        WorkoutCategoryId categoryId,
        WorkoutObjectiveId objectiveId,
        DifficultyLevelId difficultyId,
        WorkoutStateId stateId,
        string sortBy,
        string sortOrder);
    
    /// <summary>
    /// Gets the count of workout templates matching the criteria
    /// </summary>
    /// <param name="namePattern">Optional name pattern filter</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="objectiveId">Optional objective filter</param>
    /// <param name="difficultyId">Optional difficulty filter</param>
    /// <param name="stateId">Optional state filter</param>
    /// <returns>Service result containing the count</returns>
    Task<ServiceResult<int>> GetCountAsync(
        string? namePattern = null,
        WorkoutCategoryId? categoryId = null,
        WorkoutObjectiveId? objectiveId = null,
        DifficultyLevelId? difficultyId = null,
        WorkoutStateId? stateId = null);
    
    /// <summary>
    /// Gets workout templates by creator ID
    /// </summary>
    /// <param name="createdById">The creator's user ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Service result containing paginated workout templates</returns>
    Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> GetByCreatorAsync(
        UserId createdById,
        int page = 1,
        int pageSize = 10);
    
    /// <summary>
    /// Gets workout templates in a specific state
    /// </summary>
    /// <param name="stateId">The workout state ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Service result containing paginated workout templates</returns>
    Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> GetByStateAsync(
        WorkoutStateId stateId,
        int page = 1,
        int pageSize = 10);
    
    /// <summary>
    /// Checks if a workout template has any execution logs
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>Service result containing true if has execution logs, false otherwise</returns>
    Task<ServiceResult<BooleanResultDto>> HasExecutionLogsAsync(WorkoutTemplateId id);
    
    /// <summary>
    /// Gets the state of a workout template
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>Service result containing the state ID</returns>
    Task<ServiceResult<WorkoutStateId>> GetStateAsync(WorkoutTemplateId id);
}