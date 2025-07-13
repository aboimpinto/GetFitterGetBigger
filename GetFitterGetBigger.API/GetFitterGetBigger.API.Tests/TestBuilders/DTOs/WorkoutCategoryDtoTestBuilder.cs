using GetFitterGetBigger.API.DTOs;

namespace GetFitterGetBigger.API.Tests.TestBuilders.DTOs;

/// <summary>
/// Test builder for creating WorkoutCategoryDto instances
/// </summary>
public class WorkoutCategoryDtoTestBuilder
{
    private string _workoutCategoryId = "workoutcategory-11111111-1111-1111-1111-111111111111";
    private string _value = "Upper Body - Push";
    private string? _description = "Push exercises targeting chest, shoulders, and triceps";
    private string _icon = "💪";
    private string _color = "#FF5722";
    private string? _primaryMuscleGroups = "Chest,Shoulders,Triceps";
    private int _displayOrder = 1;
    private bool _isActive = true;

    private WorkoutCategoryDtoTestBuilder() { }

    /// <summary>
    /// Creates a builder for UPPER BODY PUSH category DTO
    /// </summary>
    public static WorkoutCategoryDtoTestBuilder UpperBodyPush() => new WorkoutCategoryDtoTestBuilder()
        .WithWorkoutCategoryId(TestIds.WorkoutCategoryIds.UpperBodyPush)
        .WithValue("Upper Body - Push")
        .WithDescription("Push exercises targeting chest, shoulders, and triceps")
        .WithIcon("💪")
        .WithColor("#FF5722")
        .WithPrimaryMuscleGroups("Chest,Shoulders,Triceps")
        .WithDisplayOrder(1);

    /// <summary>
    /// Creates a builder for UPPER BODY PULL category DTO
    /// </summary>
    public static WorkoutCategoryDtoTestBuilder UpperBodyPull() => new WorkoutCategoryDtoTestBuilder()
        .WithWorkoutCategoryId(TestIds.WorkoutCategoryIds.UpperBodyPull)
        .WithValue("Upper Body - Pull")
        .WithDescription("Pull exercises targeting back and biceps")
        .WithIcon("🏋️")
        .WithColor("#4CAF50")
        .WithPrimaryMuscleGroups("Back,Biceps")
        .WithDisplayOrder(2);

    /// <summary>
    /// Creates a builder for LOWER BODY category DTO
    /// </summary>
    public static WorkoutCategoryDtoTestBuilder LowerBody() => new WorkoutCategoryDtoTestBuilder()
        .WithWorkoutCategoryId(TestIds.WorkoutCategoryIds.LowerBody)
        .WithValue("Lower Body")
        .WithDescription("Lower body exercises for legs and glutes")
        .WithIcon("🦵")
        .WithColor("#2196F3")
        .WithPrimaryMuscleGroups("Quadriceps,Hamstrings,Glutes,Calves")
        .WithDisplayOrder(3);

    /// <summary>
    /// Creates a builder for CORE category DTO
    /// </summary>
    public static WorkoutCategoryDtoTestBuilder Core() => new WorkoutCategoryDtoTestBuilder()
        .WithWorkoutCategoryId(TestIds.WorkoutCategoryIds.Core)
        .WithValue("Core")
        .WithDescription("Core stability and strength exercises")
        .WithIcon("🎯")
        .WithColor("#9C27B0")
        .WithPrimaryMuscleGroups("Abs,Obliques,Lower Back")
        .WithDisplayOrder(4);

    public WorkoutCategoryDtoTestBuilder WithWorkoutCategoryId(string workoutCategoryId)
    {
        _workoutCategoryId = workoutCategoryId;
        return this;
    }

    public WorkoutCategoryDtoTestBuilder WithValue(string value)
    {
        _value = value;
        return this;
    }

    public WorkoutCategoryDtoTestBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public WorkoutCategoryDtoTestBuilder WithIcon(string icon)
    {
        _icon = icon;
        return this;
    }

    public WorkoutCategoryDtoTestBuilder WithColor(string color)
    {
        _color = color;
        return this;
    }

    public WorkoutCategoryDtoTestBuilder WithPrimaryMuscleGroups(string? primaryMuscleGroups)
    {
        _primaryMuscleGroups = primaryMuscleGroups;
        return this;
    }

    public WorkoutCategoryDtoTestBuilder WithDisplayOrder(int displayOrder)
    {
        _displayOrder = displayOrder;
        return this;
    }

    public WorkoutCategoryDtoTestBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public WorkoutCategoryDto Build()
    {
        return new WorkoutCategoryDto
        {
            WorkoutCategoryId = _workoutCategoryId,
            Value = _value,
            Description = _description,
            Icon = _icon,
            Color = _color,
            PrimaryMuscleGroups = _primaryMuscleGroups,
            DisplayOrder = _displayOrder,
            IsActive = _isActive
        };
    }
}