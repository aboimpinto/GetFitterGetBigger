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
        /// <returns>A service result containing the workout template data or error information</returns>
        Task<ServiceResult<WorkoutTemplateDto>> GetWorkoutTemplateByIdAsync(string id);
        
        /// <summary>
        /// Creates a new workout template
        /// </summary>
        /// <param name="template">The workout template creation data</param>
        /// <returns>A service result containing the created workout template or validation errors</returns>
        Task<ServiceResult<WorkoutTemplateDto>> CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto template);
        
        /// <summary>
        /// Updates an existing workout template
        /// </summary>
        /// <param name="id">The unique identifier of the workout template to update</param>
        /// <param name="template">The updated workout template data</param>
        /// <returns>A service result containing the updated workout template or validation errors</returns>
        Task<ServiceResult<WorkoutTemplateDto>> UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto template);
        
        /// <summary>
        /// Deletes a workout template by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the workout template to delete</param>
        /// <returns>A service result indicating success or failure</returns>
        Task<ServiceResult<bool>> DeleteWorkoutTemplateAsync(string id);
        
        /// <summary>
        /// Changes the state of a workout template (e.g., from draft to active)
        /// </summary>
        /// <param name="id">The unique identifier of the workout template</param>
        /// <param name="changeState">The state change data</param>
        /// <returns>A service result containing the updated workout template with the new state or errors</returns>
        Task<ServiceResult<WorkoutTemplateDto>> ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState);
        
        /// <summary>
        /// Creates a duplicate copy of an existing workout template
        /// </summary>
        /// <param name="id">The unique identifier of the workout template to duplicate</param>
        /// <param name="duplicate">The duplication options</param>
        /// <returns>A service result containing the newly created duplicate workout template or errors</returns>
        Task<ServiceResult<WorkoutTemplateDto>> DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate);
        
        /// <summary>
        /// Checks if a workout template name already exists in the system
        /// </summary>
        /// <param name="name">The template name to check</param>
        /// <returns>A service result containing true if the name exists, false otherwise</returns>
        Task<ServiceResult<bool>> CheckTemplateNameExistsAsync(string name);
    }
}