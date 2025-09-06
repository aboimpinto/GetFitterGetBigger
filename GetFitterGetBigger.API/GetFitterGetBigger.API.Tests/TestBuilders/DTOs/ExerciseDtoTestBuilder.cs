using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.DTOs;

/// <summary>
/// Test builder for creating ExerciseDto instances with sensible defaults.
/// Follows the Test Builder Pattern to create focused, maintainable test data.
/// </summary>
public class ExerciseDtoTestBuilder
{
    private string _id;
    private string _name = "Default Exercise";
    private string _description = "Default exercise description";
    private bool _isActive = true;
    private bool _isUnilateral = false;
    private List<ReferenceDataDto> _exerciseTypes = [];
    private List<MuscleGroupWithRoleDto> _muscleGroups = [];
    private List<ReferenceDataDto> _equipment = [];
    private List<ReferenceDataDto> _movementPatterns = [];
    private List<ReferenceDataDto> _bodyParts = [];
    private List<CoachNoteDto> _coachNotes = [];
    private ReferenceDataDto? _difficulty;
    private ReferenceDataDto? _kineticChain;
    private ReferenceDataDto? _exerciseWeightType;
    private string? _videoUrl;
    private string? _imageUrl;
    
    private ExerciseDtoTestBuilder()
    {
        _id = ExerciseId.New().ToString();
    }
    
    // Factory methods for common scenarios
    public static ExerciseDtoTestBuilder Default() => new();
    
    public static ExerciseDtoTestBuilder WarmupExercise()
    {
        return new ExerciseDtoTestBuilder()
            .WithName("Warmup Exercise")
            .WithDescription("Warmup exercise")
            .WithExerciseTypes(ExerciseTypeBuilder.Warmup())
            .WithDifficulty(DifficultyBuilder.Easy())
            .AsActive();
    }
    
    public static ExerciseDtoTestBuilder WorkoutExercise()
    {
        return new ExerciseDtoTestBuilder()
            .WithName("Workout Exercise")
            .WithDescription("Workout exercise")
            .WithExerciseTypes(ExerciseTypeBuilder.Workout())
            .WithDifficulty(DifficultyBuilder.Medium())
            .AsActive();
    }
    
    public static ExerciseDtoTestBuilder CooldownExercise()
    {
        return new ExerciseDtoTestBuilder()
            .WithName("Cooldown Exercise")
            .WithDescription("Cooldown exercise")
            .WithExerciseTypes(ExerciseTypeBuilder.Cooldown())
            .WithDifficulty(DifficultyBuilder.Easy())
            .AsActive();
    }
    
    public static ExerciseDtoTestBuilder RestExercise()
    {
        return new ExerciseDtoTestBuilder()
            .WithName("Rest Exercise")
            .WithDescription("Rest period")
            .WithExerciseTypes(ExerciseTypeBuilder.Rest())
            .AsActive();
    }
    
    public static ExerciseDtoTestBuilder AlternativeExercise()
    {
        return new ExerciseDtoTestBuilder()
            .WithName("Alternative Exercise")
            .WithDescription("Alternative workout exercise")
            .WithExerciseTypes(ExerciseTypeBuilder.Workout())
            .WithDifficulty(DifficultyBuilder.Medium())
            .AsActive();
    }
    
    // Fluent builder methods
    public ExerciseDtoTestBuilder WithId(ExerciseId id)
    {
        _id = id.ToString();
        return this;
    }
    
    public ExerciseDtoTestBuilder WithId(string id)
    {
        _id = id;
        return this;
    }
    
