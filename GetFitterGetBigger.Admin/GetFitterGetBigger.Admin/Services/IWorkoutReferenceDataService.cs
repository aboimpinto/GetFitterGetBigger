using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services;

public interface IWorkoutReferenceDataService
{
    Task<List<ReferenceDataDto>> GetWorkoutObjectivesAsync();
    Task<ReferenceDataDto?> GetWorkoutObjectiveByIdAsync(string id);
    
    Task<List<WorkoutCategoryDto>> GetWorkoutCategoriesAsync();
    Task<WorkoutCategoryDto?> GetWorkoutCategoryByIdAsync(string id);
    
    Task<List<ExecutionProtocolDto>> GetExecutionProtocolsAsync();
    Task<ExecutionProtocolDto?> GetExecutionProtocolByIdAsync(string id);
    Task<ExecutionProtocolDto?> GetExecutionProtocolByCodeAsync(string code);
}