using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Results;

namespace GetFitterGetBigger.Admin.Services
{
    /// <summary>
    /// Service interface for managing workout templates including CRUD operations and reference data retrieval
    /// </summary>
    public interface IWorkoutTemplateService
    {
        /// <summary>
        /// Retrieves a paginated list of workout templates based on the provided filter criteria
        /// </summary>
        /// <param name="filter">Filter criteria for querying workout templates</param>
        /// <returns>A service result containing the paginated workout template data</returns>
        Task<ServiceResult<WorkoutTemplatePagedResultDto>> GetWorkoutTemplatesAsync(WorkoutTemplateFilterDto filter);
        
        /// <summary>
        /// Retrieves a specific workout template by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the workout template</param>
        /// <returns>The workout template if found, null otherwise</returns>
        Task<WorkoutTemplateDto?> GetWorkoutTemplateByIdAsync(string id);
        
        /// <summary>
        /// Creates a new workout template
        /// </summary>
        /// <param name="template">The workout template creation data</param>
        /// <returns>The created workout template</returns>
        Task<WorkoutTemplateDto> CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto template);
        
        /// <summary>
        /// Updates an existing workout template
        /// </summary>
        /// <param name="id">The unique identifier of the workout template to update</param>
        /// <param name="template">The updated workout template data</param>
        /// <returns>The updated workout template</returns>
        Task<WorkoutTemplateDto> UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto template);
        
        /// <summary>
        /// Deletes a workout template by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the workout template to delete</param>
        Task DeleteWorkoutTemplateAsync(string id);
        
        /// <summary>
        /// Changes the state of a workout template (e.g., from draft to active)
        /// </summary>
        /// <param name="id">The unique identifier of the workout template</param>
        /// <param name="changeState">The state change data</param>
        /// <returns>The updated workout template with the new state</returns>
        Task<WorkoutTemplateDto> ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState);
        
        /// <summary>
        /// Creates a duplicate copy of an existing workout template
        /// </summary>
        /// <param name="id">The unique identifier of the workout template to duplicate</param>
        /// <param name="duplicate">The duplication options</param>
        /// <returns>The newly created duplicate workout template</returns>
        Task<WorkoutTemplateDto> DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate);
        
        /// <summary>
        /// Retrieves all exercises associated with a specific workout template
        /// </summary>
        /// <param name="templateId">The unique identifier of the workout template</param>
        /// <returns>A list of exercises for the template</returns>
        Task<List<WorkoutTemplateExerciseDto>> GetTemplateExercisesAsync(string templateId);
        
        /// <summary>
        /// Checks if a workout template name already exists in the system
        /// </summary>
        /// <param name="name">The template name to check</param>
        /// <returns>True if the name exists, false otherwise</returns>
        Task<bool> CheckTemplateNameExistsAsync(string name);
        
        // Reference data methods
        
        /// <summary>
        /// Retrieves all available workout categories
        /// </summary>
        /// <returns>A list of workout category reference data</returns>
        Task<List<ReferenceDataDto>> GetWorkoutCategoriesAsync();
        
        /// <summary>
        /// Retrieves all available difficulty levels
        /// </summary>
        /// <returns>A list of difficulty level reference data</returns>
        Task<List<ReferenceDataDto>> GetDifficultyLevelsAsync();
        
        /// <summary>
        /// Retrieves all available workout states
        /// </summary>
        /// <returns>A list of workout state reference data</returns>
        Task<List<ReferenceDataDto>> GetWorkoutStatesAsync();
        
        /// <summary>
        /// Retrieves all available workout objectives
        /// </summary>
        /// <returns>A list of workout objective reference data</returns>
        Task<List<ReferenceDataDto>> GetWorkoutObjectivesAsync();
    }
}