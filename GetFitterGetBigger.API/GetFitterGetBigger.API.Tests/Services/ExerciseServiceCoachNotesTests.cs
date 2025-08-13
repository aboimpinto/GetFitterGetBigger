using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Mappers;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

public class ExerciseServiceCoachNotesTests
{
    
    [Fact]
    public async Task CreateAsync_WithCoachNotes_CreatesExerciseWithOrderedNotes()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise with Notes")
            .WithDescription("Description")
            .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
            .WithCoachNotes(("Second note", 2), ("First note", 1), ("Third note", 3))
            .Build();
        
        var expectedResult = new ExerciseDto
        {
            Id = ExerciseId.New().ToString(),
            Name = "Exercise with Notes",
            Description = "Description",
            CoachNotes = new List<CoachNoteDto>
            {
                new() { Text = "First note", Order = 1 },
                new() { Text = "Second note", Order = 2 },
                new() { Text = "Third note", Order = 3 }
            }
        };
        
        automocker
            .SetupExerciseQueryDataServiceExistsByName("Exercise with Notes", false)
            .SetupExerciseCommandDataServiceCreate(expectedResult);
        
        // Act
        var result = await testee.CreateAsync(request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue($"Create failed with errors: {string.Join(", ", result.Errors)}");
        result.Data.CoachNotes.Should().HaveCount(3);
        
        // Verify notes are in correct order
        var orderedNotes = result.Data.CoachNotes.OrderBy(cn => cn.Order).ToList();
        orderedNotes[0].Text.Should().Be("First note");
        orderedNotes[0].Order.Should().Be(1);
        orderedNotes[1].Text.Should().Be("Second note");
        orderedNotes[1].Order.Should().Be(2);
        orderedNotes[2].Text.Should().Be("Third note");
        orderedNotes[2].Order.Should().Be(3);
    }
    
    [Fact]
    public async Task CreateAsync_WithExerciseTypes_CreatesExerciseWithTypes()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        var exerciseTypeId1 = ExerciseTypeId.New();
        var exerciseTypeId2 = ExerciseTypeId.New();
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise with Types")
            .WithDescription("Description")
            .WithExerciseTypes(exerciseTypeId1.ToString(), exerciseTypeId2.ToString())
            .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
            .Build();
        
        var expectedResult = new ExerciseDto
        {
            Id = ExerciseId.New().ToString(),
            Name = "Exercise with Types",
            Description = "Description",
            ExerciseTypes = new List<ReferenceDataDto>
            {
                new() { Id = exerciseTypeId1.ToString(), Value = "Type1" },
                new() { Id = exerciseTypeId2.ToString(), Value = "Type2" }
            }
        };
        
        automocker
            .SetupExerciseQueryDataServiceExistsByName("Exercise with Types", false)
            .SetupExerciseCommandDataServiceCreate(expectedResult);
        
        // Act
        var result = await testee.CreateAsync(request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.ExerciseTypes.Should().HaveCount(2);
        result.Data.ExerciseTypes.Should().Contain(et => et.Id == exerciseTypeId1.ToString());
        result.Data.ExerciseTypes.Should().Contain(et => et.Id == exerciseTypeId2.ToString());
    }
    
    [Fact]
    public async Task CreateAsync_WithEmptyCoachNotes_CreatesExerciseWithoutNotes()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise without Notes")
            .WithDescription("Description")
            .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
            .WithCoachNotes() // Empty coach notes
            .Build();
        
        var expectedResult = new ExerciseDto
        {
            Id = ExerciseId.New().ToString(),
            Name = "Exercise without Notes",
            Description = "Description",
            CoachNotes = new List<CoachNoteDto>()
        };
        
        automocker
            .SetupExerciseQueryDataServiceExistsByName("Exercise without Notes", false)
            .SetupExerciseCommandDataServiceCreate(expectedResult);
        
        // Act
        var result = await testee.CreateAsync(request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.CoachNotes.Should().BeEmpty();
    }
    
    [Fact]
    public async Task CreateAsync_WithInvalidExerciseTypeId_IgnoresInvalidId()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        var validId = ExerciseTypeId.New();
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise with Invalid Type")
            .WithDescription("Description")
            .WithExerciseTypes(validId.ToString(), "invalid-id", "exercisetype-not-a-guid")
            .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
            .Build();
        
        var expectedResult = new ExerciseDto
        {
            Id = ExerciseId.New().ToString(),
            Name = "Exercise with Invalid Type",
            Description = "Description",
            ExerciseTypes = new List<ReferenceDataDto>
            {
                new() { Id = validId.ToString(), Value = "ValidType" }
            }
        };
        
        automocker
            .SetupExerciseQueryDataServiceExistsByName("Exercise with Invalid Type", false)
            .SetupExerciseCommandDataServiceCreate(expectedResult);
        
        // Act
        var result = await testee.CreateAsync(request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.ExerciseTypes.Should().HaveCount(1, "only valid exercise type IDs should be processed");
        result.Data.ExerciseTypes.First().Id.Should().Be(validId.ToString());
    }
    
    [Fact]
    public async Task CreateAsync_WithCoachNotesPreservesOriginalOrder()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise with Gaps")
            .WithDescription("Description")
            .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
            .WithCoachNotes(("Note at 10", 10), ("Note at 5", 5), ("Note at 20", 20))
            .Build();
        
        var expectedResult = new ExerciseDto
        {
            Id = ExerciseId.New().ToString(),
            Name = "Exercise with Gaps",
            Description = "Description",
            CoachNotes = new List<CoachNoteDto>
            {
                new() { Text = "Note at 5", Order = 5 },
                new() { Text = "Note at 10", Order = 10 },
                new() { Text = "Note at 20", Order = 20 }
            }
        };
        
        automocker
            .SetupExerciseQueryDataServiceExistsByName("Exercise with Gaps", false)
            .SetupExerciseCommandDataServiceCreate(expectedResult);
        
        // Act
        var result = await testee.CreateAsync(request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue($"Create failed with errors: {string.Join(", ", result.Errors)}");
        result.Data.CoachNotes.Should().HaveCount(3);
        
        // Verify notes preserve original order values
        var orderedNotes = result.Data.CoachNotes.OrderBy(cn => cn.Order).ToList();
        orderedNotes[0].Text.Should().Be("Note at 5");
        orderedNotes[0].Order.Should().Be(5);
        orderedNotes[1].Text.Should().Be("Note at 10");
        orderedNotes[1].Order.Should().Be(10);
        orderedNotes[2].Text.Should().Be("Note at 20");
        orderedNotes[2].Order.Should().Be(20);
    }
}