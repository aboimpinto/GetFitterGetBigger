using System;
using System.Collections.Generic;
using System.Linq;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record WorkoutTemplate : IEmptyEntity<WorkoutTemplate>
{
    public WorkoutTemplateId Id { get; init; }
    
    // IEntity implementation
    string IEntity.Id => Id.ToString();
    bool IEntity.IsActive => true; // Workout templates are always active, controlled by WorkoutState
    
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public WorkoutCategoryId CategoryId { get; init; }
    public DifficultyLevelId DifficultyId { get; init; }
    public int EstimatedDurationMinutes { get; init; }
    public List<string> Tags { get; init; } = new();
    public bool IsPublic { get; init; }
    public WorkoutStateId WorkoutStateId { get; init; }
    public ExecutionProtocolId ExecutionProtocolId { get; init; }
    public string? ExecutionProtocolConfig { get; init; } // JSON for protocol-specific settings
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    
    // Navigation properties
    public WorkoutCategory Category { get; init; } = null!;
    public DifficultyLevel Difficulty { get; init; } = null!;
    public WorkoutState WorkoutState { get; init; } = null!;
    public ExecutionProtocol ExecutionProtocol { get; init; } = null!;
    public ICollection<WorkoutTemplateExercise> Exercises { get; init; } = new List<WorkoutTemplateExercise>();
    public ICollection<WorkoutTemplateObjective> Objectives { get; init; } = new List<WorkoutTemplateObjective>();
    
    // Private constructor to force usage of Handler
    private WorkoutTemplate() { }
    
    /// <summary>
    /// Indicates if this is an empty/null object instance
    /// </summary>
    public bool IsEmpty => Id.IsEmpty;
    
    /// <summary>
    /// Static factory for creating an empty WorkoutTemplate instance
    /// </summary>
    public static WorkoutTemplate Empty => new() 
    { 
        Id = WorkoutTemplateId.Empty,
        Name = string.Empty,
        CategoryId = WorkoutCategoryId.Empty,
        DifficultyId = DifficultyLevelId.Empty,
        EstimatedDurationMinutes = 0,
        Tags = new List<string>(),
        IsPublic = false,
        WorkoutStateId = WorkoutStateId.Empty,
        ExecutionProtocolId = ExecutionProtocolId.Empty,
        ExecutionProtocolConfig = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        // Initialize navigation properties to Empty instances
        Category = WorkoutCategory.Empty,
        Difficulty = DifficultyLevel.Empty,
        WorkoutState = WorkoutState.Empty,
        ExecutionProtocol = ExecutionProtocol.Empty
    };
    
    public static class Handler
    {
        public static EntityResult<WorkoutTemplate> CreateNew(
            string name,
            string? description,
            WorkoutCategoryId categoryId,
            DifficultyLevelId difficultyId,
            int estimatedDurationMinutes,
            List<string>? tags,
            bool isPublic,
            WorkoutStateId workoutStateId,
            ExecutionProtocolId executionProtocolId,
            string? executionProtocolConfig = null)
        {
            return Create(
                WorkoutTemplateId.New(),
                name,
                description,
                categoryId,
                difficultyId,
                estimatedDurationMinutes,
                tags,
                isPublic,
                workoutStateId,
                executionProtocolId,
                executionProtocolConfig,
                DateTime.UtcNow,
                DateTime.UtcNow
            );
        }
        
        public static EntityResult<WorkoutTemplate> Create(
            WorkoutTemplateId id,
            string name,
            string? description,
            WorkoutCategoryId categoryId,
            DifficultyLevelId difficultyId,
            int estimatedDurationMinutes,
            List<string>? tags,
            bool isPublic,
            WorkoutStateId workoutStateId,
            ExecutionProtocolId executionProtocolId,
            string? executionProtocolConfig,
            DateTime createdAt,
            DateTime updatedAt)
        {
            var validatedTags = ValidateTags(tags);
            
            return ValidateWorkoutTemplate(name, description, categoryId, difficultyId, 
                estimatedDurationMinutes, workoutStateId, executionProtocolId)
                .OnSuccess(() => new WorkoutTemplate
                {
                    Id = id,
                    Name = name.Trim(),
                    Description = description?.Trim(),
                    CategoryId = categoryId,
                    DifficultyId = difficultyId,
                    EstimatedDurationMinutes = estimatedDurationMinutes,
                    Tags = validatedTags,
                    IsPublic = isPublic,
                    WorkoutStateId = workoutStateId,
                    ExecutionProtocolId = executionProtocolId,
                    ExecutionProtocolConfig = executionProtocolConfig?.Trim(),
                    CreatedAt = createdAt,
                    UpdatedAt = updatedAt
                });
        }
        
        /// <summary>
        /// Shared validation logic for WorkoutTemplate
        /// </summary>
        private static EntityValidation<WorkoutTemplate> ValidateWorkoutTemplate(
            string name,
            string? description,
            WorkoutCategoryId categoryId,
            DifficultyLevelId difficultyId,
            int estimatedDurationMinutes,
            WorkoutStateId workoutStateId,
            ExecutionProtocolId executionProtocolId)
        {
            return Validate.For<WorkoutTemplate>()
                .EnsureNotWhiteSpace(name, "Name cannot be empty")
                .EnsureMinLength(name.Trim(), 3, "Name must be at least 3 characters long")
                .EnsureMaxLength(name.Trim(), 100, "Name cannot exceed 100 characters")
                .Ensure(() => description == null || description.Length <= 500, "Description cannot exceed 500 characters")
                .EnsureRange(estimatedDurationMinutes, 5, 300, "Estimated duration must be between 5 and 300 minutes")
                .Ensure(() => !categoryId.IsEmpty, "Category ID cannot be empty")
                .Ensure(() => !difficultyId.IsEmpty, "Difficulty ID cannot be empty")
                .Ensure(() => !workoutStateId.IsEmpty, "Workout state ID cannot be empty")
                .Ensure(() => !executionProtocolId.IsEmpty, "Execution protocol ID cannot be empty");
        }
        
        /// <summary>
        /// Validates and normalizes tags
        /// </summary>
        private static List<string> ValidateTags(List<string>? tags)
        {
            return tags?.Where(t => !string.IsNullOrWhiteSpace(t))
                        .Select(t => t.Trim())
                        .Take(10) // Limit to 10 tags
                        .ToList() ?? new List<string>();
        }
        
        public static EntityResult<WorkoutTemplate> Update(
            WorkoutTemplate template,
            string? name = null,
            string? description = null,
            WorkoutCategoryId? categoryId = null,
            DifficultyLevelId? difficultyId = null,
            int? estimatedDurationMinutes = null,
            List<string>? tags = null,
            bool? isPublic = null,
            WorkoutStateId? workoutStateId = null,
            ExecutionProtocolId? executionProtocolId = null,
            string? executionProtocolConfig = null)
        {
            // Use current values if not provided
            var newName = name ?? template.Name;
            var newDescription = description ?? template.Description;
            var newCategoryId = categoryId ?? template.CategoryId;
            var newDifficultyId = difficultyId ?? template.DifficultyId;
            var newDuration = estimatedDurationMinutes ?? template.EstimatedDurationMinutes;
            var newIsPublic = isPublic ?? template.IsPublic;
            var newWorkoutStateId = workoutStateId ?? template.WorkoutStateId;
            var newExecutionProtocolId = executionProtocolId ?? template.ExecutionProtocolId;
            var newExecutionProtocolConfig = executionProtocolConfig ?? template.ExecutionProtocolConfig;
            
            // Validate and normalize tags
            var validatedTags = tags != null ? ValidateTags(tags) : template.Tags;
            
            // Use shared validation for all fields
            return ValidateWorkoutTemplate(newName, newDescription, newCategoryId, newDifficultyId, 
                newDuration, newWorkoutStateId, newExecutionProtocolId)
                .OnSuccess(() => template with
                {
                    Name = newName.Trim(),
                    Description = newDescription?.Trim(),
                    CategoryId = newCategoryId,
                    DifficultyId = newDifficultyId,
                    EstimatedDurationMinutes = newDuration,
                    Tags = validatedTags,
                    IsPublic = newIsPublic,
                    WorkoutStateId = newWorkoutStateId,
                    ExecutionProtocolId = newExecutionProtocolId,
                    ExecutionProtocolConfig = newExecutionProtocolConfig?.Trim(),
                    UpdatedAt = DateTime.UtcNow
                });
        }
        
        public static EntityResult<WorkoutTemplate> ChangeState(WorkoutTemplate template, WorkoutStateId newStateId)
        {
            return Validate.For<WorkoutTemplate>()
                .Ensure(() => !newStateId.IsEmpty, "New state ID cannot be empty")
                .OnSuccess(() => template with
                {
                    WorkoutStateId = newStateId,
                    UpdatedAt = DateTime.UtcNow
                });
        }
        
        public static EntityResult<WorkoutTemplate> Duplicate(
            WorkoutTemplate originalTemplate, 
            string newName)
        {
            return ValidateWorkoutTemplate(newName, originalTemplate.Description, originalTemplate.CategoryId, 
                originalTemplate.DifficultyId, originalTemplate.EstimatedDurationMinutes, originalTemplate.WorkoutStateId,
                originalTemplate.ExecutionProtocolId)
                .OnSuccess(() => new WorkoutTemplate
                {
                    Id = WorkoutTemplateId.New(),
                    Name = newName.Trim(),
                    Description = originalTemplate.Description,
                    CategoryId = originalTemplate.CategoryId,
                    DifficultyId = originalTemplate.DifficultyId,
                    EstimatedDurationMinutes = originalTemplate.EstimatedDurationMinutes,
                    Tags = originalTemplate.Tags.ToList(), // Create new list to avoid reference sharing
                    IsPublic = originalTemplate.IsPublic,
                    WorkoutStateId = originalTemplate.WorkoutStateId,
                    ExecutionProtocolId = originalTemplate.ExecutionProtocolId,
                    ExecutionProtocolConfig = originalTemplate.ExecutionProtocolConfig,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
        }
    }
}