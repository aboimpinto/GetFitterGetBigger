using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.Admin.Components.WorkoutTemplates;

public partial class WorkoutTemplateForm : ComponentBase, IDisposable
{
    [Parameter] public WorkoutTemplateFormModel Model { get; set; } = new();
    [Parameter] public EventCallback<WorkoutTemplateFormModel> OnValidSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public string SubmitButtonText { get; set; } = "Save";
    [Parameter] public bool IsEditMode { get; set; }
    [Parameter] public WorkoutTemplateDto? ExistingTemplate { get; set; }
    [Parameter] public bool EnableAutoSave { get; set; }
    [Parameter] public Func<string, bool>? FieldDisabledPredicate { get; set; }
    [Parameter] public bool EnableNameValidation { get; set; } = true;
    
    [Inject] private IWorkoutTemplateService WorkoutTemplateService { get; set; } = default!;
    [Inject] private IWorkoutTemplateStateService WorkoutTemplateStateService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILocalStorageService LocalStorageService { get; set; } = default!;
    
    // Reference data
    internal List<ReferenceDataDto> Categories = new();
    internal List<ReferenceDataDto> Difficulties = new();
    internal List<ReferenceDataDto> Objectives = new();
    
    // Form state
    internal bool IsLoading { get; set; } = false;
    internal bool IsSubmitting = false;
    internal bool IsSaving => IsSubmitting;
    internal bool ShowButtons { get; set; } = true;
    internal bool IsValid => IsFormValid();
    internal string? ErrorMessage;
    internal string TagsInput = string.Empty;
    internal bool ShowNameExistsWarning = false;
    internal string? WorkoutStateInfo;
    
    // Auto-save state
    internal bool ShowAutoSaveIndicator => EnableAutoSave && IsEditMode;
    internal bool IsAutoSaving = false;
    internal DateTime? LastAutoSaved;
    internal Timer? autoSaveTimer;
    internal bool isDirty = false;
    
    // Name validation
    private string? lastValidatedName;
    private CancellationTokenSource? nameValidationCts;
    
    // Unsaved changes handling
    private const string LocalStorageKey = "workoutTemplateFormData";
    internal bool ShowUnsavedChangesDialog = false;
    internal bool ShowRecoveryDialog = false;
    internal string? pendingNavigationUrl;
    private IDisposable? navigationRegistration;
    
    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await LoadReferenceData();
        InitializeFormState();
        
        if (EnableAutoSave && IsEditMode)
        {
            StartAutoSaveTimer();
        }
        
        // Register for navigation events
        navigationRegistration = NavigationManager.RegisterLocationChangingHandler(OnLocationChanging);
        
