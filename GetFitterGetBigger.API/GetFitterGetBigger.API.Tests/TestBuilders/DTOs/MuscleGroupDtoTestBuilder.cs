using System;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.DTOs;

/// <summary>
/// Builder for creating MuscleGroupDto instances for testing
/// Provides good defaults and allows selective overrides
/// </summary>
public class MuscleGroupDtoTestBuilder
{
    private string _id = MuscleGroupId.New().ToString();
    private string _name = "Test Muscle Group";
    private string _bodyPartId = BodyPartId.New().ToString();
    private string? _bodyPartName = "Test Body Part";
    private bool _isActive = true;
    private DateTime _createdAt = DateTime.UtcNow.AddDays(-30);
    private DateTime? _updatedAt = DateTime.UtcNow.AddDays(-1);

    public MuscleGroupDtoTestBuilder WithId(MuscleGroupId id)
    {
        _id = id.ToString();
        return this;
    }

    public MuscleGroupDtoTestBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public MuscleGroupDtoTestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public MuscleGroupDtoTestBuilder WithBodyPartId(BodyPartId bodyPartId)
    {
        _bodyPartId = bodyPartId.ToString();
        return this;
    }

    public MuscleGroupDtoTestBuilder WithBodyPartId(string bodyPartId)
    {
        _bodyPartId = bodyPartId;
        return this;
    }

    public MuscleGroupDtoTestBuilder WithBodyPartName(string? bodyPartName)
    {
        _bodyPartName = bodyPartName;
        return this;
    }

    public MuscleGroupDtoTestBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public MuscleGroupDtoTestBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public MuscleGroupDtoTestBuilder WithUpdatedAt(DateTime? updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    // Common presets
    public static MuscleGroupDtoTestBuilder Chest()
    {
        return new MuscleGroupDtoTestBuilder()
            .WithName("Pectorals")
            .WithBodyPartName("Chest");
    }

    public static MuscleGroupDtoTestBuilder Back()
    {
        return new MuscleGroupDtoTestBuilder()
            .WithName("Latissimus Dorsi")
            .WithBodyPartName("Back");
    }

    public static MuscleGroupDtoTestBuilder ForBiceps()
    {
        return new MuscleGroupDtoTestBuilder()
            .WithName("Biceps")
            .WithBodyPartName("Arms");
    }

    public static MuscleGroupDtoTestBuilder ForQuadriceps()
    {
        return new MuscleGroupDtoTestBuilder()
            .WithName("Quadriceps")
            .WithBodyPartName("Legs");
    }

    public static MuscleGroupDtoTestBuilder ForPectorals()
    {
        return new MuscleGroupDtoTestBuilder()
            .WithName("Pectorals")
            .WithBodyPartName("Chest");
    }

    public static MuscleGroupDtoTestBuilder ForLatissimus()
    {
        return new MuscleGroupDtoTestBuilder()
            .WithName("Latissimus Dorsi")
            .WithBodyPartName("Back");
    }

    public static MuscleGroupDtoTestBuilder ForInactive()
    {
        return new MuscleGroupDtoTestBuilder()
            .WithName("Inactive Muscle Group")
            .WithIsActive(false);
    }

    public MuscleGroupDto Build()
    {
        return new MuscleGroupDto
        {
            Id = _id,
            Name = _name,
            BodyPartId = _bodyPartId,
            BodyPartName = _bodyPartName,
            IsActive = _isActive,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt
        };
    }
}