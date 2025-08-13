using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.Services.Builders;

/// <summary>
/// Builder for creating ReferenceDataDto instances for testing
/// Used by all reference table services (DifficultyLevel, ExerciseType, etc.)
/// </summary>
public class ReferenceDataDtoBuilder
{
    private string _id = DifficultyLevelId.New().ToString();
    private string _value = "Test Value";
    private string _description = "Test Description";

    public ReferenceDataDtoBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public ReferenceDataDtoBuilder WithId<TId>(TId id)
    {
        _id = id?.ToString() ?? "";
        return this;
    }

    public ReferenceDataDtoBuilder WithValue(string value)
    {
        _value = value;
        return this;
    }

    public ReferenceDataDtoBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    // Default preset
    public static ReferenceDataDtoBuilder Default()
    {
        return new ReferenceDataDtoBuilder();
    }

    // Difficulty Level presets
    public static ReferenceDataDtoBuilder ForBeginner()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Beginner")
            .WithDescription("For beginners");
    }

    public static ReferenceDataDtoBuilder ForIntermediate()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Intermediate")
            .WithDescription("For intermediate users");
    }

    public static ReferenceDataDtoBuilder ForAdvanced()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Advanced")
            .WithDescription("For advanced users");
    }

    // Exercise Type presets
    public static ReferenceDataDtoBuilder ForStrength()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Strength")
            .WithDescription("Strength training exercise");
    }

    public static ReferenceDataDtoBuilder ForCardio()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Cardio")
            .WithDescription("Cardiovascular exercise");
    }

    public static ReferenceDataDtoBuilder ForRest()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Rest")
            .WithDescription("Rest period");
    }

    // Kinetic Chain presets
    public static ReferenceDataDtoBuilder ForOpenChain()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Open Chain")
            .WithDescription("Open kinetic chain movement");
    }

    public static ReferenceDataDtoBuilder ForClosedChain()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Closed Chain")
            .WithDescription("Closed kinetic chain movement");
    }

    // Muscle Role presets
    public static ReferenceDataDtoBuilder ForAgonist()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Agonist")
            .WithDescription("Primary mover muscle");
    }

    public static ReferenceDataDtoBuilder ForAntagonist()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Antagonist")
            .WithDescription("Opposing muscle");
    }

    public static ReferenceDataDtoBuilder ForSynergist()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Synergist")
            .WithDescription("Assisting muscle");
    }

    // Movement Pattern presets
    public static ReferenceDataDtoBuilder ForSquat()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Squat")
            .WithDescription("Squatting movement pattern");
    }

    public static ReferenceDataDtoBuilder ForDeadlift()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Deadlift")
            .WithDescription("Hip hinge movement pattern");
    }

    public static ReferenceDataDtoBuilder ForPush()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Push")
            .WithDescription("Pushing movement pattern");
    }

    public static ReferenceDataDtoBuilder ForPull()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Pull")
            .WithDescription("Pulling movement pattern");
    }

    // Metric Type presets
    public static ReferenceDataDtoBuilder ForWeight()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Weight")
            .WithDescription("Weight measurement");
    }

    public static ReferenceDataDtoBuilder ForTime()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Time")
            .WithDescription("Time measurement");
    }

    public static ReferenceDataDtoBuilder ForDistance()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Distance")
            .WithDescription("Distance measurement");
    }

    // Exercise Weight Type presets
    public static ReferenceDataDtoBuilder ForBodyweight()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Bodyweight")
            .WithDescription("Using body weight");
    }

    public static ReferenceDataDtoBuilder ForWeighted()
    {
        return new ReferenceDataDtoBuilder()
            .WithValue("Weighted")
            .WithDescription("Using external weight");
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