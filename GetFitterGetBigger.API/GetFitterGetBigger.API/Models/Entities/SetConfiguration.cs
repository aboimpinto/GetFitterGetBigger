using System;
using System.Text.RegularExpressions;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record SetConfiguration : IEmptyEntity<SetConfiguration>
{
    public SetConfigurationId Id { get; init; }
    
    // IEntity implementation
    string IEntity.Id => Id.ToString();
    bool IEntity.IsActive => true; // Always active
    
    public WorkoutTemplateExerciseId WorkoutTemplateExerciseId { get; init; }
    public int SetNumber { get; init; }
    public string? TargetReps { get; init; } // Can be a range like "8-12"
    public decimal? TargetWeight { get; init; }
    public int? TargetTimeSeconds { get; init; }
    public int RestSeconds { get; init; }
    
    // Navigation properties
    public WorkoutTemplateExercise? WorkoutTemplateExercise { get; init; }
    
    // Private constructor to force usage of Handler
    private SetConfiguration() { }
    
    /// <summary>
    /// Indicates if this is an empty/null object instance
    /// </summary>
    public bool IsEmpty => Id.IsEmpty;
    
    /// <summary>
    /// Static factory for creating an empty SetConfiguration instance
    /// </summary>
    public static SetConfiguration Empty => new() 
    { 
        Id = SetConfigurationId.Empty,
        WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.Empty,
        SetNumber = 0,
        RestSeconds = 0
    };
    
    public static class Handler
    {
        private static readonly Regex RepsRangeRegex = new(@"^\d+(-\d+)?$", RegexOptions.Compiled);
        
        public static EntityResult<SetConfiguration> CreateNew(
            WorkoutTemplateExerciseId workoutTemplateExerciseId,
            int setNumber,
            string? targetReps,
            decimal? targetWeight,
            int? targetTimeSeconds,
            int restSeconds)
        {
            return Create(
                SetConfigurationId.New(),
                workoutTemplateExerciseId,
                setNumber,
                targetReps,
                targetWeight,
                targetTimeSeconds,
                restSeconds
            );
        }
        
        public static EntityResult<SetConfiguration> Create(
            SetConfigurationId id,
            WorkoutTemplateExerciseId workoutTemplateExerciseId,
            int setNumber,
            string? targetReps,
            decimal? targetWeight,
            int? targetTimeSeconds,
            int restSeconds)
        {
            return Validate.For<SetConfiguration>()
                .Ensure(() => !workoutTemplateExerciseId.IsEmpty, "Workout template exercise ID cannot be empty")
                .EnsureMinValue(setNumber, 1, "Set number must be at least 1")
                .Ensure(() => string.IsNullOrEmpty(targetReps) || RepsRangeRegex.IsMatch(targetReps), 
                    "Target reps must be a number or range (e.g., '10' or '8-12')")
                .Ensure(() => !targetWeight.HasValue || targetWeight.Value >= 0, "Target weight cannot be negative")
                .Ensure(() => !targetTimeSeconds.HasValue || targetTimeSeconds.Value >= 0, "Target time cannot be negative")
                .EnsureRange(restSeconds, 0, 600, "Rest seconds must be between 0 and 600 (10 minutes)")
                .Ensure(() => !string.IsNullOrEmpty(targetReps) || targetWeight.HasValue || targetTimeSeconds.HasValue,
                    "At least one target metric (reps, weight, or time) must be specified")
                .OnSuccess(() => new SetConfiguration
                {
                    Id = id,
                    WorkoutTemplateExerciseId = workoutTemplateExerciseId,
                    SetNumber = setNumber,
                    TargetReps = targetReps,
                    TargetWeight = targetWeight,
                    TargetTimeSeconds = targetTimeSeconds,
                    RestSeconds = restSeconds
                });
        }
        
        public static EntityResult<SetConfiguration> Update(
            SetConfiguration configuration,
            string? targetReps = null,
            decimal? targetWeight = null,
            int? targetTimeSeconds = null,
            int? restSeconds = null)
        {
            var newTargetReps = targetReps ?? configuration.TargetReps;
            var newTargetWeight = targetWeight ?? configuration.TargetWeight;
            var newTargetTimeSeconds = targetTimeSeconds ?? configuration.TargetTimeSeconds;
            var newRestSeconds = restSeconds ?? configuration.RestSeconds;
            
            return Validate.For<SetConfiguration>()
                .Ensure(() => string.IsNullOrEmpty(newTargetReps) || RepsRangeRegex.IsMatch(newTargetReps), 
                    "Target reps must be a number or range (e.g., '10' or '8-12')")
                .Ensure(() => !newTargetWeight.HasValue || newTargetWeight.Value >= 0, "Target weight cannot be negative")
                .Ensure(() => !newTargetTimeSeconds.HasValue || newTargetTimeSeconds.Value >= 0, "Target time cannot be negative")
                .EnsureRange(newRestSeconds, 0, 600, "Rest seconds must be between 0 and 600 (10 minutes)")
                .Ensure(() => !string.IsNullOrEmpty(newTargetReps) || newTargetWeight.HasValue || newTargetTimeSeconds.HasValue,
                    "At least one target metric (reps, weight, or time) must be specified")
                .OnSuccess(() => configuration with
                {
                    TargetReps = newTargetReps,
                    TargetWeight = newTargetWeight,
                    TargetTimeSeconds = newTargetTimeSeconds,
                    RestSeconds = newRestSeconds
                });
        }
        
        /// <summary>
        /// Parses a reps range string into min and max values
        /// </summary>
        public static (int min, int max) ParseRepsRange(string repsRange)
        {
            if (string.IsNullOrEmpty(repsRange))
            {
                return (0, 0);
            }
            
            if (repsRange.Contains('-'))
            {
                var parts = repsRange.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out var min) && int.TryParse(parts[1], out var max))
                {
                    return (min, max);
                }
            }
            else if (int.TryParse(repsRange, out var singleValue))
            {
                return (singleValue, singleValue);
            }
            
            return (0, 0);
        }
    }
}