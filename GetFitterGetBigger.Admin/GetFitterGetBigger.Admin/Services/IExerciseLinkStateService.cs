using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Results;

namespace GetFitterGetBigger.Admin.Services
{
    /// <summary>
    /// Service for managing the state of exercise links throughout the application
    /// Supports warmup, cooldown, and alternative relationships with context switching
    /// </summary>
    public interface IExerciseLinkStateService
    {
        /// <summary>
        /// Event fired when state changes
        /// </summary>
        event Action? OnChange;

        // Current exercise context
        /// <summary>
        /// The exercise ID for which links are being managed
        /// </summary>
        string? CurrentExerciseId { get; }

        /// <summary>
        /// The exercise name for display purposes
        /// </summary>
        string? CurrentExerciseName { get; }

        // Links state
        /// <summary>
        /// Current exercise links response containing warmup and cooldown links
        /// </summary>
        ExerciseLinksResponseDto? CurrentLinks { get; }

        /// <summary>
        /// Suggested links for the current exercise
        /// </summary>
        List<ExerciseLinkDto>? SuggestedLinks { get; }

        /// <summary>
        /// Whether links are currently being loaded
        /// </summary>
        bool IsLoadingLinks { get; }

        /// <summary>
        /// Whether suggested links are being loaded
        /// </summary>
        bool IsLoadingSuggestions { get; }

        /// <summary>
        /// Whether a link operation (create/update/delete) is in progress
        /// </summary>
        bool IsProcessingLink { get; }

        /// <summary>
        /// Whether any loading operation is in progress
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        /// Whether a save operation is in progress
        /// </summary>
        bool IsSaving { get; }

        /// <summary>
        /// Whether a delete operation is in progress
        /// </summary>
        bool IsDeleting { get; }

        /// <summary>
        /// Current error message if any
        /// </summary>
        string? ErrorMessage { get; }

        /// <summary>
        /// Success message for user feedback
        /// </summary>
        string? SuccessMessage { get; }

        /// <summary>
        /// Screen reader announcement for accessibility
        /// </summary>
        string? ScreenReaderAnnouncement { get; }

        // Filter/Options state
        /// <summary>
        /// Whether to include full exercise details when loading links
        /// </summary>
        bool IncludeExerciseDetails { get; set; }

        /// <summary>
        /// Current link type filter (null for all, "Warmup", "Cooldown", or "Alternative")
        /// </summary>
        string? LinkTypeFilter { get; set; }

        // Context management for multi-type exercises
        /// <summary>
        /// Active context for multi-type exercises ("Workout", "Warmup", or "Cooldown")
        /// </summary>
        string ActiveContext { get; set; }

        /// <summary>
        /// Whether the current exercise has multiple contexts (multi-type)
        /// </summary>
        bool HasMultipleContexts { get; }

        /// <summary>
        /// Available contexts for the current exercise
        /// </summary>
        IEnumerable<string> AvailableContexts { get; }

        // Computed properties
        /// <summary>
        /// Gets the warmup links from current links
        /// </summary>
        IEnumerable<ExerciseLinkDto> WarmupLinks { get; }

        /// <summary>
        /// Gets the cooldown links from current links
        /// </summary>
        IEnumerable<ExerciseLinkDto> CooldownLinks { get; }

        /// <summary>
        /// Gets the count of warmup links
        /// </summary>
        int WarmupLinkCount { get; }

        /// <summary>
        /// Gets the count of cooldown links
        /// </summary>
        int CooldownLinkCount { get; }

        /// <summary>
        /// Whether the maximum warmup links (10) has been reached
        /// </summary>
        bool HasMaxWarmupLinks { get; }

        /// <summary>
        /// Whether the maximum cooldown links (10) has been reached
        /// </summary>
        bool HasMaxCooldownLinks { get; }

        /// <summary>
        /// Gets the alternative links from current links
        /// </summary>
        IEnumerable<ExerciseLinkDto> AlternativeLinks { get; }

        /// <summary>
        /// Gets the count of alternative links
        /// </summary>
        int AlternativeLinkCount { get; }

        /// <summary>
        /// Gets the workout links (reverse relationships) - workouts using this as warmup/cooldown
        /// </summary>
        IEnumerable<ExerciseLinkDto> WorkoutLinks { get; }

        /// <summary>
        /// Gets the count of workout links (reverse relationships)
        /// </summary>
        int WorkoutLinkCount { get; }

        // Methods
        /// <summary>
        /// Initialize the state service for a specific exercise
        /// </summary>
        Task InitializeForExerciseAsync(string exerciseId, string exerciseName);

        /// <summary>
        /// Load links for the current exercise
        /// </summary>
        Task LoadLinksAsync();

        /// <summary>
        /// Load suggested links for the current exercise
        /// </summary>
        Task LoadSuggestedLinksAsync(int count = 5);

        /// <summary>
        /// Load alternative links for the current exercise
        /// </summary>
        Task LoadAlternativeLinksAsync();

        /// <summary>
        /// Load workout links (reverse relationships) - workouts using this exercise as warmup/cooldown
        /// </summary>
        Task LoadWorkoutLinksAsync();

        /// <summary>
        /// Create a new exercise link
        /// </summary>
        Task CreateLinkAsync(CreateExerciseLinkDto createDto);

        /// <summary>
        /// Create a bidirectional exercise link (for alternative relationships)
        /// </summary>
        Task CreateBidirectionalLinkAsync(CreateExerciseLinkDto createDto);

        /// <summary>
        /// Update an existing exercise link
        /// </summary>
        Task UpdateLinkAsync(string linkId, UpdateExerciseLinkDto updateDto);

        /// <summary>
        /// Delete an exercise link
        /// </summary>
        Task DeleteLinkAsync(string linkId);

        /// <summary>
        /// Delete a bidirectional exercise link (removes both directions)
        /// </summary>
        Task DeleteBidirectionalLinkAsync(string linkId);

        /// <summary>
        /// Reorder links by updating their display order
        /// </summary>
        Task ReorderLinksAsync(string linkType, Dictionary<string, int> linkIdToOrderMap);

        /// <summary>
        /// Update multiple links at once (for bulk operations)
        /// </summary>
        Task UpdateMultipleLinksAsync(List<UpdateExerciseLinkDto> updates);

        /// <summary>
        /// Switch the active context for multi-type exercises
        /// </summary>
        Task SwitchContextAsync(string contextType);

        /// <summary>
        /// Clear the current exercise context and all links
        /// </summary>
        void ClearExerciseContext();

        /// <summary>
        /// Clear any error message
        /// </summary>
        void ClearError();

        /// <summary>
        /// Clear any success message
        /// </summary>
        void ClearSuccess();

        /// <summary>
        /// Clear all messages (error and success)
        /// </summary>
        void ClearMessages();

        /// <summary>
        /// Set an error message
        /// </summary>
        void SetError(string errorMessage);

        /// <summary>
        /// Validate link compatibility for alternative exercises
        /// </summary>
        /// <param name="sourceExercise">Source exercise</param>
        /// <param name="targetExercise">Target exercise</param>
        /// <param name="linkType">Type of link being created</param>
        /// <returns>Service result indicating validation success/failure with message</returns>
        ServiceResult<bool> ValidateLinkCompatibility(ExerciseDto sourceExercise, ExerciseDto targetExercise, string linkType);
    }
}