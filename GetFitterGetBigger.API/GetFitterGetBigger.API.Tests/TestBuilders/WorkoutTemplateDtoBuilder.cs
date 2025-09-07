using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Tests.TestHelpers;

namespace GetFitterGetBigger.API.Tests.TestBuilders;

public class WorkoutTemplateDtoBuilder
{
    private string _id = TestIds.WorkoutTemplateIds.BasicTemplate;
    private string _name = "Test Workout Template";
    private string? _description = "Test Description";
    private ReferenceDataDto _category = new()
    {
        Id = TestIds.WorkoutCategoryIds.Strength,
        Value = "Strength",
        Description = "Strength training"
    };
    private ReferenceDataDto _difficulty = new()
    {
        Id = TestIds.DifficultyLevelIds.Beginner,
        Value = "Beginner",
        Description = "Beginner level"
    };
    private int _estimatedDurationMinutes = 60;
    private List<string> _tags = new() { "test", "workout" };
    private bool _isPublic = true;
    private ReferenceDataDto _workoutState = new()
    {
        Id = TestIds.WorkoutStateIds.Draft,
        Value = "Draft",
        Description = "Draft state"
    };
    private ReferenceDataDto _executionProtocol = new()
    {
        Id = "executionprotocol-30000003-3000-4000-8000-300000000001",
        Value = "Reps and Sets",
        Description = "Traditional workout with fixed sets and repetitions"
    };
    private string? _executionProtocolConfig = null;
    private List<ReferenceDataDto> _objectives = new();
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;
    private List<WorkoutTemplateExerciseDto> _exercises = new();
    
    public WorkoutTemplateDtoBuilder WithId(string id)
    {
        _id = id;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithCategory(ReferenceDataDto category)
    {
        _category = category;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithDifficulty(ReferenceDataDto difficulty)
    {
        _difficulty = difficulty;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithEstimatedDurationMinutes(int minutes)
    {
        _estimatedDurationMinutes = minutes;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithTags(List<string> tags)
    {
        _tags = tags;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithIsPublic(bool isPublic)
    {
        _isPublic = isPublic;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithWorkoutState(ReferenceDataDto workoutState)
    {
        _workoutState = workoutState;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithWorkoutStateId(string workoutStateId)
    {
        _workoutState = new ReferenceDataDto
        {
            Id = workoutStateId,
            Value = "State",
            Description = "State"
        };
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithExecutionProtocol(ReferenceDataDto executionProtocol)
    {
        _executionProtocol = executionProtocol;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithExecutionProtocolId(string executionProtocolId)
    {
        _executionProtocol = new ReferenceDataDto
        {
            Id = executionProtocolId,
            Value = "Protocol",
            Description = "Protocol"
        };
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithExecutionProtocolConfig(string? config)
    {
        _executionProtocolConfig = config;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithObjectives(List<ReferenceDataDto> objectives)
    {
        _objectives = objectives;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }
    
    public WorkoutTemplateDtoBuilder WithExercises(List<WorkoutTemplateExerciseDto> exercises)
    {
        _exercises = exercises;
        return this;
    }
    
    public WorkoutTemplateDto Build()
    {
        return new WorkoutTemplateDto
        {
            Id = _id,
            Name = _name,
            Description = _description,
            Category = _category,
            Difficulty = _difficulty,
            EstimatedDurationMinutes = _estimatedDurationMinutes,
            Tags = _tags,
            IsPublic = _isPublic,
            WorkoutState = _workoutState,
            ExecutionProtocol = _executionProtocol,
            ExecutionProtocolConfig = _executionProtocolConfig,
            Objectives = _objectives,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            Exercises = _exercises
        };
    }
}