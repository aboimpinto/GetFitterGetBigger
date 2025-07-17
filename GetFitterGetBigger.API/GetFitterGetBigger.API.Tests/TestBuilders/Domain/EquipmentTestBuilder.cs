using System;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating Equipment entities with predefined data
/// </summary>
public class EquipmentTestBuilder
{
    private EquipmentId _equipmentId = EquipmentId.New();
    private string _name = "Default Equipment";
    private bool _isActive = true;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime? _updatedAt = null;
    
    public static EquipmentTestBuilder Empty()
    {
        return new EquipmentTestBuilder()
            .WithId(EquipmentId.Empty)
            .WithName(string.Empty)
            .WithIsActive(false)
            .WithCreatedAt(DateTime.MinValue);
    }
    
    public static EquipmentTestBuilder Barbell()
    {
        return new EquipmentTestBuilder()
            .WithId(EquipmentId.ParseOrEmpty("equipment-11111111-1111-1111-1111-111111111111"))
            .WithName("Barbell")
            .WithCreatedAt(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
    
    public static EquipmentTestBuilder Dumbbell()
    {
        return new EquipmentTestBuilder()
            .WithId(EquipmentId.ParseOrEmpty("equipment-22222222-2222-2222-2222-222222222222"))
            .WithName("Dumbbell")
            .WithCreatedAt(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
    
    public static EquipmentTestBuilder CableMachine()
    {
        return new EquipmentTestBuilder()
            .WithId(EquipmentId.ParseOrEmpty("equipment-33333333-3333-3333-3333-333333333333"))
            .WithName("Cable Machine")
            .WithCreatedAt(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
    
    public static EquipmentTestBuilder ResistanceBand()
    {
        return new EquipmentTestBuilder()
            .WithId(EquipmentId.ParseOrEmpty("equipment-44444444-4444-4444-4444-444444444444"))
            .WithName("Resistance Band")
            .WithCreatedAt(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
    
    public static EquipmentTestBuilder PullUpBar()
    {
        return new EquipmentTestBuilder()
            .WithId(EquipmentId.ParseOrEmpty("equipment-55555555-5555-5555-5555-555555555555"))
            .WithName("Pull-up Bar")
            .WithCreatedAt(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
    
    public static EquipmentTestBuilder Kettlebell()
    {
        return new EquipmentTestBuilder()
            .WithId(EquipmentId.ParseOrEmpty("equipment-66666666-6666-6666-6666-666666666666"))
            .WithName("Kettlebell")
            .WithCreatedAt(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
    
    public static EquipmentTestBuilder Inactive()
    {
        return new EquipmentTestBuilder()
            .WithId(EquipmentId.ParseOrEmpty("equipment-77777777-7777-7777-7777-777777777777"))
            .WithName("Inactive Equipment")
            .WithIsActive(false)
            .WithCreatedAt(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            .WithUpdatedAt(new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc));
    }
    
    public static EquipmentTestBuilder Custom(string name)
    {
        return new EquipmentTestBuilder()
            .WithName(name);
    }
    
    public EquipmentTestBuilder WithId(EquipmentId id)
    {
        _equipmentId = id;
        return this;
    }
    
    public EquipmentTestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public EquipmentTestBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }
    
    public EquipmentTestBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }
    
    public EquipmentTestBuilder WithUpdatedAt(DateTime? updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }
    
    public Equipment Build()
    {
        return Equipment.Handler.Create(
            _equipmentId,
            _name,
            _isActive,
            _createdAt,
            _updatedAt);
    }
}