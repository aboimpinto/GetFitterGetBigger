using System;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.DTOs;

/// <summary>
/// Builder for creating EquipmentDto instances for testing
/// Provides good defaults and allows selective overrides
/// </summary>
public class EquipmentDtoTestBuilder
{
    private string _id = EquipmentId.New().ToString();
    private string _name = "Test Equipment";
    private bool _isActive = true;
    private DateTime _createdAt = DateTime.UtcNow.AddDays(-30);
    private DateTime? _updatedAt = DateTime.UtcNow.AddDays(-1);

    public EquipmentDtoTestBuilder WithId(EquipmentId id)
    {
        _id = id.ToString();
        return this;
    }

    public EquipmentDtoTestBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public EquipmentDtoTestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public EquipmentDtoTestBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public EquipmentDtoTestBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public EquipmentDtoTestBuilder WithUpdatedAt(DateTime? updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    // Common equipment presets
    public static EquipmentDtoTestBuilder Barbell()
    {
        return new EquipmentDtoTestBuilder()
            .WithName("Barbell");
    }

    public static EquipmentDtoTestBuilder Dumbbell()
    {
        return new EquipmentDtoTestBuilder()
            .WithName("Dumbbell");
    }

    public static EquipmentDtoTestBuilder ForBarbell()
    {
        return new EquipmentDtoTestBuilder()
            .WithName("Barbell");
    }

    public static EquipmentDtoTestBuilder ForDumbbell()
    {
        return new EquipmentDtoTestBuilder()
            .WithName("Dumbbell");
    }

    public static EquipmentDtoTestBuilder ForKettlebell()
    {
        return new EquipmentDtoTestBuilder()
            .WithName("Kettlebell");
    }

    public static EquipmentDtoTestBuilder ForResistanceBand()
    {
        return new EquipmentDtoTestBuilder()
            .WithName("Resistance Band");
    }

    public static EquipmentDtoTestBuilder ForPullUpBar()
    {
        return new EquipmentDtoTestBuilder()
            .WithName("Pull-up Bar");
    }

    public static EquipmentDtoTestBuilder ForBench()
    {
        return new EquipmentDtoTestBuilder()
            .WithName("Bench");
    }

    public static EquipmentDtoTestBuilder ForTreadmill()
    {
        return new EquipmentDtoTestBuilder()
            .WithName("Treadmill");
    }

    public static EquipmentDtoTestBuilder ForInactive()
    {
        return new EquipmentDtoTestBuilder()
            .WithName("Inactive Equipment")
            .WithIsActive(false);
    }

    public EquipmentDto Build()
    {
        return new EquipmentDto
        {
            Id = _id,
            Name = _name,
            IsActive = _isActive,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt
        };
    }
}