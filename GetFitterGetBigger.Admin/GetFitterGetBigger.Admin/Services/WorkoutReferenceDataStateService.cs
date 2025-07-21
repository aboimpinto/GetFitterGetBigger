using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services;

public class WorkoutReferenceDataStateService : IWorkoutReferenceDataStateService
{
    private readonly IWorkoutReferenceDataService _workoutReferenceDataService;
    private readonly ILogger<WorkoutReferenceDataStateService> _logger;
    
    // Workout Objectives state
    private List<ReferenceDataDto> _workoutObjectives = new();
    private bool _isLoadingObjectives;
    private string? _objectivesError;
    private string _objectivesSearchTerm = string.Empty;
    private ReferenceDataDto? _selectedObjective;
    
    // Workout Categories state
    private List<WorkoutCategoryDto> _workoutCategories = new();
    private bool _isLoadingCategories;
    private string? _categoriesError;
    private string _categoriesSearchTerm = string.Empty;
    private WorkoutCategoryDto? _selectedCategory;
    
    // Execution Protocols state
    private List<ExecutionProtocolDto> _executionProtocols = new();
    private bool _isLoadingProtocols;
    private string? _protocolsError;
    private string _protocolsSearchTerm = string.Empty;
    private string _selectedIntensityLevel = string.Empty;
    private ExecutionProtocolDto? _selectedProtocol;

    public event Action? OnChange;

    public WorkoutReferenceDataStateService(
        IWorkoutReferenceDataService workoutReferenceDataService,
        ILogger<WorkoutReferenceDataStateService> logger)
    {
        _workoutReferenceDataService = workoutReferenceDataService;
        _logger = logger;
    }
    
    public async Task InitializeAsync()
    {
        var loadTasks = new List<Task>
        {
            LoadWorkoutObjectivesAsync(),
            LoadWorkoutCategoriesAsync(),
            LoadExecutionProtocolsAsync()
        };
        
        await Task.WhenAll(loadTasks);
    }

    // Workout Objectives properties
    public IEnumerable<ReferenceDataDto> WorkoutObjectives => _workoutObjectives;

