using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.DTOs;

/// <summary>
/// Builder for creating WorkoutTemplateExerciseDto instances in tests
/// </summary>
public class WorkoutTemplateExerciseDtoTestBuilder
{
    private string _id = WorkoutTemplateExerciseId.New().ToString();
    private ExerciseDto _exercise = ExerciseDtoTestBuilder.Default().Build();
    private string _zone = "Main";
    private int _sequenceOrder = 1;
    private string? _notes = "Test notes";
    private List<SetConfigurationDto> _setConfigurations = new();
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public static WorkoutTemplateExerciseDtoTestBuilder Create() => new();

    public WorkoutTemplateExerciseDtoTestBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public WorkoutTemplateExerciseDtoTestBuilder WithExercise(ExerciseDto exercise)
    {
        _exercise = exercise;
        return this;
    }

    public WorkoutTemplateExerciseDtoTestBuilder WithZone(string zone)
    {
        _zone = zone;
        return this;
    }

    public WorkoutTemplateExerciseDtoTestBuilder WithSequenceOrder(int sequenceOrder)
    {
        _sequenceOrder = sequenceOrder;
        return this;
    }

    public WorkoutTemplateExerciseDtoTestBuilder WithNotes(string? notes)
    {
        _notes = notes;
        return this;
    }

    public WorkoutTemplateExerciseDtoTestBuilder WithSetConfigurations(List<SetConfigurationDto> setConfigurations)
    {
        _setConfigurations = setConfigurations;
        return this;
    }

    public WorkoutTemplateExerciseDtoTestBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public WorkoutTemplateExerciseDtoTestBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public WorkoutTemplateExerciseDto Build()
    {
        return new WorkoutTemplateExerciseDto
        {
            Id = _id,
            Exercise = _exercise,
            Zone = _zone,
            SequenceOrder = _sequenceOrder,
            Notes = _notes,
            SetConfigurations = _setConfigurations,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt
        };
    }
}

/// <summary>
/// Shorter alias for the builder
/// </summary>
public static class WorkoutTemplateExerciseDtoBuilder
{
    public static WorkoutTemplateExerciseDtoTestBuilder Create() => WorkoutTemplateExerciseDtoTestBuilder.Create();
}