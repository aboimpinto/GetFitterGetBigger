using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Mappers;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

public class ExerciseServiceUpdateCoachNotesTests
{
    
    [Fact]
    public async Task UpdateAsync_WithNewCoachNotes_AddsNotesInOrder()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var exerciseId = ExerciseId.New();
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise")
            .WithDescription("Updated Description")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .AddCoachNote("Third note", 3)
            .AddCoachNote("First note", 1)
            .AddCoachNote("Second note", 2)
            .Build();
        
        var expectedDto = new ExerciseDto
        {
            Id = exerciseId.ToString(),
            Name = "Updated Exercise",
            Description = "Updated Description",
            IsActive = true,
            IsUnilateral = false,
            CoachNotes = new List<CoachNoteDto>
            {
                new() { Id = "coach-note-" + Guid.NewGuid(), Text = "First note", Order = 1 },
                new() { Id = "coach-note-" + Guid.NewGuid(), Text = "Second note", Order = 2 },
                new() { Id = "coach-note-" + Guid.NewGuid(), Text = "Third note", Order = 3 }
            }
        };
        
        autoMocker
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceExistsByName("Updated Exercise", false, exerciseId)
            .SetupExerciseCommandDataServiceUpdate(expectedDto)
            .SetupExerciseTypeService(allExist: true, isRestType: false);
        
        // Act
        var result = await testee.UpdateAsync(exerciseId, request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.CoachNotes.Should().HaveCount(3);
        result.Data.CoachNotes[0].Text.Should().Be("First note");
        result.Data.CoachNotes[0].Order.Should().Be(1);
        result.Data.CoachNotes[1].Text.Should().Be("Second note");
        result.Data.CoachNotes[1].Order.Should().Be(2);
        result.Data.CoachNotes[2].Text.Should().Be("Third note");
        result.Data.CoachNotes[2].Order.Should().Be(3);
    }
    
    [Fact]
    public async Task UpdateAsync_WithExistingCoachNoteIds_PreservesIds()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var exerciseId = ExerciseId.New();
        var existingNoteId1 = CoachNoteId.New();
        var existingNoteId2 = CoachNoteId.New();
        
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise")
            .WithDescription("Updated Description")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .AddCoachNote(existingNoteId1, "Updated note 1", 1)
            .AddCoachNote(existingNoteId2, "Updated note 2", 2)
            .AddCoachNote(null, "New note 3", 3)
            .Build();
        
        var expectedDto = new ExerciseDto
        {
            Id = exerciseId.ToString(),
            Name = "Updated Exercise",
            Description = "Updated Description",
            IsActive = true,
            IsUnilateral = false,
            CoachNotes = new List<CoachNoteDto>
            {
                new() { Id = existingNoteId1.ToString(), Text = "Updated note 1", Order = 1 },
                new() { Id = existingNoteId2.ToString(), Text = "Updated note 2", Order = 2 },
                new() { Id = "coach-note-" + Guid.NewGuid(), Text = "New note 3", Order = 3 }
            }
        };
        
        autoMocker
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceExistsByName("Updated Exercise", false, exerciseId)
            .SetupExerciseCommandDataServiceUpdate(expectedDto)
            .SetupExerciseTypeService(allExist: true, isRestType: false);
        
        // Act
        var result = await testee.UpdateAsync(exerciseId, request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.CoachNotes.Should().HaveCount(3);
        
        // Check that existing IDs were preserved
        result.Data.CoachNotes[0].Id.Should().Be(existingNoteId1.ToString());
        result.Data.CoachNotes[0].Text.Should().Be("Updated note 1");
        
        result.Data.CoachNotes[1].Id.Should().Be(existingNoteId2.ToString());
        result.Data.CoachNotes[1].Text.Should().Be("Updated note 2");
        
        // New note should have a new ID
        result.Data.CoachNotes[2].Id.Should().NotBeEmpty();
        result.Data.CoachNotes[2].Id.Should().NotBe(existingNoteId1.ToString());
        result.Data.CoachNotes[2].Id.Should().NotBe(existingNoteId2.ToString());
        result.Data.CoachNotes[2].Text.Should().Be("New note 3");
    }
    
    [Fact]
    public async Task UpdateAsync_RemovingCoachNotes_UpdatesWithEmptyList()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var exerciseId = ExerciseId.New();
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise")
            .WithDescription("Updated Description")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build(); // No AddCoachNote calls = empty coach notes
        
        var expectedDto = new ExerciseDto
        {
            Id = exerciseId.ToString(),
            Name = "Updated Exercise",
            Description = "Updated Description",
            IsActive = true,
            IsUnilateral = false,
            CoachNotes = new List<CoachNoteDto>() // Empty list
        };
        
