using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using Xunit;

namespace GetFitterGetBigger.API.Tests.DTOs.WorkoutTemplateExercise;

/// <summary>
/// Unit tests for WorkoutTemplateExercise DTOs
/// </summary>
public class WorkoutTemplateExerciseDtoTests
{
    [Fact]
    public void AddExerciseDto_Should_HaveCorrectProperties()
    {
        // Arrange & Act
        var dto = AddExerciseDto.Empty;
        
        // Assert
        dto.Should().NotBeNull();
        dto.ExerciseId.Should().NotBeNull();
        dto.Phase.Should().Be(string.Empty);
        dto.RoundNumber.Should().Be(0);
        dto.Metadata.Should().Be(string.Empty);
    }

    [Fact]
    public void AddExerciseResultDto_Should_HaveCorrectProperties()
    {
        // Arrange & Act
        var dto = AddExerciseResultDto.Empty;
        
        // Assert
        dto.Should().NotBeNull();
        dto.AddedExercises.Should().NotBeNull().And.BeEmpty();
        dto.Message.Should().Be(string.Empty);
    }

    [Fact]
    public void RemoveExerciseResultDto_Should_HaveCorrectProperties()
    {
        // Arrange & Act
        var dto = RemoveExerciseResultDto.Empty;
        
        // Assert
        dto.Should().NotBeNull();
        dto.RemovedExercises.Should().NotBeNull().And.BeEmpty();
        dto.Message.Should().Be(string.Empty);
    }

    [Fact]
    public void UpdateMetadataResultDto_Should_HaveCorrectProperties()
    {
        // Arrange & Act
        var dto = UpdateMetadataResultDto.Empty;
        
        // Assert
        dto.Should().NotBeNull();
        dto.UpdatedExercise.Should().NotBeNull();
        dto.Message.Should().Be(string.Empty);
    }

    [Fact]
    public void ReorderResultDto_Should_HaveCorrectProperties()
    {
        // Arrange & Act
        var dto = ReorderResultDto.Empty;
        
        // Assert
        dto.Should().NotBeNull();
        dto.ReorderedExercises.Should().NotBeNull().And.BeEmpty();
        dto.Message.Should().Be(string.Empty);
    }

    [Fact]
    public void CopyRoundDto_Should_HaveCorrectProperties()
    {
        // Arrange & Act
        var dto = CopyRoundDto.Empty;
        
        // Assert
        dto.Should().NotBeNull();
        dto.SourcePhase.Should().Be(string.Empty);
        dto.SourceRoundNumber.Should().Be(0);
        dto.TargetPhase.Should().Be(string.Empty);
        dto.TargetRoundNumber.Should().Be(0);
    }

    [Fact]
    public void CopyRoundResultDto_Should_HaveCorrectProperties()
    {
        // Arrange & Act
        var dto = CopyRoundResultDto.Empty;
        
        // Assert
        dto.Should().NotBeNull();
        dto.CopiedExercises.Should().NotBeNull().And.BeEmpty();
        dto.Message.Should().Be(string.Empty);
    }

    [Fact]
    public void WorkoutTemplateExercisesDto_Should_HaveCorrectProperties()
    {
        // Arrange & Act
        var dto = WorkoutTemplateExercisesDto.Empty;
        
        // Assert
        dto.Should().NotBeNull();
        dto.TemplateId.Should().NotBeNull();
        dto.TemplateName.Should().Be(string.Empty);
        dto.ExecutionProtocol.Should().NotBeNull();
        dto.Warmup.Should().NotBeNull();
        dto.Workout.Should().NotBeNull();
        dto.Cooldown.Should().NotBeNull();
    }

    [Fact]
    public void WorkoutPhaseDto_Should_HaveCorrectProperties()
    {
        // Arrange & Act
        var dto = WorkoutPhaseDto.Empty;
        
        // Assert
        dto.Should().NotBeNull();
        dto.Rounds.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void RoundDto_Should_HaveCorrectProperties()
    {
        // Arrange & Act
        var dto = RoundDto.Empty;
        
        // Assert
        dto.Should().NotBeNull();
        dto.RoundNumber.Should().Be(0);
        dto.Exercises.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void WorkoutTemplateExerciseDto_Should_HaveEnhancedPhaseRoundProperties()
    {
        // Arrange & Act
        var dto = WorkoutTemplateExerciseDto.Empty;
        
        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(string.Empty);
        dto.Phase.Should().Be(string.Empty);
        dto.RoundNumber.Should().Be(0);
        dto.OrderInRound.Should().Be(0);
        dto.Metadata.Should().Be("{}");
        dto.Exercise.Should().NotBeNull();
        
        // Legacy properties should still exist
        dto.Zone.Should().Be(string.Empty);
        dto.SequenceOrder.Should().Be(0);
        dto.SetConfigurations.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void WorkoutTemplateExerciseDto_Should_HaveLegacyPropertiesMarkedObsolete()
    {
        // Arrange
        var dtoType = typeof(WorkoutTemplateExerciseDto);
        
        // Act & Assert
        var zoneProperty = dtoType.GetProperty(nameof(WorkoutTemplateExerciseDto.Zone));
        zoneProperty.Should().NotBeNull();
        zoneProperty!.GetCustomAttributes(typeof(ObsoleteAttribute), false)
            .Should().HaveCount(1);
            
        var sequenceOrderProperty = dtoType.GetProperty(nameof(WorkoutTemplateExerciseDto.SequenceOrder));
        sequenceOrderProperty.Should().NotBeNull();
        sequenceOrderProperty!.GetCustomAttributes(typeof(ObsoleteAttribute), false)
            .Should().HaveCount(1);
            
        var setConfigurationsProperty = dtoType.GetProperty(nameof(WorkoutTemplateExerciseDto.SetConfigurations));
        setConfigurationsProperty.Should().NotBeNull();
        setConfigurationsProperty!.GetCustomAttributes(typeof(ObsoleteAttribute), false)
            .Should().HaveCount(1);
    }
}