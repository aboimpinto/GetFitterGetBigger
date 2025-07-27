using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Results;

namespace GetFitterGetBigger.Admin.Services.DataProviders
{
    /// <summary>
    /// Defines the contract for workout template data access operations.
    /// Implementations can use various data sources (HTTP, gRPC, Database, etc.).
    /// </summary>
    public interface IWorkoutTemplateDataProvider
    {
        /// <summary>
        /// Retrieves a paginated list of workout templates based on the provided filter.
        /// </summary>
        Task<DataServiceResult<WorkoutTemplatePagedResultDto>> GetWorkoutTemplatesAsync(WorkoutTemplateFilterDto filter);

        /// <summary>
        /// Retrieves a specific workout template by its ID.
        /// </summary>
        Task<DataServiceResult<WorkoutTemplateDto>> GetWorkoutTemplateByIdAsync(string id);

        /// <summary>
        /// Creates a new workout template.
        /// </summary>
        Task<DataServiceResult<WorkoutTemplateDto>> CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto template);

        /// <summary>
        /// Updates an existing workout template.
        /// </summary>
        Task<DataServiceResult<WorkoutTemplateDto>> UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto template);

        /// <summary>
        /// Deletes a workout template.
        /// </summary>
        Task<DataServiceResult<bool>> DeleteWorkoutTemplateAsync(string id);

        /// <summary>
        /// Changes the state of a workout template.
        /// </summary>
        Task<DataServiceResult<WorkoutTemplateDto>> ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState);

        /// <summary>
        /// Duplicates an existing workout template.
        /// </summary>
        Task<DataServiceResult<WorkoutTemplateDto>> DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate);

        /// <summary>
        /// Retrieves the exercises associated with a workout template.
        /// </summary>
        Task<DataServiceResult<List<WorkoutTemplateExerciseDto>>> GetTemplateExercisesAsync(string templateId);

        /// <summary>
        /// Checks if a workout template with the specified name already exists.
        /// </summary>
        Task<DataServiceResult<bool>> CheckTemplateNameExistsAsync(string name);

        /// <summary>
        /// Retrieves workout states reference data.
        /// </summary>
        Task<DataServiceResult<List<ReferenceDataDto>>> GetWorkoutStatesAsync();
    }
}