        autoMocker
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceExistsByName("Updated Exercise", false, exerciseId)
            .SetupExerciseCommandDataServiceUpdate(expectedDto)
            .SetupExerciseTypeService(allExist: true, isRestType: false);
        
        // Act
        var result = await testee.UpdateAsync(exerciseId, request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.CoachNotes.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateAsync_WithInvalidCoachNoteId_IgnoresInvalidIdAndCreatesNew()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var exerciseId = ExerciseId.New();
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise")
            .WithDescription("Updated Description")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .AddCoachNoteWithInvalidFormat("Note with invalid ID", 1)
            .AddCoachNoteWithMalformedId("Note with malformed ID", 2)
            .Build();
        
        var expectedDto = new ExerciseDto
        {
            Id = exerciseId.ToString(),
            Name = "Updated Exercise",
            Description = "Updated Description",
            IsActive = true,
            IsUnilateral = false,
            CoachNotes = new List<CoachNoteDto>
            {
                new() { Id = "coach-note-" + Guid.NewGuid(), Text = "Note with invalid ID", Order = 1 },
                new() { Id = "coach-note-" + Guid.NewGuid(), Text = "Note with malformed ID", Order = 2 }
            }
        };
        
        autoMocker
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceExistsByName("Updated Exercise", false, exerciseId)
            .SetupExerciseCommandDataServiceUpdate(expectedDto)
            .SetupExerciseTypeService(allExist: true, isRestType: false);
        
        // Act
        var result = await testee.UpdateAsync(exerciseId, request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.CoachNotes.Should().HaveCount(2); // Invalid IDs result in new notes being created
        
        // Verify the notes have new IDs (not the invalid ones from the request)
        result.Data.CoachNotes[0].Text.Should().Be("Note with invalid ID");
        result.Data.CoachNotes[1].Text.Should().Be("Note with malformed ID");
    }
    
    [Fact]
    public async Task UpdateAsync_WithNullCoachNotes_DoesNotUpdateCoachNotes()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var exerciseId = ExerciseId.New();
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise")
            .WithDescription("Updated Description")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build();
        
        var expectedDto = new ExerciseDto
        {
            Id = exerciseId.ToString(),
            Name = "Updated Exercise",
            Description = "Updated Description",
            IsActive = true,
            IsUnilateral = false,
            CoachNotes = new List<CoachNoteDto>() // Empty list - null is treated as empty
        };
        
        autoMocker
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceExistsByName("Updated Exercise", false, exerciseId)
            .SetupExerciseCommandDataServiceUpdate(expectedDto)
            .SetupExerciseTypeService(allExist: true, isRestType: false);
        
        // Act
        var result = await testee.UpdateAsync(exerciseId, request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.CoachNotes.Should().BeEmpty(); // Null request.CoachNotes means empty collection in new entity
    }
    
    [Fact]
    public async Task UpdateAsync_PreservesCoachNotesOriginalOrder()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var exerciseId = ExerciseId.New();
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise")
            .WithDescription("Updated Description")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .AddCoachNote("Note at 100", 100)
            .AddCoachNote("Note at 5", 5)
            .AddCoachNote("Note at 50", 50)
            .Build();
        
        var expectedDto = new ExerciseDto
        {
            Id = exerciseId.ToString(),
            Name = "Updated Exercise",
            Description = "Updated Description",
            IsActive = true,
            IsUnilateral = false,
            CoachNotes = new List<CoachNoteDto>
            {
                new() { Id = "coach-note-" + Guid.NewGuid(), Text = "Note at 5", Order = 5 },
                new() { Id = "coach-note-" + Guid.NewGuid(), Text = "Note at 50", Order = 50 },
                new() { Id = "coach-note-" + Guid.NewGuid(), Text = "Note at 100", Order = 100 }
            }
        };
        
        autoMocker
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceExistsByName("Updated Exercise", false, exerciseId)
            .SetupExerciseCommandDataServiceUpdate(expectedDto)
            .SetupExerciseTypeService(allExist: true, isRestType: false);
        
        // Act
        var result = await testee.UpdateAsync(exerciseId, request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.CoachNotes.Should().HaveCount(3);
        
        // Verify notes preserve original order values and are sorted by order
        result.Data.CoachNotes[0].Text.Should().Be("Note at 5");
        result.Data.CoachNotes[0].Order.Should().Be(5);
        result.Data.CoachNotes[1].Text.Should().Be("Note at 50");
        result.Data.CoachNotes[1].Order.Should().Be(50);
        result.Data.CoachNotes[2].Text.Should().Be("Note at 100");
        result.Data.CoachNotes[2].Order.Should().Be(100);
    }
}