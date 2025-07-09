using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services.Exceptions;

namespace GetFitterGetBigger.Admin.Services
{
    /// <summary>
    /// Implementation of the exercise link state management service
    /// </summary>
    public class ExerciseLinkStateService : IExerciseLinkStateService
    {
        private readonly IExerciseLinkService _exerciseLinkService;
        private const int MaxLinksPerType = 10;

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

        // Filter/Options state
        public bool IncludeExerciseDetails { get; set; } = true;
        public string? LinkTypeFilter { get; set; }

        // Computed properties
        public IEnumerable<ExerciseLinkDto> WarmupLinks => 
            CurrentLinks?.Links.Where(l => l.LinkType == "Warmup").OrderBy(l => l.DisplayOrder) ?? Enumerable.Empty<ExerciseLinkDto>();

        public IEnumerable<ExerciseLinkDto> CooldownLinks => 
            CurrentLinks?.Links.Where(l => l.LinkType == "Cooldown").OrderBy(l => l.DisplayOrder) ?? Enumerable.Empty<ExerciseLinkDto>();

        public int WarmupLinkCount => WarmupLinks.Count();
        public int CooldownLinkCount => CooldownLinks.Count();
        public bool HasMaxWarmupLinks => WarmupLinkCount >= MaxLinksPerType;
        public bool HasMaxCooldownLinks => CooldownLinkCount >= MaxLinksPerType;

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
            ClearMessages();
            
            await LoadLinksAsync();
        }

        public async Task LoadLinksAsync()
        {
            await LoadLinksAsync(preserveErrorMessage: false);
        }

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
            catch (ExerciseLinkApiException ex)
            {
                ErrorMessage = $"Failed to load links: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
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

        public async Task CreateLinkAsync(CreateExerciseLinkDto createDto)
        {
            if (string.IsNullOrEmpty(CurrentExerciseId))
            {
                ErrorMessage = "No exercise selected";
                NotifyStateChanged();
                return;
            }

            // Check max links before attempting to create
            if (createDto.LinkType == "Warmup" && HasMaxWarmupLinks)
            {
                ErrorMessage = $"Maximum {MaxLinksPerType} warmup links allowed";
                NotifyStateChanged();
                return;
            }
            
            if (createDto.LinkType == "Cooldown" && HasMaxCooldownLinks)
            {
                ErrorMessage = $"Maximum {MaxLinksPerType} cooldown links allowed";
                NotifyStateChanged();
                return;
            }

            try
            {
                IsProcessingLink = true;
                ClearMessages();
                NotifyStateChanged();

                // Optimistic update - add to local state immediately
                if (CurrentLinks != null)
                {
                    var optimisticLink = new ExerciseLinkDto
                    {
                        Id = Guid.NewGuid().ToString(), // Temporary ID
                        SourceExerciseId = CurrentExerciseId,
                        TargetExerciseId = createDto.TargetExerciseId,
                        LinkType = createDto.LinkType,
                        DisplayOrder = createDto.DisplayOrder,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    CurrentLinks.Links.Add(optimisticLink);
                    CurrentLinks.TotalCount++;
                    NotifyStateChanged();
                }

                await _exerciseLinkService.CreateLinkAsync(CurrentExerciseId, createDto);
                
                // Reload to get the actual server state
                await LoadLinksAsync();
                
                SuccessMessage = $"{createDto.LinkType} link created successfully";
            }
            catch (DuplicateExerciseLinkException)
            {
                ErrorMessage = "This exercise is already linked";
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
            }
            catch (MaximumLinksExceededException ex)
            {
                ErrorMessage = ex.Message;
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
            }
            catch (InvalidExerciseLinkException ex)
            {
                ErrorMessage = ex.Message;
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
            }
            catch (ExerciseLinkApiException ex)
            {
                ErrorMessage = $"Failed to create link: {ex.Message}";
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
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
                        link.IsActive = updateDto.IsActive;
                        link.UpdatedAt = DateTime.UtcNow;
                        NotifyStateChanged();
                    }
                }

                await _exerciseLinkService.UpdateLinkAsync(CurrentExerciseId, linkId, updateDto);
                
                // Reload to get the actual server state
                await LoadLinksAsync();
                
                SuccessMessage = "Link updated successfully";
            }
            catch (ExerciseLinkNotFoundException)
            {
                ErrorMessage = "Link not found";
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
            }
            catch (ExerciseLinkApiException ex)
            {
                ErrorMessage = $"Failed to update link: {ex.Message}";
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
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
            }
            catch (ExerciseLinkNotFoundException)
            {
                ErrorMessage = "Link not found";
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
            }
            catch (ExerciseLinkApiException ex)
            {
                ErrorMessage = $"Failed to delete link: {ex.Message}";
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                await LoadLinksAsync(preserveErrorMessage: true); // Revert optimistic update
            }
            finally
            {
                IsProcessingLink = false;
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
                            IsActive = link.IsActive
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
            CurrentLinks = null;
            SuggestedLinks = null;
            LinkTypeFilter = null;
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
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}