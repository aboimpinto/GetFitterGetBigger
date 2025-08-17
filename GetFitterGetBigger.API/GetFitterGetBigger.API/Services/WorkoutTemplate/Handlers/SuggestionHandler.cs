using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Exercise;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Constants.ErrorMessages;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Handlers;

/// <summary>
/// Handles exercise suggestions for workout templates
/// </summary>
public class SuggestionHandler
{
    private readonly IExerciseService _exerciseService;
    private readonly ILogger<SuggestionHandler> _logger;

    public SuggestionHandler(
        IExerciseService exerciseService,
        ILogger<SuggestionHandler> logger)
    {
        _exerciseService = exerciseService;
        _logger = logger;
    }

    /// <summary>
    /// Gets suggested exercises for a workout category
    /// </summary>
    public virtual async Task<ServiceResult<IEnumerable<ExerciseDto>>> GetSuggestedExercisesAsync(
        WorkoutCategoryId categoryId,
        IEnumerable<ExerciseId> existingExerciseIds,
        int maxSuggestions = 10)
    {
        return await ServiceValidate.Build<IEnumerable<ExerciseDto>>()
            .EnsureNotEmpty(categoryId, "CategoryId is required")
            .Ensure(() => maxSuggestions > 0, ServiceError.ValidationFailed(WorkoutTemplateErrorMessages.MaxSuggestionsRange))
            .MatchAsync(
                whenValid: async () => await LoadSuggestedExercisesAsync(categoryId, existingExerciseIds, maxSuggestions),
                whenInvalid: errors => ServiceResult<IEnumerable<ExerciseDto>>.Failure(
                    [], 
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }

    /// <summary>
    /// Gets exercise suggestions based on muscle groups
    /// </summary>
    public async Task<ServiceResult<IEnumerable<ExerciseDto>>> GetSuggestedByMuscleGroupsAsync(
        IEnumerable<MuscleGroupId> muscleGroupIds,
        IEnumerable<ExerciseId> existingExerciseIds,
        int maxSuggestions = 10)
    {
        var muscleGroups = muscleGroupIds.ToList();
        
        return await ServiceValidate.Build<IEnumerable<ExerciseDto>>()
            .Ensure(() => muscleGroups.Any(), ServiceError.ValidationFailed("At least one muscle group is required"))
            .Ensure(() => maxSuggestions > 0, ServiceError.ValidationFailed(WorkoutTemplateErrorMessages.MaxSuggestionsRange))
            .MatchAsync(
                whenValid: async () => await LoadSuggestedByMuscleGroupsAsync(muscleGroups, existingExerciseIds, maxSuggestions),
                whenInvalid: errors => ServiceResult<IEnumerable<ExerciseDto>>.Failure(
                    [], 
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }

    /// <summary>
    /// Gets complementary exercises to balance a workout
    /// </summary>
    public async Task<ServiceResult<IEnumerable<ExerciseDto>>> GetComplementaryExercisesAsync(
        IEnumerable<ExerciseId> currentExerciseIds,
        int maxSuggestions = 5)
    {
        var currentIds = currentExerciseIds.ToList();
        
        if (!currentIds.Any())
        {
            return ServiceResult<IEnumerable<ExerciseDto>>.Success([]);
        }

        // This is a simplified implementation
        // A more sophisticated version would:
        // - Analyze the muscle groups of current exercises
        // - Identify underworked muscle groups
        // - Suggest exercises for balance
        
        var command = new GetExercisesCommand(
            Page: 1,
            PageSize: 50,
            Name: string.Empty,
            SearchTerm: string.Empty,
            DifficultyLevelId: DifficultyLevelId.Empty,
            MuscleGroupIds: [],
            EquipmentIds: [],
            MovementPatternIds: [],
            BodyPartIds: [],
            IncludeInactive: false,
            IsActive: true);
            
        var exercisesResult = await _exerciseService.GetPagedAsync(command);
        
        var currentIdSet = currentIds.ToHashSet();
        var complementaryExercises = exercisesResult.Data.Items
            .Where(exercise => !currentIdSet.Contains(ExerciseId.ParseOrEmpty(exercise.Id)))
            .Take(maxSuggestions)
            .ToList();
        
        _logger.LogInformation(
            "Generated {Count} complementary exercise suggestions",
            complementaryExercises.Count);
        
        return complementaryExercises.Any()
            ? ServiceResult<IEnumerable<ExerciseDto>>.Success(complementaryExercises)
            : ServiceResult<IEnumerable<ExerciseDto>>.Failure(
                [],
                ServiceError.NotFound(WorkoutTemplateErrorMessages.NoSuggestedExercisesFound));
    }

    private async Task<ServiceResult<IEnumerable<ExerciseDto>>> LoadSuggestedExercisesAsync(
        WorkoutCategoryId categoryId,
        IEnumerable<ExerciseId> existingExerciseIds,
        int maxSuggestions)
    {
        // Use ExerciseService to get exercises
        // In a more sophisticated implementation, we would filter by category-specific criteria
        var command = new GetExercisesCommand(
            Page: 1,
            PageSize: 100,
            Name: string.Empty,
            SearchTerm: string.Empty,
            DifficultyLevelId: DifficultyLevelId.Empty,
            MuscleGroupIds: [],
            EquipmentIds: [],
            MovementPatternIds: [],
            BodyPartIds: [],
            IncludeInactive: false,
            IsActive: true);
            
        var exercisesResult = await _exerciseService.GetPagedAsync(command);
        
        var existingIdSet = existingExerciseIds.ToHashSet();
        
        // Filter out existing exercises and take the requested amount
        // Note: This is a basic implementation. A more sophisticated algorithm would:
        // - Consider muscle group distribution
        // - Match exercises to workout category characteristics
        // - Consider difficulty levels and progression
        var suggestedExercises = exercisesResult.Data.Items
            .Where(exercise => !existingIdSet.Contains(ExerciseId.ParseOrEmpty(exercise.Id)))
            .Take(maxSuggestions)
            .ToList();
        
        _logger.LogInformation(
            "Generated {Count} exercise suggestions for category {CategoryId}",
            suggestedExercises.Count, categoryId);
        
        return suggestedExercises.Any()
            ? ServiceResult<IEnumerable<ExerciseDto>>.Success(suggestedExercises)
            : ServiceResult<IEnumerable<ExerciseDto>>.Failure(
                [],
                ServiceError.NotFound(WorkoutTemplateErrorMessages.NoSuggestedExercisesFound));
    }

    private async Task<ServiceResult<IEnumerable<ExerciseDto>>> LoadSuggestedByMuscleGroupsAsync(
        IEnumerable<MuscleGroupId> muscleGroupIds,
        IEnumerable<ExerciseId> existingExerciseIds,
        int maxSuggestions)
    {
        // Get exercises filtered by muscle groups
        var command = new GetExercisesCommand(
            Page: 1,
            PageSize: 100,
            Name: string.Empty,
            SearchTerm: string.Empty,
            DifficultyLevelId: DifficultyLevelId.Empty,
            MuscleGroupIds: muscleGroupIds.ToList(),
            EquipmentIds: [],
            MovementPatternIds: [],
            BodyPartIds: [],
            IncludeInactive: false,
            IsActive: true);
            
        var exercisesResult = await _exerciseService.GetPagedAsync(command);
        
        var existingIdSet = existingExerciseIds.ToHashSet();
        
        var suggestedExercises = exercisesResult.Data.Items
            .Where(exercise => !existingIdSet.Contains(ExerciseId.ParseOrEmpty(exercise.Id)))
            .Take(maxSuggestions)
            .ToList();
        
        _logger.LogInformation(
            "Generated {Count} exercise suggestions for muscle groups",
            suggestedExercises.Count);
        
        return suggestedExercises.Any()
            ? ServiceResult<IEnumerable<ExerciseDto>>.Success(suggestedExercises)
            : ServiceResult<IEnumerable<ExerciseDto>>.Failure(
                [],
                ServiceError.NotFound(WorkoutTemplateErrorMessages.NoSuggestedExercisesFound));
    }
}