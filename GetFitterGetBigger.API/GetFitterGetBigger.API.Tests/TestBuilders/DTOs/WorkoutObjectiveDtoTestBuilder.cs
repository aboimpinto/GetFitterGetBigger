using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Tests.TestBuilders;

namespace GetFitterGetBigger.API.Tests.TestBuilders.DTOs;

/// <summary>
/// Test builder for WorkoutObjectiveDto
/// </summary>
public class WorkoutObjectiveDtoTestBuilder
{
    private string _workoutObjectiveId = TestIds.WorkoutObjectiveIds.MuscularStrength;
    private string _value = "Test Objective";
    private string? _description = "Test Description";
    private int _displayOrder = 1;
    private bool _isActive = true;

    public WorkoutObjectiveDtoTestBuilder WithWorkoutObjectiveId(string workoutObjectiveId)
    {
        _workoutObjectiveId = workoutObjectiveId;
        return this;
    }

    public WorkoutObjectiveDtoTestBuilder WithValue(string value)
    {
        _value = value;
        return this;
    }

    public WorkoutObjectiveDtoTestBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public WorkoutObjectiveDtoTestBuilder WithDisplayOrder(int displayOrder)
    {
        _displayOrder = displayOrder;
        return this;
    }

    public WorkoutObjectiveDtoTestBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public WorkoutObjectiveDtoTestBuilder WithMuscularStrength()
    {
        _workoutObjectiveId = TestIds.WorkoutObjectiveIds.MuscularStrength;
        _value = "Muscular Strength";
        _description = "Build maximum strength through heavy loads and low repetitions";
        _displayOrder = 1;
        _isActive = true;
        return this;
    }

    public WorkoutObjectiveDtoTestBuilder WithMuscularHypertrophy()
    {
        _workoutObjectiveId = TestIds.WorkoutObjectiveIds.MuscularHypertrophy;
        _value = "Muscular Hypertrophy";
        _description = "Increase muscle size through moderate loads and volume";
        _displayOrder = 2;
        _isActive = true;
        return this;
    }

    public WorkoutObjectiveDto Build()
    {
        return new WorkoutObjectiveDto
        {
            WorkoutObjectiveId = _workoutObjectiveId,
            Value = _value,
            Description = _description,
            DisplayOrder = _displayOrder,
            IsActive = _isActive
        };
    }
}