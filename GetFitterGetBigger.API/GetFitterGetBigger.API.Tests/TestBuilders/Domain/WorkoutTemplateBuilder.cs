using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating valid WorkoutTemplate entities with proper validation
/// </summary>
public class WorkoutTemplateBuilder
{
    private WorkoutTemplateId? _id = null;
    private string _name = "Test Workout Template";
    private string? _description = "Test workout template description";
    private WorkoutCategoryId _workoutCategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.UpperBodyPush);
    private WorkoutObjectiveId _workoutObjectiveId = WorkoutObjectiveId.ParseOrEmpty(TestIds.WorkoutObjectiveIds.MuscularStrength);
    private ExecutionProtocolId _executionProtocolId = ExecutionProtocolId.ParseOrEmpty(TestIds.ExecutionProtocolIds.Standard);
    private WorkoutStateId _workoutStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Draft);
    private int _estimatedDurationMinutes = 60;
    private string _difficultyLevel = "Intermediate";
    private List<string> _tags = new List<string> { "strength", "upper-body" };
    private bool _isPublic = false;
    private string _createdBy = "test-trainer-id";
    
    // Navigation properties
    private WorkoutState? _workoutState = null;
    private WorkoutCategory? _workoutCategory = null;
    private WorkoutObjective? _workoutObjective = null;
    private ExecutionProtocol? _executionProtocol = null;
    private List<WorkoutTemplateExercise> _exercises = new List<WorkoutTemplateExercise>();

    private WorkoutTemplateBuilder() { }

    /// <summary>
    /// Creates a builder with default values
    /// </summary>
    public static WorkoutTemplateBuilder Default() => new WorkoutTemplateBuilder();

    /// <summary>
    /// Creates a builder for an upper body push workout template
    /// </summary>
    public static WorkoutTemplateBuilder UpperBodyPush() => new WorkoutTemplateBuilder()
        .WithId(WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.UpperBodyPushTemplate))
        .WithName("Upper Body Push Workout")
        .WithDescription("Chest, shoulders, and triceps focused workout")
        .WithWorkoutCategoryId(WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.UpperBodyPush))
        .WithWorkoutObjectiveId(WorkoutObjectiveId.ParseOrEmpty(TestIds.WorkoutObjectiveIds.MuscularStrength))
        .WithEstimatedDuration(75)
        .WithDifficultyLevel("Intermediate");

    /// <summary>
    /// Creates a builder for a lower body workout template
    /// </summary>
    public static WorkoutTemplateBuilder LowerBody() => new WorkoutTemplateBuilder()
        .WithId(WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.LowerBodyTemplate))
        .WithName("Lower Body Strength")
        .WithDescription("Comprehensive leg workout targeting quads, hamstrings, and glutes")
        .WithWorkoutCategoryId(WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.LowerBody))
        .WithWorkoutObjectiveId(WorkoutObjectiveId.ParseOrEmpty(TestIds.WorkoutObjectiveIds.MuscularHypertrophy))
        .WithEstimatedDuration(90)
        .WithDifficultyLevel("Advanced");

    /// <summary>
    /// Creates a builder for a full body workout template
    /// </summary>
    public static WorkoutTemplateBuilder FullBody() => new WorkoutTemplateBuilder()
        .WithId(WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.FullBodyTemplate))
        .WithName("Full Body Circuit")
        .WithDescription("Complete body workout with compound movements")
        .WithWorkoutCategoryId(WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.LowerBody)) // No full body category in test data
        .WithWorkoutObjectiveId(WorkoutObjectiveId.ParseOrEmpty(TestIds.WorkoutObjectiveIds.MuscularEndurance))
        .WithExecutionProtocolId(ExecutionProtocolId.ParseOrEmpty(TestIds.ExecutionProtocolIds.AMRAP))
        .WithEstimatedDuration(45)
        .WithDifficultyLevel("Beginner");

    /// <summary>
    /// Creates a builder for a production-ready template
    /// </summary>
    public static WorkoutTemplateBuilder ProductionReady() => new WorkoutTemplateBuilder()
        .WithWorkoutStateId(WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Production))
        .AsPublic();

    /// <summary>
    /// Creates a builder for an archived template
    /// </summary>
    public static WorkoutTemplateBuilder Archived() => new WorkoutTemplateBuilder()
        .WithWorkoutStateId(WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Archived));

    public WorkoutTemplateBuilder WithId(WorkoutTemplateId id)
    {
        _id = id;
        return this;
    }

    public WorkoutTemplateBuilder WithId(string idString)
    {
        var id = WorkoutTemplateId.ParseOrEmpty(idString);
        if (id.IsEmpty)
        {
            throw new ArgumentException($"Invalid WorkoutTemplateId format: '{idString}'");
        }
        _id = id;
        return this;
    }

    public WorkoutTemplateBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 100)
        {
            throw new ArgumentException("Template name must be between 3 and 100 characters", nameof(name));
        }
        _name = name;
        return this;
    }

    public WorkoutTemplateBuilder WithDescription(string? description)
    {
        if (description != null && description.Length > 1000)
        {
            throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));
        }
        _description = description;
        return this;
    }

    public WorkoutTemplateBuilder WithWorkoutCategoryId(WorkoutCategoryId categoryId)
    {
        _workoutCategoryId = categoryId;
        return this;
    }

    public WorkoutTemplateBuilder WithWorkoutObjectiveId(WorkoutObjectiveId objectiveId)
    {
        _workoutObjectiveId = objectiveId;
        return this;
    }

    public WorkoutTemplateBuilder WithExecutionProtocolId(ExecutionProtocolId protocolId)
    {
        _executionProtocolId = protocolId;
        return this;
    }

    public WorkoutTemplateBuilder WithWorkoutStateId(WorkoutStateId stateId)
    {
        _workoutStateId = stateId;
        return this;
    }

    public WorkoutTemplateBuilder WithEstimatedDuration(int minutes)
    {
        if (minutes < 5 || minutes > 300)
        {
            throw new ArgumentException("Estimated duration must be between 5 and 300 minutes", nameof(minutes));
        }
        _estimatedDurationMinutes = minutes;
        return this;
    }

    public WorkoutTemplateBuilder WithDifficultyLevel(string level)
    {
        if (!new[] { "Beginner", "Intermediate", "Advanced" }.Contains(level))
        {
            throw new ArgumentException("Difficulty level must be Beginner, Intermediate, or Advanced", nameof(level));
        }
        _difficultyLevel = level;
        return this;
    }

    public WorkoutTemplateBuilder WithTags(params string[] tags)
    {
        _tags = tags.ToList();
        return this;
    }

    public WorkoutTemplateBuilder AsPublic(bool isPublic = true)
    {
        _isPublic = isPublic;
        return this;
    }

    public WorkoutTemplateBuilder AsPrivate() => AsPublic(false);

    public WorkoutTemplateBuilder WithCreatedBy(string trainerId)
    {
        _createdBy = trainerId;
        return this;
    }

    public WorkoutTemplateBuilder WithExercises(params WorkoutTemplateExercise[] exercises)
    {
        _exercises = exercises.ToList();
        return this;
    }

    public WorkoutTemplateBuilder WithWorkoutState(WorkoutState state)
    {
        _workoutState = state;
        _workoutStateId = state.WorkoutStateId;
        return this;
    }

    /// <summary>
    /// Builds a WorkoutTemplate entity with validation
    /// </summary>
    public WorkoutTemplate Build()
    {
        var result = _id.HasValue
            ? WorkoutTemplate.Handler.Create(
                id: _id.Value,
                name: _name,
                description: _description,
                workoutCategoryId: _workoutCategoryId,
                workoutObjectiveId: _workoutObjectiveId,
                executionProtocolId: _executionProtocolId,
                workoutStateId: _workoutStateId,
                estimatedDurationMinutes: _estimatedDurationMinutes,
                difficultyLevel: _difficultyLevel,
                tags: _tags,
                isPublic: _isPublic,
                createdBy: _createdBy)
            : WorkoutTemplate.Handler.CreateNew(
                name: _name,
                description: _description,
                workoutCategoryId: _workoutCategoryId,
                workoutObjectiveId: _workoutObjectiveId,
                executionProtocolId: _executionProtocolId,
                workoutStateId: _workoutStateId,
                estimatedDurationMinutes: _estimatedDurationMinutes,
                difficultyLevel: _difficultyLevel,
                tags: _tags,
                isPublic: _isPublic,
                createdBy: _createdBy);

        var template = result.Data;
        
        // Set navigation properties if provided
        if (_workoutState != null)
        {
            var workoutStateProperty = template.GetType().GetProperty("WorkoutState");
            workoutStateProperty?.SetValue(template, _workoutState);
        }
        
        if (_exercises.Any())
        {
            var exercisesProperty = template.GetType().GetProperty("Exercises");
            exercisesProperty?.SetValue(template, _exercises);
        }

        return template;
    }

    /// <summary>
    /// Builds and returns just the WorkoutTemplateId string for use in requests
    /// </summary>
    public string BuildId()
    {
        return Build().Id;
    }

    /// <summary>
    /// Implicit conversion to WorkoutTemplate for convenience
    /// </summary>
    public static implicit operator WorkoutTemplate(WorkoutTemplateBuilder builder)
    {
        return builder.Build();
    }
}