    public IEnumerable<ReferenceDataDto> FilteredWorkoutObjectives
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_objectivesSearchTerm))
                return _workoutObjectives;

            return _workoutObjectives.Where(o =>
                o.Value.Contains(_objectivesSearchTerm, StringComparison.OrdinalIgnoreCase) ||
                (o.Description?.Contains(_objectivesSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
        }
    }

    public bool IsLoadingObjectives => _isLoadingObjectives;
    public string? ObjectivesError => _objectivesError;
    public string ObjectivesSearchTerm
    {
        get => _objectivesSearchTerm;
        set
        {
            _objectivesSearchTerm = value;
            NotifyStateChanged();
        }
    }
    public ReferenceDataDto? SelectedObjective => _selectedObjective;

    // Workout Categories properties
    public IEnumerable<WorkoutCategoryDto> WorkoutCategories => _workoutCategories;

    public IEnumerable<WorkoutCategoryDto> FilteredWorkoutCategories
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_categoriesSearchTerm))
                return _workoutCategories;

            return _workoutCategories.Where(c =>
                c.Value.Contains(_categoriesSearchTerm, StringComparison.OrdinalIgnoreCase) ||
                (c.Description?.Contains(_categoriesSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.PrimaryMuscleGroups?.Contains(_categoriesSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
        }
    }

    public bool IsLoadingCategories => _isLoadingCategories;
    public string? CategoriesError => _categoriesError;
    public string CategoriesSearchTerm
    {
        get => _categoriesSearchTerm;
        set
        {
            _categoriesSearchTerm = value;
            NotifyStateChanged();
        }
    }
    public WorkoutCategoryDto? SelectedCategory => _selectedCategory;

    // Execution Protocols properties
    public IEnumerable<ExecutionProtocolDto> ExecutionProtocols => _executionProtocols;

    public IEnumerable<ExecutionProtocolDto> FilteredExecutionProtocols
    {
        get
        {
            var filtered = _executionProtocols.AsEnumerable();

            // Apply intensity level filter
            if (!string.IsNullOrWhiteSpace(_selectedIntensityLevel))
            {
                filtered = filtered.Where(p => p.IntensityLevel == _selectedIntensityLevel);
            }

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(_protocolsSearchTerm))
            {
                filtered = filtered.Where(p =>
                    p.Value.Contains(_protocolsSearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Code.Contains(_protocolsSearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (p.Description?.Contains(_protocolsSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            return filtered;
        }
    }

    public bool IsLoadingProtocols => _isLoadingProtocols;
    public string? ProtocolsError => _protocolsError;
    public string ProtocolsSearchTerm
    {
        get => _protocolsSearchTerm;
        set
        {
            _protocolsSearchTerm = value;
            NotifyStateChanged();
        }
    }
    public string SelectedIntensityLevel
    {
        get => _selectedIntensityLevel;
        set
        {
            _selectedIntensityLevel = value;
            NotifyStateChanged();
        }
    }
    public ExecutionProtocolDto? SelectedProtocol => _selectedProtocol;

    // Methods
    public async Task LoadWorkoutObjectivesAsync()
    {
        _logger.LogInformation("LoadWorkoutObjectivesAsync called");
        try
        {
            _isLoadingObjectives = true;
            _objectivesError = null;
            NotifyStateChanged();

            _logger.LogInformation("Calling WorkoutReferenceDataService.GetWorkoutObjectivesAsync");
            var objectives = await _workoutReferenceDataService.GetWorkoutObjectivesAsync();
            _workoutObjectives = objectives.OrderBy(o => o.Value).ToList();
            _logger.LogInformation("Successfully loaded {Count} workout objectives", _workoutObjectives.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load workout objectives");
            _objectivesError = $"Failed to load workout objectives: {ex.Message}";
        }
        finally
        {
            _isLoadingObjectives = false;
            NotifyStateChanged();
        }
    }

    public async Task LoadWorkoutCategoriesAsync()
    {
        try
        {
            _isLoadingCategories = true;
            _categoriesError = null;
            NotifyStateChanged();

            var categories = await _workoutReferenceDataService.GetWorkoutCategoriesAsync();
            _workoutCategories = categories.OrderBy(c => c.Value).ToList();
        }
        catch (Exception ex)
        {
            _categoriesError = $"Failed to load workout categories: {ex.Message}";
        }
        finally
        {
            _isLoadingCategories = false;
            NotifyStateChanged();
        }
    }

    public async Task LoadExecutionProtocolsAsync()
    {
        try
        {
            _isLoadingProtocols = true;
            _protocolsError = null;
            NotifyStateChanged();

            var protocols = await _workoutReferenceDataService.GetExecutionProtocolsAsync();
            _executionProtocols = protocols.OrderBy(p => p.Code).ToList();
        }
        catch (Exception ex)
        {
            _protocolsError = $"Failed to load execution protocols: {ex.Message}";
        }
        finally
        {
            _isLoadingProtocols = false;
            NotifyStateChanged();
        }
    }

    public void SelectObjective(ReferenceDataDto? objective)
    {
        _selectedObjective = objective;
        NotifyStateChanged();
    }

    public void SelectCategory(WorkoutCategoryDto? category)
    {
        _selectedCategory = category;
        NotifyStateChanged();
    }

    public void SelectProtocol(ExecutionProtocolDto? protocol)
    {
        _selectedProtocol = protocol;
        NotifyStateChanged();
    }

    public void ClearObjectivesError()
    {
        _objectivesError = null;
        NotifyStateChanged();
    }

    public void ClearCategoriesError()
    {
        _categoriesError = null;
        NotifyStateChanged();
    }

    public void ClearProtocolsError()
    {
        _protocolsError = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}