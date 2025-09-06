using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Errors;
using GetFitterGetBigger.Admin.Models.Results;
using GetFitterGetBigger.Admin.Services.Exceptions;

namespace GetFitterGetBigger.Admin.Services
{
    /// <summary>
    /// Implementation of the exercise link state management service
    /// Supports warmup, cooldown, and alternative relationships with context switching
    /// </summary>
    public class ExerciseLinkStateService : IExerciseLinkStateService
    {
        private readonly IExerciseLinkService _exerciseLinkService;

        public event Action? OnChange;

        // Current exercise context
        public string? CurrentExerciseId { get; private set; }
        public string? CurrentExerciseName { get; private set; }

        // Links state
        public ExerciseLinksResponseDto? CurrentLinks { get; private set; }
        public List<ExerciseLinkDto>? SuggestedLinks { get; private set; }
        public bool IsLoadingLinks { get; private set; }
        public bool IsLoadingSuggestions { get; private set; }
        public bool IsProcessingLink { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? SuccessMessage { get; private set; }
        public string? ScreenReaderAnnouncement { get; private set; }

        // Additional loading states
        public bool IsLoading => IsLoadingLinks || IsLoadingSuggestions;
        public bool IsSaving { get; private set; }
        public bool IsDeleting { get; private set; }

        // Filter/Options state
        public bool IncludeExerciseDetails { get; set; } = true;
        public string? LinkTypeFilter { get; set; }

        // Context management for multi-type exercises
        private string _activeContext = "Workout";
        private ExerciseDto? _currentExercise;
        public string ActiveContext 
        { 
            get => _activeContext;
            set
            {
                if (_activeContext != value)
                {
                    _activeContext = value;
                    NotifyStateChanged();
                }
            }
        }

        public bool HasMultipleContexts => _currentExercise?.ExerciseTypes?.Count() > 1;

        public IEnumerable<string> AvailableContexts
        {
            get
            {
                if (_currentExercise?.ExerciseTypes == null) return new List<string> { "Workout" };

                var contexts = new List<string>();
                var types = _currentExercise.ExerciseTypes.Select(t => t.Value);

                if (types.Contains("Workout")) contexts.Add("Workout");
                if (types.Contains("Warmup")) contexts.Add("Warmup");
                if (types.Contains("Cooldown")) contexts.Add("Cooldown");

                return contexts.Any() ? contexts : new List<string> { "Workout" };
            }
        }

        // Additional link collections
        private List<ExerciseLinkDto> _alternativeLinks = new();
        private List<ExerciseLinkDto> _workoutLinks = new();
#pragma warning disable CS0414 // Field is assigned but its value is never used - intended for future UI loading states
        private bool _isLoadingAlternatives;
        private bool _isLoadingWorkoutLinks;
#pragma warning restore CS0414

        // Computed properties
        public IEnumerable<ExerciseLinkDto> WarmupLinks =>
            CurrentLinks?.Links.Where(l => l.LinkType == "Warmup").OrderBy(l => l.DisplayOrder) ?? Enumerable.Empty<ExerciseLinkDto>();

        public IEnumerable<ExerciseLinkDto> CooldownLinks =>
            CurrentLinks?.Links.Where(l => l.LinkType == "Cooldown").OrderBy(l => l.DisplayOrder) ?? Enumerable.Empty<ExerciseLinkDto>();

        public int WarmupLinkCount => WarmupLinks.Count();
        public int CooldownLinkCount => CooldownLinks.Count();
        public bool HasMaxWarmupLinks => false; // No limits on links
        public bool HasMaxCooldownLinks => false; // No limits on links

        public IEnumerable<ExerciseLinkDto> AlternativeLinks => _alternativeLinks;
        public int AlternativeLinkCount => _alternativeLinks.Count;

        public IEnumerable<ExerciseLinkDto> WorkoutLinks => _workoutLinks;
        public int WorkoutLinkCount => _workoutLinks.Count;

        public ExerciseLinkStateService(IExerciseLinkService exerciseLinkService)
        {
            _exerciseLinkService = exerciseLinkService;
        }

        public async Task InitializeForExerciseAsync(string exerciseId, string exerciseName)
        {
            CurrentExerciseId = exerciseId;
            CurrentExerciseName = exerciseName;
            CurrentLinks = null;
            SuggestedLinks = null;
            _alternativeLinks.Clear();
            _workoutLinks.Clear();
            _activeContext = "Workout"; // Reset to default context
            ClearMessages();

            await LoadLinksAsync();
        }

        // New overload to initialize with exercise data for context detection
        public async Task InitializeForExerciseAsync(ExerciseDto exercise)
        {
            _currentExercise = exercise;
            CurrentExerciseId = exercise.Id;
            CurrentExerciseName = exercise.Name;
            CurrentLinks = null;
            SuggestedLinks = null;
            _alternativeLinks.Clear();
            _workoutLinks.Clear();
            
            // Set initial context based on exercise types
            var availableContexts = AvailableContexts.ToList();
            _activeContext = availableContexts.FirstOrDefault() ?? "Workout";
            
            ClearMessages();

            await LoadLinksAsync();
        }

        public async Task LoadLinksAsync()
        {
            await LoadLinksAsync(preserveErrorMessage: false);
        }

        /// <summary>
        /// Loads exercise links with optional error message preservation.
        /// When preserveErrorMessage is true, any existing error message will be retained
        /// after the load operation completes. This is crucial for maintaining user feedback
        /// when reverting optimistic updates after errors occur.
        /// </summary>
        /// <param name="preserveErrorMessage">Whether to preserve the current error message through the load operation</param>
        private async Task LoadLinksAsync(bool preserveErrorMessage)
        {
            if (string.IsNullOrEmpty(CurrentExerciseId))
            {
                ErrorMessage = "No exercise selected";
                NotifyStateChanged();
                return;
            }

            // Store existing error message if we need to preserve it
            var existingErrorMessage = preserveErrorMessage ? ErrorMessage : null;

            try
            {
                IsLoadingLinks = true;
                if (!preserveErrorMessage)
                {
                    ErrorMessage = null;
                }
                NotifyStateChanged();

                CurrentLinks = await _exerciseLinkService.GetLinksAsync(
                    CurrentExerciseId,
                    LinkTypeFilter,
                    IncludeExerciseDetails);
            }
            catch (ExerciseNotFoundException)
            {
                ErrorMessage = "Exercise not found";
            }
            catch (Exception ex)
            {
                ErrorMessage = ErrorMessageFormatter.FormatExerciseLinkError(ex);
            }
            finally
            {
                IsLoadingLinks = false;
                // Restore the error message if we were preserving it and no new error occurred
                if (preserveErrorMessage && string.IsNullOrEmpty(ErrorMessage) && !string.IsNullOrEmpty(existingErrorMessage))
                {
                    ErrorMessage = existingErrorMessage;
                }
                NotifyStateChanged();
            }
        }

        public async Task LoadSuggestedLinksAsync(int count = 5)
        {
            if (string.IsNullOrEmpty(CurrentExerciseId))
            {
                return;
            }

            try
            {
                IsLoadingSuggestions = true;
                NotifyStateChanged();

                SuggestedLinks = await _exerciseLinkService.GetSuggestedLinksAsync(CurrentExerciseId, count);
            }
            catch (Exception)
            {
                // Silently fail for suggestions - they're not critical
                SuggestedLinks = new List<ExerciseLinkDto>();
            }
            finally
            {
                IsLoadingSuggestions = false;
                NotifyStateChanged();
            }
        }

        public async Task LoadAlternativeLinksAsync()
        {
            if (string.IsNullOrEmpty(CurrentExerciseId))
            {
                return;
            }

            try
            {
                _isLoadingAlternatives = true;
                NotifyStateChanged();

                var alternativeLinks = await _exerciseLinkService.GetLinksAsync(
                    CurrentExerciseId, 
                    "Alternative", 
                    IncludeExerciseDetails,
                    includeReverse: true); // Include reverse alternative links
                    
                _alternativeLinks.Clear();
                if (alternativeLinks?.Links != null)
                {
                    _alternativeLinks.AddRange(alternativeLinks.Links);
                }
            }
            catch (Exception)
            {
                // Silently fail for alternative links loading
                _alternativeLinks.Clear();
            }
            finally
            {
                _isLoadingAlternatives = false;
                NotifyStateChanged();
            }
        }

        public async Task LoadWorkoutLinksAsync()
        {
            if (string.IsNullOrEmpty(CurrentExerciseId))
            {
                return;
            }

            try
            {
                _isLoadingWorkoutLinks = true;
                NotifyStateChanged();

                // Load reverse relationships - workouts using this exercise as warmup/cooldown
                // Use the new includeReverse parameter to get reverse relationships
                _workoutLinks.Clear();
                
                // For warmup context, get workouts that use this exercise as warmup
                // For cooldown context, get workouts that use this exercise as cooldown
                var linkTypeForReverse = _activeContext == "Warmup" ? "Warmup" : "Cooldown";
                var workoutLinks = await _exerciseLinkService.GetLinksAsync(
                    CurrentExerciseId, 
                    linkTypeForReverse, 
                    IncludeExerciseDetails,
                    includeReverse: true);
                    
                if (workoutLinks?.Links != null)
                {
                    // Add only the reverse relationships (where this exercise is the target)
                    var reverseLinks = workoutLinks.Links.Where(l => l.TargetExerciseId == CurrentExerciseId);
                    _workoutLinks.AddRange(reverseLinks);
                }
            }
            catch (Exception)
            {
                // Silently fail for workout links loading
                _workoutLinks.Clear();
            }
            finally
            {
                _isLoadingWorkoutLinks = false;
                NotifyStateChanged();
            }
        }

        public async Task CreateLinkAsync(CreateExerciseLinkDto createDto)
        {
            Console.WriteLine($"[ExerciseLinkStateService] CreateLinkAsync called");
            Console.WriteLine($"[ExerciseLinkStateService] CurrentExerciseId: {CurrentExerciseId}");
            Console.WriteLine($"[ExerciseLinkStateService] CreateDto: SourceExerciseId={createDto.SourceExerciseId}, TargetExerciseId={createDto.TargetExerciseId}, LinkType={createDto.LinkType}, DisplayOrder={createDto.DisplayOrder}");

            if (string.IsNullOrEmpty(CurrentExerciseId))
            {
                Console.WriteLine($"[ExerciseLinkStateService] Error: No exercise selected");
                ErrorMessage = "No exercise selected";
                NotifyStateChanged();
                return;
            }

            // No limits on the number of links - removed check

            try
            {
                Console.WriteLine($"[ExerciseLinkStateService] Starting link creation process");
                IsProcessingLink = true;
                ClearMessages();
                NotifyStateChanged();

                // Optimistic update - add to appropriate collection
                var optimisticLink = new ExerciseLinkDto
                {
                    Id = Guid.NewGuid().ToString(), // Temporary ID
                    SourceExerciseId = CurrentExerciseId,
                    TargetExerciseId = createDto.TargetExerciseId,
                    LinkType = createDto.LinkType,
                    DisplayOrder = createDto.DisplayOrder ?? 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                Console.WriteLine($"[ExerciseLinkStateService] Adding optimistic link with ID: {optimisticLink.Id}");

                if (createDto.LinkType == "Alternative")
                {
                    _alternativeLinks.Add(optimisticLink);
                }
                else if (CurrentLinks != null)
                {
                    CurrentLinks.Links.Add(optimisticLink);
                    CurrentLinks.TotalCount++;
                }
                
                NotifyStateChanged();

                Console.WriteLine($"[ExerciseLinkStateService] Calling ExerciseLinkService.CreateLinkAsync");
                await _exerciseLinkService.CreateLinkAsync(CurrentExerciseId, createDto);

                Console.WriteLine($"[ExerciseLinkStateService] Link created successfully, reloading links");
                
                // Reload appropriate links based on type
                if (createDto.LinkType == "Alternative")
                {
                    await LoadAlternativeLinksAsync();
                }
                else
                {
                    await LoadLinksAsync();
                }

                SuccessMessage = $"{createDto.LinkType} link created successfully";
                ScreenReaderAnnouncement = $"{createDto.LinkType} exercise link has been added successfully";
                Console.WriteLine($"[ExerciseLinkStateService] Link creation completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ExerciseLinkStateService] Exception in CreateLinkAsync: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ExerciseLinkStateService] Inner Exception: {ex.InnerException.Message}");
                }
                ErrorMessage = ErrorMessageFormatter.FormatExerciseLinkError(ex);
                Console.WriteLine($"[ExerciseLinkStateService] Formatted error message: {ErrorMessage}");
                
                // Revert optimistic update based on link type
                if (createDto.LinkType == "Alternative")
                {
                    await LoadAlternativeLinksAsync();
                }
                else
                {
                    await LoadLinksAsync(preserveErrorMessage: true);
                }
            }
            finally
            {
                Console.WriteLine($"[ExerciseLinkStateService] CreateLinkAsync cleanup");
                IsProcessingLink = false;
                NotifyStateChanged();
            }
        }

        public async Task CreateBidirectionalLinkAsync(CreateExerciseLinkDto createDto)
        {
            if (string.IsNullOrEmpty(CurrentExerciseId))
            {
                ErrorMessage = "No exercise selected";
                NotifyStateChanged();
                return;
            }

            // Bidirectional links are typically for Alternative type relationships
            if (createDto.LinkType != "Alternative")
            {
                ErrorMessage = "Bidirectional links are only supported for Alternative relationships";
                NotifyStateChanged();
                return;
            }

            try
            {
                IsProcessingLink = true;
                ClearMessages();
                NotifyStateChanged();

                // Optimistic update - add bidirectional links
                var forwardLink = new ExerciseLinkDto
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceExerciseId = CurrentExerciseId,
                    TargetExerciseId = createDto.TargetExerciseId,
                    LinkType = createDto.LinkType,
                    DisplayOrder = 0, // Alternative links don't use display order
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var reverseLink = new ExerciseLinkDto
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceExerciseId = createDto.TargetExerciseId,
                    TargetExerciseId = CurrentExerciseId,
                    LinkType = createDto.LinkType,
                    DisplayOrder = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _alternativeLinks.Add(forwardLink);
                NotifyStateChanged();

                // API call - use bidirectional method to create both directions
                await _exerciseLinkService.CreateBidirectionalLinkAsync(CurrentExerciseId, createDto);

                // Reload to get actual server state
                await LoadAlternativeLinksAsync();

                SuccessMessage = "Alternative link created (bidirectional)";
                ScreenReaderAnnouncement = "Alternative exercise link has been created for both exercises";
            }
            catch (Exception ex)
            {
                ErrorMessage = ErrorMessageFormatter.FormatExerciseLinkError(ex);
                await LoadAlternativeLinksAsync(); // Revert optimistic update
            }
            finally
            {
                IsProcessingLink = false;
                NotifyStateChanged();
            }
        }

        public async Task UpdateLinkAsync(string linkId, UpdateExerciseLinkDto updateDto)
        {
            if (string.IsNullOrEmpty(CurrentExerciseId))
            {
                ErrorMessage = "No exercise selected";
                NotifyStateChanged();
                return;
            }

            try
            {
                IsProcessingLink = true;
                ClearMessages();
                NotifyStateChanged();

                // Optimistic update - update local state immediately
                if (CurrentLinks != null)
                {
                    var link = CurrentLinks.Links.FirstOrDefault(l => l.Id == linkId);
                    if (link != null)
                    {
                        link.DisplayOrder = updateDto.DisplayOrder;
                        link.IsActive = true; // Always ON - no soft delete
                        link.UpdatedAt = DateTime.UtcNow;
                        NotifyStateChanged();
                    }
                }

                await _exerciseLinkService.UpdateLinkAsync(CurrentExerciseId, linkId, updateDto);

                // Reload to get the actual server state
                await LoadLinksAsync();

                SuccessMessage = "Link updated successfully";
                ScreenReaderAnnouncement = "Exercise link has been updated successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = ErrorMessageFormatter.FormatExerciseLinkError(ex);
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
            }
            finally
            {
                IsProcessingLink = false;
                NotifyStateChanged();
            }
        }

        public async Task DeleteLinkAsync(string linkId)
        {
            if (string.IsNullOrEmpty(CurrentExerciseId))
            {
                ErrorMessage = "No exercise selected";
                NotifyStateChanged();
                return;
            }

            try
            {
                IsProcessingLink = true;
                IsDeleting = true;
                ClearMessages();
                NotifyStateChanged();

                // Find the link type for success message
                var linkToDelete = CurrentLinks?.Links.FirstOrDefault(l => l.Id == linkId);
                var linkType = linkToDelete?.LinkType ?? "Link";

                // Optimistic update - remove from local state immediately
                if (CurrentLinks != null && linkToDelete != null)
                {
                    CurrentLinks.Links.Remove(linkToDelete);
                    CurrentLinks.TotalCount--;
                    NotifyStateChanged();
                }

                await _exerciseLinkService.DeleteLinkAsync(CurrentExerciseId, linkId);

                // Reload to get the actual server state
                await LoadLinksAsync();

                SuccessMessage = $"{linkType} link removed successfully";
                ScreenReaderAnnouncement = $"{linkType} exercise link has been removed successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = ErrorMessageFormatter.FormatExerciseLinkError(ex);
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
            }
            finally
            {
                IsProcessingLink = false;
                IsDeleting = false;
                NotifyStateChanged();
            }
        }

        public async Task DeleteBidirectionalLinkAsync(string linkId)
        {
            if (string.IsNullOrEmpty(CurrentExerciseId))
            {
                ErrorMessage = "No exercise selected";
                NotifyStateChanged();
                return;
            }

            try
            {
                IsProcessingLink = true;
                IsDeleting = true;
                ClearMessages();
                NotifyStateChanged();

                // Find the link to delete from alternative links
                var linkToDelete = _alternativeLinks.FirstOrDefault(l => l.Id == linkId);
                var linkType = linkToDelete?.LinkType ?? "Link";

                // Optimistic update - remove from local state immediately
                if (linkToDelete != null)
                {
                    _alternativeLinks.Remove(linkToDelete);
                    NotifyStateChanged();
                }

                // API call - use bidirectional method to delete both directions
                await _exerciseLinkService.DeleteBidirectionalLinkAsync(CurrentExerciseId, linkId, true);

                // Reload to get the actual server state
                await LoadAlternativeLinksAsync();

                SuccessMessage = $"{linkType} link removed (bidirectional)";
                ScreenReaderAnnouncement = $"{linkType} exercise link has been removed from both exercises";
            }
            catch (Exception ex)
            {
                ErrorMessage = ErrorMessageFormatter.FormatExerciseLinkError(ex);
                await LoadAlternativeLinksAsync(); // Revert optimistic update
            }
            finally
            {
                IsProcessingLink = false;
                IsDeleting = false;
                NotifyStateChanged();
            }
        }

        public async Task SwitchContextAsync(string contextType)
        {
            if (!AvailableContexts.Contains(contextType))
            {
                ErrorMessage = $"Invalid context type: {contextType}";
                NotifyStateChanged();
                return;
            }

            try
            {
                ClearMessages();
                ActiveContext = contextType;

                // Load appropriate links for the new context
                switch (contextType)
                {
                    case "Workout":
                        await LoadLinksAsync();
                        await LoadAlternativeLinksAsync();
                        break;
                    case "Warmup":
                    case "Cooldown":
                        await LoadWorkoutLinksAsync(); // Load reverse relationships
                        await LoadAlternativeLinksAsync();
                        break;
                }

                ScreenReaderAnnouncement = $"Switched to {contextType} context";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to switch context: {ex.Message}";
                NotifyStateChanged();
            }
        }

        public async Task UpdateMultipleLinksAsync(List<UpdateExerciseLinkDto> updates)
        {
            if (string.IsNullOrEmpty(CurrentExerciseId))
            {
                ErrorMessage = "No exercise selected";
                NotifyStateChanged();
                return;
            }

            try
            {
                IsProcessingLink = true;
                IsSaving = true;
                ClearMessages();
                NotifyStateChanged();

                var updateTasks = updates.Select(update =>
                    _exerciseLinkService.UpdateLinkAsync(CurrentExerciseId, update.Id, update)
                ).ToList();

                await Task.WhenAll(updateTasks);

                // Reload to ensure consistency
                await LoadLinksAsync();

                SuccessMessage = "Links updated successfully";
                ScreenReaderAnnouncement = "Exercise links have been updated successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = ErrorMessageFormatter.FormatExerciseLinkError(ex);
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
            }
            finally
            {
                IsProcessingLink = false;
                IsSaving = false;
                NotifyStateChanged();
            }
        }

        public async Task ReorderLinksAsync(string linkType, Dictionary<string, int> linkIdToOrderMap)
        {
            if (string.IsNullOrEmpty(CurrentExerciseId) || CurrentLinks == null)
            {
                return;
            }

            try
            {
                IsProcessingLink = true;
                ClearMessages();
                NotifyStateChanged();

                // Create update tasks for all links that need reordering
                var updateTasks = new List<Task>();

                foreach (var kvp in linkIdToOrderMap)
                {
                    var linkId = kvp.Key;
                    var newOrder = kvp.Value;

                    var link = CurrentLinks.Links.FirstOrDefault(l => l.Id == linkId && l.LinkType == linkType);
                    if (link != null && link.DisplayOrder != newOrder)
                    {
                        // Optimistic update
                        link.DisplayOrder = newOrder;

                        var updateDto = new UpdateExerciseLinkDto
                        {
                            DisplayOrder = newOrder,
                            IsActive = true // Always ON - no soft delete
                        };

                        updateTasks.Add(_exerciseLinkService.UpdateLinkAsync(CurrentExerciseId, linkId, updateDto));
                    }
                }

                if (updateTasks.Any())
                {
                    NotifyStateChanged();
                    await Task.WhenAll(updateTasks);

                    // Reload to ensure consistency
                    await LoadLinksAsync();

                    SuccessMessage = $"{linkType} links reordered successfully";
                    ScreenReaderAnnouncement = $"{linkType} exercise links have been reordered successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to reorder links: {ex.Message}";
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
            }
            finally
            {
                IsProcessingLink = false;
                NotifyStateChanged();
            }
        }

        public void ClearExerciseContext()
        {
            CurrentExerciseId = null;
            CurrentExerciseName = null;
            _currentExercise = null;
            CurrentLinks = null;
            SuggestedLinks = null;
            _alternativeLinks.Clear();
            _workoutLinks.Clear();
            LinkTypeFilter = null;
            _activeContext = "Workout";
            ClearMessages();
            NotifyStateChanged();
        }

        public void ClearError()
        {
            ErrorMessage = null;
            NotifyStateChanged();
        }

        public void ClearSuccess()
        {
            SuccessMessage = null;
            NotifyStateChanged();
        }

        public void ClearMessages()
        {
            ErrorMessage = null;
            SuccessMessage = null;
            ScreenReaderAnnouncement = null;
            NotifyStateChanged();
        }

        public void SetError(string errorMessage)
        {
            ErrorMessage = errorMessage;
            SuccessMessage = null;
            ScreenReaderAnnouncement = $"Error: {errorMessage}";
            NotifyStateChanged();
        }

        public ServiceResult<bool> ValidateLinkCompatibility(ExerciseDto sourceExercise, ExerciseDto targetExercise, string linkType)
        {
            // Rule: Cannot self-reference
            if (sourceExercise.Id == targetExercise.Id)
            {
                return ServiceResult<bool>.Failure(
                    new ServiceError(ServiceErrorCode.ValidationInvalid, "An exercise cannot be linked to itself"));
            }

            // Rule for Alternative links: Must share at least one exercise type
            if (linkType == "Alternative")
            {
                var sourceTypes = sourceExercise.ExerciseTypes?.Select(t => t.Value) ?? Enumerable.Empty<string>();
                var targetTypes = targetExercise.ExerciseTypes?.Select(t => t.Value) ?? Enumerable.Empty<string>();

                if (!sourceTypes.Intersect(targetTypes).Any())
                {
                    return ServiceResult<bool>.Failure(
                        new ServiceError(ServiceErrorCode.ValidationInvalid, 
                            "Alternative exercises must share at least one exercise type"));
                }
            }

            // Rule: Check for existing link
            bool hasExistingLink = false;
            if (linkType == "Alternative")
            {
                hasExistingLink = _alternativeLinks.Any(l => l.TargetExerciseId == targetExercise.Id);
            }
            else if (CurrentLinks != null)
            {
                hasExistingLink = CurrentLinks.Links.Any(l => 
                    l.TargetExerciseId == targetExercise.Id && l.LinkType == linkType);
            }

            if (hasExistingLink)
            {
                return ServiceResult<bool>.Failure(
                    new ServiceError(ServiceErrorCode.DuplicateName, 
                        $"A {linkType.ToLower()} link to this exercise already exists"));
            }

            return ServiceResult<bool>.Success(true);
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}