    public ExerciseDtoTestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public ExerciseDtoTestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }
    
    public ExerciseDtoTestBuilder AsActive()
    {
        _isActive = true;
        return this;
    }
    
    public ExerciseDtoTestBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }
    
    public ExerciseDtoTestBuilder AsUnilateral()
    {
        _isUnilateral = true;
        return this;
    }
    
    public ExerciseDtoTestBuilder AsBilateral()
    {
        _isUnilateral = false;
        return this;
    }
    
    public ExerciseDtoTestBuilder WithExerciseTypes(params ReferenceDataDto[] types)
    {
        _exerciseTypes = types.ToList();
        return this;
    }
    
    public ExerciseDtoTestBuilder WithExerciseTypes(List<ReferenceDataDto> types)
    {
        _exerciseTypes = types;
        return this;
    }
    
    public ExerciseDtoTestBuilder WithMuscleGroups(params MuscleGroupWithRoleDto[] groups)
    {
        _muscleGroups = groups.ToList();
        return this;
    }
    
    public ExerciseDtoTestBuilder WithEquipment(params ReferenceDataDto[] equipment)
    {
        _equipment = equipment.ToList();
        return this;
    }
    
    public ExerciseDtoTestBuilder WithMovementPatterns(params ReferenceDataDto[] patterns)
    {
        _movementPatterns = patterns.ToList();
        return this;
    }
    
    public ExerciseDtoTestBuilder WithBodyParts(params ReferenceDataDto[] bodyParts)
    {
        _bodyParts = bodyParts.ToList();
        return this;
    }
    
    public ExerciseDtoTestBuilder WithCoachNotes(params CoachNoteDto[] notes)
    {
        _coachNotes = notes.ToList();
        return this;
    }
    
    public ExerciseDtoTestBuilder WithDifficulty(ReferenceDataDto difficulty)
    {
        _difficulty = difficulty;
        return this;
    }
    
    public ExerciseDtoTestBuilder WithKineticChain(ReferenceDataDto kineticChain)
    {
        _kineticChain = kineticChain;
        return this;
    }
    
    public ExerciseDtoTestBuilder WithExerciseWeightType(ReferenceDataDto weightType)
    {
        _exerciseWeightType = weightType;
        return this;
    }
    
    public ExerciseDtoTestBuilder WithVideoUrl(string videoUrl)
    {
        _videoUrl = videoUrl;
        return this;
    }
    
    public ExerciseDtoTestBuilder WithImageUrl(string imageUrl)
    {
        _imageUrl = imageUrl;
        return this;
    }
    
    public ExerciseDto Build()
    {
        return new ExerciseDto
        {
            Id = _id,
            Name = _name,
            Description = _description,
            IsActive = _isActive,
            IsUnilateral = _isUnilateral,
            ExerciseTypes = _exerciseTypes,
            MuscleGroups = _muscleGroups,
            Equipment = _equipment,
            MovementPatterns = _movementPatterns,
            BodyParts = _bodyParts,
            CoachNotes = _coachNotes,
            Difficulty = _difficulty ?? DifficultyBuilder.Easy(),
            KineticChain = _kineticChain,
            ExerciseWeightType = _exerciseWeightType,
            VideoUrl = _videoUrl,
            ImageUrl = _imageUrl
        };
    }
    
    // Implicit conversion for convenience
    public static implicit operator ExerciseDto(ExerciseDtoTestBuilder builder) => builder.Build();
}

/// <summary>
/// Helper builders for common reference data
/// </summary>
public static class ExerciseTypeBuilder
{
    public static ReferenceDataDto Warmup() => new()
    {
        Id = "exercisetype-warmup",
        Value = "Warmup",
        Description = "Warmup exercise type"
    };
    
    public static ReferenceDataDto Workout() => new()
    {
        Id = "exercisetype-workout",
        Value = "Workout",
        Description = "Workout exercise type"
    };
    
    public static ReferenceDataDto Cooldown() => new()
    {
        Id = "exercisetype-cooldown",
        Value = "Cooldown",
        Description = "Cooldown exercise type"
    };
    
    public static ReferenceDataDto Rest() => new()
    {
        Id = "exercisetype-rest",
        Value = "Rest",
        Description = "Rest exercise type"
    };
}

public static class DifficultyBuilder
{
    public static ReferenceDataDto Easy() => new()
    {
        Id = "difficulty-easy",
        Value = "Easy",
        Description = "Easy difficulty"
    };
    
    public static ReferenceDataDto Medium() => new()
    {
        Id = "difficulty-medium",
        Value = "Medium",
        Description = "Medium difficulty"
    };
    
    public static ReferenceDataDto Hard() => new()
    {
        Id = "difficulty-hard",
        Value = "Hard",
        Description = "Hard difficulty"
    };
}