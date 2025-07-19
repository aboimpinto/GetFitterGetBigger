using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services;

public interface IWorkoutReferenceDataStateService
{
    event Action? OnChange;

    // Workout Objectives state
    IEnumerable<ReferenceDataDto> WorkoutObjectives { get; }
    IEnumerable<ReferenceDataDto> FilteredWorkoutObjectives { get; }
    bool IsLoadingObjectives { get; }
    string? ObjectivesError { get; }
    string ObjectivesSearchTerm { get; set; }
    ReferenceDataDto? SelectedObjective { get; }

    // Workout Categories state
    IEnumerable<WorkoutCategoryDto> WorkoutCategories { get; }
    IEnumerable<WorkoutCategoryDto> FilteredWorkoutCategories { get; }
    bool IsLoadingCategories { get; }
    string? CategoriesError { get; }
    string CategoriesSearchTerm { get; set; }
    WorkoutCategoryDto? SelectedCategory { get; }

    // Execution Protocols state
    IEnumerable<ExecutionProtocolDto> ExecutionProtocols { get; }
    IEnumerable<ExecutionProtocolDto> FilteredExecutionProtocols { get; }
    bool IsLoadingProtocols { get; }
    string? ProtocolsError { get; }
    string ProtocolsSearchTerm { get; set; }
    string SelectedIntensityLevel { get; set; }
    ExecutionProtocolDto? SelectedProtocol { get; }

    // Methods
    Task LoadWorkoutObjectivesAsync();
    Task LoadWorkoutCategoriesAsync();
    Task LoadExecutionProtocolsAsync();
    
    void SelectObjective(ReferenceDataDto? objective);
    void SelectCategory(WorkoutCategoryDto? category);
    void SelectProtocol(ExecutionProtocolDto? protocol);
    
    void ClearObjectivesError();
    void ClearCategoriesError();
    void ClearProtocolsError();
}