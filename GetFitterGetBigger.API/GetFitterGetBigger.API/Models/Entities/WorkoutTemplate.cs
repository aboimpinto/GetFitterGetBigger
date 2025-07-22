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
    public UserId CreatedBy { get; init; }
    public WorkoutStateId WorkoutStateId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    
    // Navigation properties
    public WorkoutCategory Category { get; init; } = null!;
    public DifficultyLevel Difficulty { get; init; } = null!;
    public WorkoutState WorkoutState { get; init; } = null!;
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
        CreatedBy = UserId.Empty,
        WorkoutStateId = WorkoutStateId.Empty,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        // Initialize navigation properties to Empty instances
        Category = WorkoutCategory.Empty,
        Difficulty = DifficultyLevel.Empty,
        WorkoutState = WorkoutState.Empty
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
            UserId createdBy,
            WorkoutStateId workoutStateId)
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
                createdBy,
                workoutStateId,
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
            UserId createdBy,
            WorkoutStateId workoutStateId,
            DateTime createdAt,
            DateTime updatedAt)
        {
            // Validate tags
            var validatedTags = tags?.Where(t => !string.IsNullOrWhiteSpace(t))
                                     .Select(t => t.Trim())
                                     .Take(10) // Limit to 10 tags
                                     .ToList() ?? new List<string>();
            
            return Validate.For<WorkoutTemplate>()
                .EnsureNotWhiteSpace(name, "Name cannot be empty")
                .EnsureMinLength(name.Trim(), 3, "Name must be at least 3 characters long")
                .EnsureMaxLength(name.Trim(), 100, "Name cannot exceed 100 characters")
                .Ensure(() => description == null || description.Length <= 500, "Description cannot exceed 500 characters")
                .EnsureRange(estimatedDurationMinutes, 5, 300, "Estimated duration must be between 5 and 300 minutes")
                .Ensure(() => !categoryId.IsEmpty, "Category ID cannot be empty")
                .Ensure(() => !difficultyId.IsEmpty, "Difficulty ID cannot be empty")
                .Ensure(() => !createdBy.IsEmpty, "Created by user ID cannot be empty")
                .Ensure(() => !workoutStateId.IsEmpty, "Workout state ID cannot be empty")
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
                    CreatedBy = createdBy,
                    WorkoutStateId = workoutStateId,
                    CreatedAt = createdAt,
                    UpdatedAt = updatedAt
                });
        }
        
        public static EntityResult<WorkoutTemplate> Update(
            WorkoutTemplate template,
            string? name = null,
            string? description = null,
            WorkoutCategoryId? categoryId = null,
            DifficultyLevelId? difficultyId = null,
            int? estimatedDurationMinutes = null,
            List<string>? tags = null,
            bool? isPublic = null)
        {
            // Apply same validation rules as Create
            var newName = name ?? template.Name;
            var newDuration = estimatedDurationMinutes ?? template.EstimatedDurationMinutes;
            
            // Validate tags if provided
            var validatedTags = tags != null
                ? tags.Where(t => !string.IsNullOrWhiteSpace(t))
                      .Select(t => t.Trim())
                      .Take(10)
                      .ToList()
                : template.Tags;
            
            var validation = Validate.For<WorkoutTemplate>();
            
            // Only validate name if it's being changed
            if (!string.IsNullOrEmpty(name))
            {
                validation
                    .EnsureNotWhiteSpace(name, "Name cannot be empty")
                    .EnsureMinLength(name.Trim(), 3, "Name must be at least 3 characters long")
                    .EnsureMaxLength(name.Trim(), 100, "Name cannot exceed 100 characters");
            }
            
            // Only validate description if provided
            if (description != null)
            {
                validation.EnsureMaxLength(description, 500, "Description cannot exceed 500 characters");
            }
            
            // Only validate duration if being changed
            if (estimatedDurationMinutes.HasValue)
            {
                validation.EnsureRange(estimatedDurationMinutes.Value, 5, 300, "Estimated duration must be between 5 and 300 minutes");
            }
            
            return validation.OnSuccess(() => template with
            {
                Name = newName.Trim(),
                Description = description != null ? description.Trim() : template.Description,
                CategoryId = categoryId ?? template.CategoryId,
                DifficultyId = difficultyId ?? template.DifficultyId,
                EstimatedDurationMinutes = newDuration,
                Tags = validatedTags,
                IsPublic = isPublic ?? template.IsPublic,
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
    }
}