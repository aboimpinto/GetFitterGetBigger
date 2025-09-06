using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.AspNetCore.Components;

namespace GetFitterGetBigger.Admin.Components.Pages.Exercises.ExerciseLinks
{
    /// <summary>
    /// Main orchestrator component for four-way exercise linking system
    /// Supports context-aware relationships: workout, warmup, cooldown, and alternatives
    /// </summary>
    public partial class FourWayExerciseLinkManager : IDisposable
    {
        [Parameter, EditorRequired] public ExerciseDto Exercise { get; set; } = null!;
        [Parameter, EditorRequired] public IExerciseLinkStateService StateService { get; set; } = null!;
        [Parameter, EditorRequired] public IExerciseService ExerciseService { get; set; } = null!;
        [Parameter, EditorRequired] public IEnumerable<ExerciseTypeDto> ExerciseTypes { get; set; } = null!;
        
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private IExerciseLinkValidationService ValidationService { get; set; } = null!;

        private bool _showAddModal = false;
        private string _addLinkType = string.Empty;
        private ExerciseLinkDto? _linkToDelete;
        private string? _successMessage;
        private System.Threading.Timer? _successMessageTimer;
        private ElementReference _deleteConfirmButton;
        
        /// <summary>
        /// Show component for any exercise with types (removes "Workout" only restriction)
        /// </summary>
        private bool ShowComponent => Exercise.ExerciseTypes?.Any() == true;

        /// <summary>
        /// Check if this is a REST exercise (no linking allowed)
        /// </summary>
        private bool IsRestExercise => Exercise.ExerciseTypes?.Any(t => t.Value == "REST") == true;

        /// <summary>
        /// Check if exercise has multiple contexts (multi-type)
        /// </summary>
        private bool HasMultipleContexts => GetExerciseContexts().Count() > 1;

        /// <summary>
        /// Get available contexts for this exercise
        /// </summary>
        private IEnumerable<string> AvailableContexts => GetExerciseContexts();

