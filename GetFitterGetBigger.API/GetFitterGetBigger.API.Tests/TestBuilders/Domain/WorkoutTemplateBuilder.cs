using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

public class WorkoutTemplateBuilder
{
    private WorkoutTemplateId _id = WorkoutTemplateId.New();
    private string _name = "Test Workout Template";
    private string? _description = "Test workout template description";
    private WorkoutCategoryId _categoryId = WorkoutCategoryId.New();
    private DifficultyLevelId _difficultyId = DifficultyLevelId.New();
    private int _estimatedDurationMinutes = 60;
    private List<string> _tags = new List<string> { "test", "workout" };
    private bool _isPublic = true;
    private WorkoutStateId _workoutStateId = WorkoutStateId.New();
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;
    
    // Navigation properties
    private WorkoutCategory? _category = null;
    private DifficultyLevel? _difficulty = null;
    private WorkoutState? _workoutState = null;
    private List<WorkoutTemplateExercise> _exercises = new List<WorkoutTemplateExercise>();
    private List<WorkoutTemplateObjective> _objectives = new List<WorkoutTemplateObjective>();

    // Common presets
    public static WorkoutTemplateBuilder AWorkoutTemplate() => new WorkoutTemplateBuilder();
    
    public static WorkoutTemplateBuilder ADraftWorkoutTemplate() => new WorkoutTemplateBuilder()
        .WithName("Draft Workout Template")
        .WithDescription("A workout template in draft state")
        .WithWorkoutStateId(WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Draft))
        .WithCategoryId(WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.UpperBodyPush))
        .WithDifficultyId(DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate))
        .WithEstimatedDuration(45)
        .AsPublic();
    
    public static WorkoutTemplateBuilder AProductionWorkoutTemplate() => new WorkoutTemplateBuilder()
        .WithName("Production Workout Template")
        .WithDescription("A workout template in production state")
        .WithWorkoutStateId(WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Production))
        .WithCategoryId(WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.LowerBody))
        .WithDifficultyId(DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Advanced))
        .WithEstimatedDuration(60)
        .AsPublic();
    
    public static WorkoutTemplateBuilder AnArchivedWorkoutTemplate() => new WorkoutTemplateBuilder()
        .WithName("Archived Workout Template")
        .WithDescription("A workout template in archived state")
        .WithWorkoutStateId(WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Archived))
        .WithCategoryId(WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Core))
        .WithDifficultyId(DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner))
        .WithEstimatedDuration(30)
        .AsPrivate();
    
    public static WorkoutTemplateBuilder APrivateWorkoutTemplate() => new WorkoutTemplateBuilder()
        .WithName("Private Workout Template")
        .WithDescription("A private workout template")
        .AsPrivate();

    // ID management
    public WorkoutTemplateBuilder WithId(WorkoutTemplateId id)
    {
        _id = id;
        return this;
    }
    
    public WorkoutTemplateBuilder WithId(string id)
    {
        _id = WorkoutTemplateId.ParseOrEmpty(id);
        return this;
    }
    
    public WorkoutTemplateBuilder WithNewId()
    {
        _id = WorkoutTemplateId.New();
        return this;
    }

    // Basic properties
    public WorkoutTemplateBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public WorkoutTemplateBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }
    
    public WorkoutTemplateBuilder WithoutDescription()
    {
        _description = null;
        return this;
    }
    
    public WorkoutTemplateBuilder WithEstimatedDuration(int minutes)
    {
        _estimatedDurationMinutes = minutes;
        return this;
    }
    
    public WorkoutTemplateBuilder WithTags(params string[] tags)
    {
        _tags = new List<string>(tags);
        return this;
    }
    
    public WorkoutTemplateBuilder WithTag(string tag)
    {
        _tags.Add(tag);
        return this;
    }
    
    public WorkoutTemplateBuilder WithoutTags()
    {
        _tags = new List<string>();
        return this;
    }
    
    public WorkoutTemplateBuilder AsPublic()
    {
        _isPublic = true;
        return this;
    }
    
    public WorkoutTemplateBuilder AsPrivate()
    {
        _isPublic = false;
        return this;
    }

    // Required relationships
    public WorkoutTemplateBuilder WithCategoryId(WorkoutCategoryId categoryId)
    {
        _categoryId = categoryId;
        return this;
    }
    
    public WorkoutTemplateBuilder WithCategoryId(string categoryId)
    {
        _categoryId = WorkoutCategoryId.ParseOrEmpty(categoryId);
        return this;
    }
    
    public WorkoutTemplateBuilder WithCategory(WorkoutCategory category)
    {
        _category = category;
        _categoryId = category.WorkoutCategoryId;
        return this;
    }
    
    public WorkoutTemplateBuilder WithDifficultyId(DifficultyLevelId difficultyId)
    {
        _difficultyId = difficultyId;
        return this;
    }
    
    public WorkoutTemplateBuilder WithDifficultyId(string difficultyId)
    {
        _difficultyId = DifficultyLevelId.ParseOrEmpty(difficultyId);
        return this;
    }
    
    public WorkoutTemplateBuilder WithDifficulty(DifficultyLevel difficulty)
    {
        _difficulty = difficulty;
        _difficultyId = difficulty.DifficultyLevelId;
        return this;
    }
    
    
    public WorkoutTemplateBuilder WithWorkoutStateId(WorkoutStateId workoutStateId)
    {
        _workoutStateId = workoutStateId;
        return this;
    }
    
    public WorkoutTemplateBuilder WithWorkoutStateId(string workoutStateId)
    {
        _workoutStateId = WorkoutStateId.ParseOrEmpty(workoutStateId);
        return this;
    }
    
    public WorkoutTemplateBuilder WithWorkoutState(WorkoutState workoutState)
    {
        _workoutState = workoutState;
        _workoutStateId = workoutState.WorkoutStateId;
        return this;
    }

    // Timestamps
    public WorkoutTemplateBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }
    
    public WorkoutTemplateBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    // Collections
    public WorkoutTemplateBuilder WithExercise(WorkoutTemplateExercise exercise)
    {
        _exercises.Add(exercise);
        return this;
    }
    
    public WorkoutTemplateBuilder WithExercises(params WorkoutTemplateExercise[] exercises)
    {
        _exercises.AddRange(exercises);
        return this;
    }
    
    public WorkoutTemplateBuilder WithoutExercises()
    {
        _exercises.Clear();
        return this;
    }
    
    public WorkoutTemplateBuilder WithObjective(WorkoutTemplateObjective objective)
    {
        _objectives.Add(objective);
        return this;
    }
    
    public WorkoutTemplateBuilder WithObjectives(params WorkoutTemplateObjective[] objectives)
    {
        _objectives.AddRange(objectives);
        return this;
    }
    
    public WorkoutTemplateBuilder WithoutObjectives()
    {
        _objectives.Clear();
        return this;
    }

    // Build method
    public WorkoutTemplate Build()
    {
        var result = WorkoutTemplate.Handler.CreateNew(
            _name,
            _description,
            _categoryId,
            _difficultyId,
            _estimatedDurationMinutes,
            _tags,
            _isPublic,
            _workoutStateId);
        
        return result switch
        {
            { IsSuccess: true } => ApplyTestOverrides(result.Value! with { Id = _id }),
            _ => WorkoutTemplate.Empty
        };
    }
    
    private WorkoutTemplate ApplyTestOverrides(WorkoutTemplate template)
    {
        return (_createdAt != DateTime.MinValue || _updatedAt != DateTime.MinValue)
            ? template with { CreatedAt = _createdAt, UpdatedAt = _updatedAt }
            : template;
    }
    
    // Build with navigation properties (for when you need a fully loaded template)
    public WorkoutTemplate BuildWithNavigationProperties()
    {
        var template = Build();
        
        // Since WorkoutTemplate uses init-only properties, we need to create a new instance with all navigation properties
        // For now, tests should mock repository methods to return templates with navigation properties
        
        return template;
    }
}