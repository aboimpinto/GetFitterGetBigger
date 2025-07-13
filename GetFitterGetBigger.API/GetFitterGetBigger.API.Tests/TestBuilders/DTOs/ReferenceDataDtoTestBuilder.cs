using GetFitterGetBigger.API.DTOs;

namespace GetFitterGetBigger.API.Tests.TestBuilders.DTOs;

/// <summary>
/// Test builder for creating ReferenceDataDto instances
/// </summary>
public class ReferenceDataDtoTestBuilder
{
    private string _id = "workoutobjective-11111111-1111-1111-1111-111111111111";
    private string _value = "Muscular Strength";
    private string? _description = "Build maximum strength through heavy loads and low repetitions";

    private ReferenceDataDtoTestBuilder() { }

    /// <summary>
    /// Creates a builder for MUSCULAR STRENGTH objective DTO
    /// </summary>
    public static ReferenceDataDtoTestBuilder MuscularStrength() => new ReferenceDataDtoTestBuilder()
        .WithId(TestIds.WorkoutObjectiveIds.MuscularStrength)
        .WithValue("Muscular Strength")
        .WithDescription("Build maximum strength through heavy loads and low repetitions");

    /// <summary>
    /// Creates a builder for MUSCULAR HYPERTROPHY objective DTO
    /// </summary>
    public static ReferenceDataDtoTestBuilder MuscularHypertrophy() => new ReferenceDataDtoTestBuilder()
        .WithId(TestIds.WorkoutObjectiveIds.MuscularHypertrophy)
        .WithValue("Muscular Hypertrophy")
        .WithDescription("Build muscle size through moderate loads and volume");

    /// <summary>
    /// Creates a builder for MUSCULAR ENDURANCE objective DTO
    /// </summary>
    public static ReferenceDataDtoTestBuilder MuscularEndurance() => new ReferenceDataDtoTestBuilder()
        .WithId(TestIds.WorkoutObjectiveIds.MuscularEndurance)
        .WithValue("Muscular Endurance")
        .WithDescription("Build endurance through high repetitions and lighter loads");

    /// <summary>
    /// Creates a builder for POWER DEVELOPMENT objective DTO
    /// </summary>
    public static ReferenceDataDtoTestBuilder PowerDevelopment() => new ReferenceDataDtoTestBuilder()
        .WithId(TestIds.WorkoutObjectiveIds.PowerDevelopment)
        .WithValue("Power Development")
        .WithDescription("Build explosive power through dynamic movements");

    public ReferenceDataDtoTestBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public ReferenceDataDtoTestBuilder WithValue(string value)
    {
        _value = value;
        return this;
    }

    public ReferenceDataDtoTestBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ReferenceDataDto Build()
    {
        return new ReferenceDataDto
        {
            Id = _id,
            Value = _value,
            Description = _description
        };
    }
}