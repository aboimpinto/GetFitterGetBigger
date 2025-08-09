using System;
using System.Collections.Generic;
using System.Text.Json;
using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Unit.DTOs;

public class ExerciseDtoKineticChainTests
{
    [Fact]
    public void ExerciseDto_WithKineticChain_SerializesCorrectly()
    {
        // Arrange
        var dto = new ExerciseDto
            {
            Id = "exercise-123",
            Name = "Squat",
            Description = "Lower body exercise",
            IsUnilateral = false,
            IsActive = true,
            Difficulty = new ReferenceDataDto { Id = "difficulty-beginner", Value = "Beginner" },
            KineticChain = new ReferenceDataDto { Id = "kinetic-closed", Value = "Closed Chain" }
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ExerciseDto>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.KineticChain);
        Assert.Equal("kinetic-closed", deserialized.KineticChain.Id);
        Assert.Equal("Closed Chain", deserialized.KineticChain.Value);
    }

    [Fact]
    public void ExerciseDto_WithoutKineticChain_SerializesAsNull()
    {
        // Arrange
        var dto = new ExerciseDto
            {
            Id = "exercise-123",
            Name = "Rest",
            Description = "Rest period",
            IsUnilateral = false,
            IsActive = true,
            Difficulty = new ReferenceDataDto { Id = "difficulty-beginner", Value = "Beginner" },
            KineticChain = null
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ExerciseDto>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.KineticChain);
    }

    [Fact]
    public void CreateExerciseRequest_WithKineticChainId_SerializesCorrectly()
    {
        // Arrange
        var request = new CreateExerciseRequest
            {
            Name = "Deadlift",
            Description = "Full body exercise",
            DifficultyId = "difficulty-intermediate",
            KineticChainId = "kineticchain-123",
            IsUnilateral = false
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<CreateExerciseRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("kineticchain-123", deserialized.KineticChainId);
    }

    [Fact]
    public void CreateExerciseRequest_WithoutKineticChainId_SerializesAsNull()
    {
        // Arrange
        var request = new CreateExerciseRequest
            {
            Name = "Rest",
            Description = "Rest period",
            DifficultyId = "difficulty-beginner",
            KineticChainId = null,
            IsUnilateral = false
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<CreateExerciseRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.KineticChainId);
    }

    [Fact]
    public void UpdateExerciseRequest_WithKineticChainId_SerializesCorrectly()
    {
        // Arrange
        var request = new UpdateExerciseRequest
            {
            Name = "Bench Press",
            Description = "Upper body exercise",
            DifficultyId = "difficulty-intermediate",
            KineticChainId = "kineticchain-456"
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<UpdateExerciseRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("kineticchain-456", deserialized.KineticChainId);
    }
}