using System;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating ExerciseLink entities with predefined data
/// </summary>
public class ExerciseLinkBuilder
{
    private ExerciseLinkId _id = ExerciseLinkId.New();
    private ExerciseId _sourceExerciseId = ExerciseId.ParseOrEmpty("exercise-source-123");
    private ExerciseId _targetExerciseId = ExerciseId.ParseOrEmpty("exercise-target-456");
    private string _linkType = "WARMUP";
    private ExerciseLinkType? _linkTypeEnum = ExerciseLinkType.WARMUP;
    private int _displayOrder = 1;
    private bool _isActive = true;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public static ExerciseLinkBuilder Create()
    {
        return new ExerciseLinkBuilder();
    }

    public static ExerciseLinkBuilder Empty()
    {
        return new ExerciseLinkBuilder()
            .WithId(ExerciseLinkId.Empty)
            .WithSourceExerciseId(ExerciseId.Empty)
            .WithTargetExerciseId(ExerciseId.Empty)
            .WithLinkType(string.Empty)
            .WithLinkTypeEnum(null)
            .WithDisplayOrder(0)
            .WithIsActive(false)
            .WithCreatedAt(DateTime.MinValue)
            .WithUpdatedAt(DateTime.MinValue);
    }

    public static ExerciseLinkBuilder WarmupLink()
    {
        return new ExerciseLinkBuilder()
            .WithLinkType("Warmup")
            .WithLinkTypeEnum(ExerciseLinkType.WARMUP);
    }

    public static ExerciseLinkBuilder CooldownLink()
    {
        return new ExerciseLinkBuilder()
            .WithLinkType("Cooldown")
            .WithLinkTypeEnum(ExerciseLinkType.COOLDOWN);
    }

    public static ExerciseLinkBuilder WorkoutLink()
    {
        return new ExerciseLinkBuilder()
            .WithLinkType("WORKOUT")
            .WithLinkTypeEnum(ExerciseLinkType.WORKOUT);
    }

    public static ExerciseLinkBuilder AlternativeLink()
    {
        return new ExerciseLinkBuilder()
            .WithLinkType("ALTERNATIVE")
            .WithLinkTypeEnum(ExerciseLinkType.ALTERNATIVE);
    }

    public static ExerciseLinkBuilder LegacyWarmupLink()
    {
        return new ExerciseLinkBuilder()
            .WithLinkType("Warmup")
            .WithLinkTypeEnum(null); // Legacy format without enum
    }

    public static ExerciseLinkBuilder LegacyCooldownLink()
    {
        return new ExerciseLinkBuilder()
            .WithLinkType("Cooldown")
            .WithLinkTypeEnum(null); // Legacy format without enum
    }

    public static ExerciseLinkBuilder Bidirectional(string sourceId, string targetId)
    {
        return new ExerciseLinkBuilder()
            .WithSourceExerciseId(ExerciseId.ParseOrEmpty(sourceId))
            .WithTargetExerciseId(ExerciseId.ParseOrEmpty(targetId));
    }

    public ExerciseLinkBuilder WithId(ExerciseLinkId id)
    {
        _id = id;
        return this;
    }

    public ExerciseLinkBuilder WithId(string id)
    {
        _id = ExerciseLinkId.ParseOrEmpty(id);
        return this;
    }

    public ExerciseLinkBuilder WithSourceExerciseId(ExerciseId sourceExerciseId)
    {
        _sourceExerciseId = sourceExerciseId;
        return this;
    }

    public ExerciseLinkBuilder WithSourceExerciseId(string sourceExerciseId)
    {
        _sourceExerciseId = ExerciseId.ParseOrEmpty(sourceExerciseId);
        return this;
    }

    public ExerciseLinkBuilder WithTargetExerciseId(ExerciseId targetExerciseId)
    {
        _targetExerciseId = targetExerciseId;
        return this;
    }

    public ExerciseLinkBuilder WithTargetExerciseId(string targetExerciseId)
    {
        _targetExerciseId = ExerciseId.ParseOrEmpty(targetExerciseId);
        return this;
    }

    public ExerciseLinkBuilder WithLinkType(string linkType)
    {
        _linkType = linkType;
        return this;
    }

    public ExerciseLinkBuilder WithLinkTypeEnum(ExerciseLinkType? linkTypeEnum)
    {
        _linkTypeEnum = linkTypeEnum;
        return this;
    }

    public ExerciseLinkBuilder WithLinkTypeEnum(ExerciseLinkType linkTypeEnum)
    {
        _linkTypeEnum = linkTypeEnum;
        _linkType = linkTypeEnum.ToString(); // Keep in sync
        return this;
    }

    public ExerciseLinkBuilder WithDisplayOrder(int displayOrder)
    {
        _displayOrder = displayOrder;
        return this;
    }

    public ExerciseLinkBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public ExerciseLinkBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public ExerciseLinkBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }
    
    // Convenience methods for backward compatibility with tests
    public ExerciseLinkBuilder WithExerciseId(ExerciseId exerciseId)
    {
        return WithSourceExerciseId(exerciseId);
    }
    
    public ExerciseLinkBuilder WithLinkedExerciseId(ExerciseId linkedExerciseId)
    {
        return WithTargetExerciseId(linkedExerciseId);
    }

    /// <summary>
    /// Builds the ExerciseLink using the enhanced Handler method (with enum support)
    /// </summary>
    public ExerciseLink Build()
    {
        return ExerciseLink.Handler.Create(
            _id,
            _sourceExerciseId,
            _targetExerciseId,
            _linkType,
            _linkTypeEnum,
            _displayOrder,
            _isActive,
            _createdAt,
            _updatedAt);
    }

    /// <summary>
    /// Builds the ExerciseLink using the legacy Handler method (string-only for backward compatibility testing)
    /// </summary>
    public ExerciseLink BuildLegacy()
    {
        return ExerciseLink.Handler.Create(
            _id,
            _sourceExerciseId,
            _targetExerciseId,
            _linkType,
            _displayOrder,
            _isActive,
            _createdAt,
            _updatedAt);
    }
}