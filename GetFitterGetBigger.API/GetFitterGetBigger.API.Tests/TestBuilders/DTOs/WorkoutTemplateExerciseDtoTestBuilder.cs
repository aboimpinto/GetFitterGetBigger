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
    private string _phase = "Workout";
    private int _roundNumber = 1;
    private int _orderInRound = 1;
    private string _metadata = "{}";
    private string? _notes = "Test notes";
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

    public WorkoutTemplateExerciseDtoTestBuilder WithPhase(string phase)
    {
        _phase = phase;
        return this;
    }

    public WorkoutTemplateExerciseDtoTestBuilder WithRoundNumber(int roundNumber)
    {
        _roundNumber = roundNumber;
        return this;
    }

    public WorkoutTemplateExerciseDtoTestBuilder WithOrderInRound(int orderInRound)
    {
        _orderInRound = orderInRound;
        return this;
    }

    public WorkoutTemplateExerciseDtoTestBuilder WithMetadata(string metadata)
    {
        _metadata = metadata;
        return this;
    }

    public WorkoutTemplateExerciseDtoTestBuilder WithNotes(string? notes)
    {
        _notes = notes;
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
            Phase = _phase,
            RoundNumber = _roundNumber,
            OrderInRound = _orderInRound,
            Metadata = _metadata,
            Notes = _notes,
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