        // Check for recovery data
        await CheckForRecoveryData();
    }
    
    protected override async Task OnParametersSetAsync()
    {
        // Re-initialize if model changes
        if (Model != null)
        {
            InitializeFormState();
            
            // Trigger name validation if name has value
            if (!string.IsNullOrWhiteSpace(Model.Name) && EnableNameValidation)
            {
                await ValidateNameAsync(Model.Name);
            }
        }
    }
    
    private async Task LoadReferenceData()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            
            var categoriesTask = WorkoutTemplateService.GetWorkoutCategoriesAsync();
            var difficultiesTask = WorkoutTemplateService.GetDifficultyLevelsAsync();
            var objectivesTask = WorkoutTemplateService.GetWorkoutObjectivesAsync();
            
            await Task.WhenAll(categoriesTask, difficultiesTask, objectivesTask);
            
            Categories = await categoriesTask ?? new List<ReferenceDataDto>();
            Difficulties = await difficultiesTask ?? new List<ReferenceDataDto>();
            Objectives = await objectivesTask ?? new List<ReferenceDataDto>();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load reference data. Please try again.";
            Console.Error.WriteLine($"Error loading reference data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private void InitializeFormState()
    {
        // Initialize tags input from model
        TagsInput = string.Join(", ", Model.Tags);
        
        // Set workout state info
        if (!IsEditMode)
        {
            WorkoutStateInfo = "New templates will be created in Draft state.";
        }
        else if (ExistingTemplate != null)
        {
            WorkoutStateInfo = ExistingTemplate.WorkoutState?.Id switch
            {
                "PRODUCTION" => "This template is in Production state. Some fields may be restricted.",
                "ARCHIVED" => "This template is Archived. Only limited changes are allowed.",
                _ => null
            };
        }
    }
    
    internal void ProcessTags()
    {
        if (!string.IsNullOrWhiteSpace(TagsInput))
        {
            Model.Tags = TagsInput
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();
        }
        else
        {
            Model.Tags.Clear();
        }
        
        MarkAsDirty();
    }
    
    internal bool IsFieldDisabled(string fieldName)
    {
        if (FieldDisabledPredicate != null)
        {
            return FieldDisabledPredicate(fieldName);
        }
        
        return false;
    }
    
    internal bool IsFormValid()
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(Model.Name)) return false;
        if (string.IsNullOrWhiteSpace(Model.CategoryId)) return false;
        if (string.IsNullOrWhiteSpace(Model.DifficultyId)) return false;
        if (Model.EstimatedDurationMinutes < 5 || Model.EstimatedDurationMinutes > 300) return false;
        
        // Name exists check
        if (ShowNameExistsWarning) return false;
        
        return true;
    }
    
    internal async Task HandleValidSubmit()
    {
        try
        {
            IsSubmitting = true;
            ErrorMessage = null;
            
            // Process tags one more time
            ProcessTags();
            
            await OnValidSubmit.InvokeAsync(Model);
            
            // Clear dirty flag and recovery data on successful save
            isDirty = false;
            await ClearRecoveryData();
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.StatusCode switch
            {
                System.Net.HttpStatusCode.Conflict => "A workout template with this name already exists. Please choose a different name.",
                System.Net.HttpStatusCode.BadRequest => "Invalid data provided. Please check your input and try again.",
                System.Net.HttpStatusCode.Unauthorized => "You are not authorized to perform this action. Please log in again.",
                System.Net.HttpStatusCode.ServiceUnavailable => "The service is temporarily unavailable. Please try again later.",
                _ => "Unable to save the workout template. Please check your connection and try again."
            };
            Console.Error.WriteLine($"HTTP error in form submit: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            ErrorMessage = "The operation timed out. Please check your connection and try again.";
        }
        catch (Exception ex)
        {
            ErrorMessage = "An unexpected error occurred while saving. Please try again.";
            Console.Error.WriteLine($"Error in form submit: {ex.Message}");
        }
        finally
        {
            IsSubmitting = false;
        }
    }
    
    internal async Task HandleSubmitFromFloating()
    {
        // Validate form before submitting
        if (!IsFormValid())
        {
            return;
        }
        
        await HandleValidSubmit();
    }
    
    internal async Task TriggerFormSubmit()
    {
        await HandleSubmitFromFloating();
    }
    
    private async Task HandleCancel()
    {
        StopAutoSaveTimer();
        isDirty = false;
        await ClearRecoveryData();
        await OnCancel.InvokeAsync();
    }
    
    // Name validation
    internal async Task ValidateNameAsync(string name)
    {
        if (!EnableNameValidation || string.IsNullOrWhiteSpace(name))
        {
            ShowNameExistsWarning = false;
            return;
        }
        
        // Skip if already validated
        if (name == lastValidatedName)
        {
            return;
        }
        
        // Cancel previous validation
        nameValidationCts?.Cancel();
        nameValidationCts = new CancellationTokenSource();
        
        try
        {
            // Debounce
            await Task.Delay(500, nameValidationCts.Token);
            
            // Skip validation for edit mode if name hasn't changed
            if (IsEditMode && ExistingTemplate != null && name == ExistingTemplate.Name)
            {
                ShowNameExistsWarning = false;
                lastValidatedName = name;
                return;
            }
            
            var exists = await WorkoutTemplateService.CheckTemplateNameExistsAsync(name);
            ShowNameExistsWarning = exists;
            lastValidatedName = name;
            
            await InvokeAsync(StateHasChanged);
        }
        catch (TaskCanceledException)
        {
            // Validation was cancelled, ignore
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error validating name: {ex.Message}");
        }
    }
    
    // Auto-save functionality
    private void StartAutoSaveTimer()
    {
        autoSaveTimer = new Timer(async _ => await AutoSave(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }
    
    private void StopAutoSaveTimer()
    {
        autoSaveTimer?.Dispose();
        autoSaveTimer = null;
    }
    
    internal void MarkAsDirty()
    {
        isDirty = true;
    }
    
    internal async Task AutoSave()
    {
        if (!isDirty || IsSubmitting || !IsFormValid())
        {
            return;
        }
        
        try
        {
            IsAutoSaving = true;
            await InvokeAsync(StateHasChanged);
            
            // Call the save callback
            await OnValidSubmit.InvokeAsync(Model);
            
            LastAutoSaved = DateTime.Now;
            isDirty = false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Auto-save error: {ex.Message}");
        }
        finally
        {
            IsAutoSaving = false;
            await InvokeAsync(StateHasChanged);
        }
    }
    
    internal string GetLastSavedTimeAgo()
    {
        if (!LastAutoSaved.HasValue) return "Never";
        
        var timeAgo = DateTime.Now - LastAutoSaved.Value;
        
        if (timeAgo.TotalSeconds < 60)
            return "Just now";
        else if (timeAgo.TotalMinutes < 60)
            return $"{(int)timeAgo.TotalMinutes} minute{((int)timeAgo.TotalMinutes == 1 ? "" : "s")} ago";
        else
            return $"{(int)timeAgo.TotalHours} hour{((int)timeAgo.TotalHours == 1 ? "" : "s")} ago";
    }
    
    // Unsaved changes handling
    internal async ValueTask OnLocationChanging(LocationChangingContext context)
    {
        if (!isDirty || IsAutoSaving || IsSubmitting)
        {
            return;
        }
        
        // Store the navigation URL and prevent navigation
        pendingNavigationUrl = context.TargetLocation;
        context.PreventNavigation();
        
        // Save form data to local storage for recovery
        await SaveFormDataToLocalStorage();
        
        // Show unsaved changes dialog
        ShowUnsavedChangesDialog = true;
        await InvokeAsync(StateHasChanged);
    }
    
    internal async Task CheckForRecoveryData()
    {
        try
        {
            var savedData = await LocalStorageService.GetItemAsync(LocalStorageKey);
            if (!string.IsNullOrEmpty(savedData))
            {
                ShowRecoveryDialog = true;
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error checking for recovery data: {ex.Message}");
        }
    }
    
    internal async Task SaveFormDataToLocalStorage()
    {
        try
        {
            var formData = System.Text.Json.JsonSerializer.Serialize(Model);
            await LocalStorageService.SetItemAsync(LocalStorageKey, formData);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving form data: {ex.Message}");
        }
    }
    
    internal async Task RecoverFormData()
    {
        try
        {
            var savedData = await LocalStorageService.GetItemAsync(LocalStorageKey);
            if (!string.IsNullOrEmpty(savedData))
            {
                var recoveredModel = System.Text.Json.JsonSerializer.Deserialize<WorkoutTemplateFormModel>(savedData);
                if (recoveredModel != null)
                {
                    Model = recoveredModel;
                    InitializeFormState();
                    MarkAsDirty();
                    
                    // Clear recovery data
                    await ClearRecoveryData();
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error recovering form data: {ex.Message}");
        }
        finally
        {
            ShowRecoveryDialog = false;
            await InvokeAsync(StateHasChanged);
        }
    }
    
    internal async Task DiscardRecoveryData()
    {
        await ClearRecoveryData();
        ShowRecoveryDialog = false;
        await InvokeAsync(StateHasChanged);
    }
    
    private async Task ClearRecoveryData()
    {
        try
        {
            await LocalStorageService.RemoveItemAsync(LocalStorageKey);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error clearing recovery data: {ex.Message}");
        }
    }
    
    internal async Task ConfirmNavigation()
    {
        ShowUnsavedChangesDialog = false;
        isDirty = false;
        
        // Clear recovery data
        await ClearRecoveryData();
        
        if (!string.IsNullOrEmpty(pendingNavigationUrl))
        {
            NavigationManager.NavigateTo(pendingNavigationUrl);
        }
    }
    
    internal async Task CancelNavigation()
    {
        ShowUnsavedChangesDialog = false;
        pendingNavigationUrl = null;
        await InvokeAsync(StateHasChanged);
    }
    
    internal async Task SaveAndNavigate()
    {
        ShowUnsavedChangesDialog = false;
        
        // Try to save the form
        await HandleValidSubmit();
        
        // Navigate after save (HandleValidSubmit already clears recovery data)
        if (!string.IsNullOrEmpty(pendingNavigationUrl))
        {
            NavigationManager.NavigateTo(pendingNavigationUrl);
        }
    }
    
    public void Dispose()
    {
        StopAutoSaveTimer();
        nameValidationCts?.Cancel();
        nameValidationCts?.Dispose();
        navigationRegistration?.Dispose();
    }
    
    // Form Model for data binding
    public class WorkoutTemplateFormModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be less than 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Description must be less than 500 characters")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        public string CategoryId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Difficulty is required")]
        public string DifficultyId { get; set; } = string.Empty;
        
        public string? ObjectiveId { get; set; }
        
        [Required(ErrorMessage = "Duration is required")]
        [Range(5, 300, ErrorMessage = "Duration must be between 5 and 300 minutes")]
        public int EstimatedDurationMinutes { get; set; } = 30;
        
        public bool IsPublic { get; set; } = false;
        
        public List<string> Tags { get; set; } = new();
        
        // For edit mode
        public string? Id { get; set; }
        public string? WorkoutStateId { get; set; }
    }
}