        protected override async Task OnInitializedAsync()
        {
            if (ShowComponent && !IsRestExercise)
            {
                StateService.OnChange += HandleStateChange;
                // Use the new overload that takes the full Exercise object to properly set context
                await StateService.InitializeForExerciseAsync(Exercise);
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            // Handle exercise changes - re-initialize if exercise ID changed
            if (ShowComponent && !IsRestExercise && 
                !string.IsNullOrEmpty(StateService.CurrentExerciseId) && 
                StateService.CurrentExerciseId != Exercise.Id)
            {
                // Use the new overload that takes the full Exercise object to properly set context
                await StateService.InitializeForExerciseAsync(Exercise);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // Focus management for delete confirmation dialog
            if (_linkToDelete != null && _deleteConfirmButton.Id != null)
            {
                await _deleteConfirmButton.FocusAsync();
            }
        }

        private void HandleStateChange()
        {
            InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Get exercise contexts based on exercise types
        /// </summary>
        private IEnumerable<string> GetExerciseContexts()
        {
            var contexts = new List<string>();
            var types = Exercise.ExerciseTypes?.Select(t => t.Value) ?? Enumerable.Empty<string>();
            
            // Add contexts based on exercise types
            if (types.Contains("Workout")) contexts.Add("Workout");
            if (types.Contains("Warmup")) contexts.Add("Warmup");
            if (types.Contains("Cooldown")) contexts.Add("Cooldown");
            
            return contexts;
        }

        /// <summary>
        /// Handle context switching for multi-type exercises
        /// </summary>
        private async Task HandleContextSwitch(string contextType)
        {
            if (!string.IsNullOrEmpty(contextType) && contextType != StateService.ActiveContext)
            {
                await StateService.SwitchContextAsync(contextType);
                
                // Load context-specific data
                switch (contextType)
                {
                    case "Workout":
                        // Standard links (warmup/cooldown/alternatives) already loaded by SwitchContextAsync
                        break;
                        
                    case "Warmup":
                    case "Cooldown":
                        // Load reverse relationships (workouts using this exercise)
                        await StateService.LoadWorkoutLinksAsync();
                        await StateService.LoadAlternativeLinksAsync();
                        break;
                }
                
                ShowSuccessMessage($"Switched to {GetContextDisplayName(contextType)} context");
            }
        }

        /// <summary>
        /// Handle adding new exercise links - Check restrictions first
        /// </summary>
        private void HandleAddLink(string linkType)
        {
            // Check if this link type can be added in the current context
            var validationResult = ValidationService.CanAddLinkType(StateService.ActiveContext ?? "Workout", linkType);
            
            if (!validationResult.IsValid)
            {
                StateService.SetError(validationResult.ErrorMessage ?? $"Cannot add {linkType} links in {StateService.ActiveContext} context");
                return;
            }
            
            _addLinkType = linkType;
            _showAddModal = true;
        }

        /// <summary>
        /// Handle exercise selection from the modal
        /// </summary>
        private async Task HandleExerciseSelected(ExerciseListDto exercise)
        {
            _showAddModal = false;
            
            // Determine link type based on context and add type
            string linkTypeForValidation;
            string linkTypeForApi;
            
            if (_addLinkType == "Alternative")
            {
                linkTypeForValidation = "Alternative";
                linkTypeForApi = "ALTERNATIVE"; // API requires uppercase for ALTERNATIVE
            }
            else
            {
                linkTypeForValidation = _addLinkType;
                linkTypeForApi = _addLinkType; // Warmup/Cooldown work with title case
            }
            
            // Validate before creating the link
            var linkType = linkTypeForValidation switch
            {
                "Warmup" => ExerciseLinkType.Warmup,
                "Cooldown" => ExerciseLinkType.Cooldown,
                "Alternative" => ExerciseLinkType.Alternative,
                _ => throw new ArgumentException($"Invalid link type: {linkTypeForValidation}")
            };
            
            var validationResult = await ValidationService.ValidateCreateLink(
                Exercise, 
                exercise.Id, 
                linkType, 
                StateService.CurrentLinks?.Links ?? Enumerable.Empty<ExerciseLinkDto>());
            
            if (!validationResult.IsValid)
            {
                StateService.SetError(validationResult.ErrorMessage ?? "Validation failed");
                return;
            }
            
            var createDto = new CreateExerciseLinkDto
            {
                SourceExerciseId = Exercise.Id,
                TargetExerciseId = exercise.Id,
                LinkType = linkTypeForApi
            };

            // Set DisplayOrder for all link types
            if (_addLinkType == "Warmup" || _addLinkType == "Cooldown")
            {
                // Calculate the next display order based on existing links
                var existingLinks = StateService.CurrentLinks?.Links?
                    .Where(l => l.LinkType == _addLinkType)
                    .OrderBy(l => l.DisplayOrder)
                    .ToList() ?? new List<ExerciseLinkDto>();

                createDto.DisplayOrder = existingLinks.Any() 
                    ? existingLinks.Max(l => l.DisplayOrder) + 1 
                    : 1;
            }
            else
            {
                // Alternative links don't use display order, but API requires a value
                createDto.DisplayOrder = 0;
            }

            // Use bidirectional creation for alternative links
            if (_addLinkType == "Alternative")
            {
                await StateService.CreateBidirectionalLinkAsync(createDto);
            }
            else
            {
                await StateService.CreateLinkAsync(createDto);
            }
            
            if (string.IsNullOrEmpty(StateService.ErrorMessage))
            {
                var linkDescription = _addLinkType.ToLower();
                ShowSuccessMessage($"{exercise.Name} added to {linkDescription} exercises");
            }
        }

        /// <summary>
        /// Handle removing exercise links
        /// </summary>
        private async Task HandleRemoveLink(ExerciseLinkDto link)
        {
            _linkToDelete = link;
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Confirm link deletion
        /// </summary>
        private async Task ConfirmDelete()
        {
            if (_linkToDelete != null)
            {
                var exerciseName = _linkToDelete.TargetExercise?.Name ?? $"Exercise {_linkToDelete.TargetExerciseId}";
                
                // Use bidirectional deletion for alternative links
                if (_linkToDelete.LinkType == "Alternative")
                {
                    await StateService.DeleteBidirectionalLinkAsync(_linkToDelete.Id);
                }
                else
                {
                    await StateService.DeleteLinkAsync(_linkToDelete.Id);
                }
                
                if (string.IsNullOrEmpty(StateService.ErrorMessage))
                {
                    ShowSuccessMessage($"{exerciseName} removed from {_linkToDelete.LinkType.ToLower()} exercises");
                }
                
                _linkToDelete = null;
            }
        }

        /// <summary>
        /// Cancel link deletion
        /// </summary>
        private void CancelDelete()
        {
            _linkToDelete = null;
        }

        /// <summary>
        /// Handle exercise link reordering
        /// </summary>
        private async Task HandleReorderLinks((string linkType, Dictionary<string, int> reorderMap) args)
        {
            var updates = args.reorderMap.Select(kvp => new UpdateExerciseLinkDto
            {
                Id = kvp.Key,
                DisplayOrder = kvp.Value,
                IsActive = true
            }).ToList();

            await StateService.UpdateMultipleLinksAsync(updates);
            
            if (string.IsNullOrEmpty(StateService.ErrorMessage))
            {
                ShowSuccessMessage($"{args.linkType} exercises reordered");
            }
        }

        /// <summary>
        /// Handle viewing exercise details
        /// </summary>
        private void HandleViewExercise(ExerciseLinkDto link)
        {
            if (!string.IsNullOrEmpty(link.TargetExerciseId))
            {
                NavigationManager.NavigateTo($"/exercises/{link.TargetExerciseId}");
            }
        }

        /// <summary>
        /// Show success message with auto-dismiss
        /// </summary>
        private void ShowSuccessMessage(string message)
        {
            _successMessage = message;
            
            // Cancel any existing timer
            _successMessageTimer?.Dispose();
            
            // Set a new timer to clear the message after 3 seconds
            _successMessageTimer = new System.Threading.Timer(_ =>
            {
                _successMessage = null;
                InvokeAsync(StateHasChanged);
            }, null, 3000, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Get display name for context
        /// </summary>
        private string GetContextDisplayName(string context)
        {
            return context switch
            {
                "Workout" => "Workout Exercise",
                "Warmup" => "Warmup Exercise",
                "Cooldown" => "Cooldown Exercise",
                _ => $"{context} Exercise"
            };
        }

        /// <summary>
        /// Component disposal
        /// </summary>
        public void Dispose()
        {
            if (ShowComponent && !IsRestExercise)
            {
                StateService.OnChange -= HandleStateChange;
            }
            _successMessageTimer?.Dispose();
        }
    }
}