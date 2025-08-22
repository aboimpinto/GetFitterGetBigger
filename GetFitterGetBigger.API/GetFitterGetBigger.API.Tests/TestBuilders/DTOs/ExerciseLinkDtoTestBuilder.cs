using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.DTOs;

/// <summary>
/// Test builder for creating ExerciseLinkDto instances with sensible defaults.
/// Follows the Test Builder Pattern to create focused, maintainable test data.
/// </summary>
public class ExerciseLinkDtoTestBuilder
{
    private string _id;
    private string _sourceExerciseId;
    private string _targetExerciseId;
    private ExerciseDto? _targetExercise;
    private string _linkType = ExerciseLinkType.ALTERNATIVE.ToString();
    private int _displayOrder = 1;
    private bool _isActive = true;
    
    private ExerciseLinkDtoTestBuilder()
    {
        _id = ExerciseLinkId.New().ToString();
        _sourceExerciseId = ExerciseId.New().ToString();
        _targetExerciseId = ExerciseId.New().ToString();
    }
    
    // Factory methods for common link types
    public static ExerciseLinkDtoTestBuilder Default() => new();
    
    public static ExerciseLinkDtoTestBuilder WarmupLink()
    {
        return new ExerciseLinkDtoTestBuilder()
            .WithLinkType(ExerciseLinkType.WARMUP);
    }
    
    public static ExerciseLinkDtoTestBuilder CooldownLink()
    {
        return new ExerciseLinkDtoTestBuilder()
            .WithLinkType(ExerciseLinkType.COOLDOWN);
    }
    
    public static ExerciseLinkDtoTestBuilder AlternativeLink()
    {
        return new ExerciseLinkDtoTestBuilder()
            .WithLinkType(ExerciseLinkType.ALTERNATIVE);
    }
    
    public static ExerciseLinkDtoTestBuilder WorkoutLink()
    {
        return new ExerciseLinkDtoTestBuilder()
            .WithLinkType(ExerciseLinkType.WORKOUT);
    }
    
    // Fluent builder methods
    public ExerciseLinkDtoTestBuilder WithId(ExerciseLinkId id)
    {
        _id = id.ToString();
        return this;
    }
    
    public ExerciseLinkDtoTestBuilder WithId(string id)
    {
        _id = id;
        return this;
    }
    
    public ExerciseLinkDtoTestBuilder WithSourceExercise(ExerciseId exerciseId)
    {
        _sourceExerciseId = exerciseId.ToString();
        return this;
    }
    
    public ExerciseLinkDtoTestBuilder WithSourceExercise(string exerciseId)
    {
        _sourceExerciseId = exerciseId;
        return this;
    }
    
    public ExerciseLinkDtoTestBuilder WithTargetExercise(ExerciseId exerciseId)
    {
        _targetExerciseId = exerciseId.ToString();
        return this;
    }
    
    public ExerciseLinkDtoTestBuilder WithTargetExercise(string exerciseId)
    {
        _targetExerciseId = exerciseId;
        return this;
    }
    
    public ExerciseLinkDtoTestBuilder WithLinkType(ExerciseLinkType linkType)
    {
        _linkType = linkType.ToString();
        return this;
    }
    
    public ExerciseLinkDtoTestBuilder WithLinkType(string linkType)
    {
        _linkType = linkType;
        return this;
    }
    
    public ExerciseLinkDtoTestBuilder WithDisplayOrder(int displayOrder)
    {
        _displayOrder = displayOrder;
        return this;
    }
    
    public ExerciseLinkDtoTestBuilder WithTargetExercise(ExerciseDto targetExercise)
    {
        _targetExercise = targetExercise;
        return this;
    }
    
    public ExerciseLinkDtoTestBuilder AsActive()
    {
        _isActive = true;
        return this;
    }
    
    public ExerciseLinkDtoTestBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }
    
    public ExerciseLinkDto Build()
    {
        return new ExerciseLinkDto
        {
            Id = _id,
            SourceExerciseId = _sourceExerciseId,
            TargetExerciseId = _targetExerciseId,
            TargetExercise = _targetExercise,
            LinkType = _linkType,
            DisplayOrder = _displayOrder,
            IsActive = _isActive
        };
    }
    
    // Implicit conversion for convenience
    public static implicit operator ExerciseLinkDto(ExerciseLinkDtoTestBuilder builder) => builder.Build();
}