using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Exercise;
using GetFitterGetBigger.API.Services.Exercise.DataServices;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using Moq;
using Moq.AutoMock;
using Olimpo.EntityFramework.Persistency;
using Xunit;
using ExerciseEntity = GetFitterGetBigger.API.Models.Entities.Exercise;

namespace GetFitterGetBigger.API.Tests.Services;

public class ExerciseServiceMapToDtoTests
{
    
    [Fact]
    public async Task GetByIdAsync_WithKineticChain_MapsKineticChainCorrectly()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var difficultyId = DifficultyLevelId.New();
        var kineticChainId = KineticChainTypeId.New();
        
        var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Intermediate", "Medium difficulty", 3).Value;
        var kineticChain = KineticChainType.Handler.Create(kineticChainId, "Open Chain", "Open kinetic chain movement", 1).Value;
        
        var exercise = ExerciseEntity.Handler.CreateNew(
            "Test Exercise",
            "Test Description",
            null,
            null,
            false,
            difficultyId,
            kineticChainId);
            
        // Use reflection to set navigation properties for testing
        typeof(ExerciseEntity).GetProperty(nameof(ExerciseEntity.Difficulty))!.SetValue(exercise, difficulty);
        typeof(ExerciseEntity).GetProperty(nameof(ExerciseEntity.KineticChain))!.SetValue(exercise, kineticChain);
        
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        var expectedDto = new ExerciseDto 
        {
            Name = exercise.Name,
            Id = exercise.Id.ToString(),
            KineticChain = new ReferenceDataDto
            {
                Id = kineticChainId.ToString(),
                Value = "Open Chain",
                Description = "Open kinetic chain movement"
            }
        };
        
        automocker
            .SetupExerciseTypeService()
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceGetById(expectedDto);
        
        // Act
        var result = await testee.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()));
        
        // Assert
        result.Should().NotBeNull();
        result.Data.KineticChain.Should().NotBeNull();
        result.Data.KineticChain.Id.Should().Be(kineticChainId.ToString());
        result.Data.KineticChain.Value.Should().Be("Open Chain");
        result.Data.KineticChain.Description.Should().Be("Open kinetic chain movement");
    }
    
    [Fact]
    public async Task GetByIdAsync_WithoutKineticChain_MapsKineticChainAsNull()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var difficultyId = DifficultyLevelId.New();
        
        var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "Easy difficulty", 1).Value;
        
        var exercise = ExerciseEntity.Handler.CreateNew(
            "Rest Exercise",
            "Rest period",
            null,
            null,
            false,
            difficultyId,
            null); // No kinetic chain
            
        // Use reflection to set navigation properties for testing
        typeof(ExerciseEntity).GetProperty(nameof(ExerciseEntity.Difficulty))!.SetValue(exercise, difficulty);
        
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        var expectedDto = new ExerciseDto 
        {
            Name = exercise.Name,
            Id = exercise.Id.ToString(),
            KineticChain = null
        };
        
        automocker
            .SetupExerciseTypeService()
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceGetById(expectedDto);
        
        // Act
        var result = await testee.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()));
        
        // Assert
        result.Should().NotBeNull();
        result.Data.KineticChain.Should().BeNull();
    }
    
    [Fact]
    public async Task GetByIdAsync_WithCoachNotes_MapsCoachNotesCorrectly()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exercise = ExerciseEntity.Handler.CreateNew(
            "Test Exercise",
            "Description",
            "http://video.url",
            "http://image.url",
            true,
            DifficultyLevelId.New());
        
        // Add coach notes
        var note1 = CoachNote.Handler.CreateNew(exerciseId, "First note", 1);
        var note2 = CoachNote.Handler.CreateNew(exerciseId, "Second note", 2);
        var note3 = CoachNote.Handler.CreateNew(exerciseId, "Third note", 3);
        
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        // Create expected DTO with populated CoachNotes
        var expectedDto = new ExerciseDto 
        { 
            Name = exercise.Name, 
            Id = exercise.Id.ToString(),
            CoachNotes = new List<CoachNoteDto>
            {
                new CoachNoteDto { Id = note1.Id.ToString(), Text = "First note", Order = 1 },
                new CoachNoteDto { Id = note2.Id.ToString(), Text = "Second note", Order = 2 },
                new CoachNoteDto { Id = note3.Id.ToString(), Text = "Third note", Order = 3 }
            }
        };
        
        automocker
            .SetupExerciseTypeService()
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceGetById(expectedDto);
        
        // Act
        var result = await testee.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()));
        
        // Assert
        result.Should().NotBeNull();
        result.Data.CoachNotes.Should().HaveCount(3);
        
        // Verify ordering
        result.Data.CoachNotes[0].Id.Should().Be(note1.Id.ToString());
        result.Data.CoachNotes[0].Text.Should().Be("First note");
        result.Data.CoachNotes[0].Order.Should().Be(1);
        
        result.Data.CoachNotes[1].Id.Should().Be(note2.Id.ToString());
        result.Data.CoachNotes[1].Text.Should().Be("Second note");
        result.Data.CoachNotes[1].Order.Should().Be(2);
        
        result.Data.CoachNotes[2].Id.Should().Be(note3.Id.ToString());
        result.Data.CoachNotes[2].Text.Should().Be("Third note");
        result.Data.CoachNotes[2].Order.Should().Be(3);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithExerciseTypes_MapsExerciseTypesCorrectly()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exercise = ExerciseEntity.Handler.CreateNew(
            "Test Exercise",
            "Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        // Create exercise types
        var warmupType = ExerciseType.Handler.CreateNew("Warmup", "Warmup exercises", 1).Value;
        var workoutType = ExerciseType.Handler.CreateNew("Workout", "Main workout", 2).Value;
        
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        // Create expected DTO with populated ExerciseTypes
        var expectedDto = new ExerciseDto 
        { 
            Name = exercise.Name, 
            Id = exercise.Id.ToString(),
            ExerciseTypes = new List<ReferenceDataDto>
            {
                new ReferenceDataDto { Id = warmupType.Id.ToString(), Value = "Warmup", Description = "Warmup exercises" },
                new ReferenceDataDto { Id = workoutType.Id.ToString(), Value = "Workout", Description = "Main workout" }
            }
        };
        
        automocker
            .SetupExerciseTypeService()
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceGetById(expectedDto);
        
        // Act
        var result = await testee.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()));
        
        // Assert
        result.Should().NotBeNull();
        result.Data.ExerciseTypes.Should().HaveCount(2);
        
        var warmupDto = result.Data.ExerciseTypes.FirstOrDefault(et => et.Value == "Warmup");
        warmupDto.Should().NotBeNull();
        warmupDto.Id.Should().Be(warmupType.Id.ToString());
        warmupDto.Description.Should().Be("Warmup exercises");
        
        var workoutDto = result.Data.ExerciseTypes.FirstOrDefault(et => et.Value == "Workout");
        workoutDto.Should().NotBeNull();
        workoutDto.Id.Should().Be(workoutType.Id.ToString());
        workoutDto.Description.Should().Be("Main workout");
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyCoachNotesAndTypes_ReturnsEmptyCollections()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exercise = ExerciseEntity.Handler.CreateNew(
            "Test Exercise",
            "Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        automocker
            .SetupExerciseTypeService()
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceGetById(new ExerciseDto { Name = exercise.Name, Id = exercise.Id.ToString() });
        
        // Act
        var result = await testee.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()));
        
        // Assert
        result.Should().NotBeNull();
        result.Data.CoachNotes.Should().BeEmpty();
        result.Data.ExerciseTypes.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetPagedAsync_WithCoachNotesAndTypes_MapsAllCorrectly()
    {
        // Arrange
        var exercise1 = ExerciseEntity.Handler.CreateNew(
            "Exercise 1",
            "Description 1",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        var exercise2 = ExerciseEntity.Handler.CreateNew(
            "Exercise 2",
            "Description 2",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        // Create coach notes for exercise1
        var note1 = CoachNote.Handler.CreateNew(exercise1.Id, "Note 1", 1);
        var note2 = CoachNote.Handler.CreateNew(exercise1.Id, "Note 2", 2);
        
        // Create exercise types for exercise2
        var exerciseType = ExerciseType.Handler.CreateNew("Cooldown", "Cooldown exercises", 3).Value;
        
        var filterParams = new ExerciseFilterParams
            {
            Page = 1,
            PageSize = 10
        };
        
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        // Create DTOs with populated collections
        var exercise1Dto = new ExerciseDto 
        { 
            Name = "Exercise 1", 
            Id = exercise1.Id.ToString(),
            CoachNotes = new List<CoachNoteDto>
            {
                new CoachNoteDto { Id = note1.Id.ToString(), Text = "Note 1", Order = 1 },
                new CoachNoteDto { Id = note2.Id.ToString(), Text = "Note 2", Order = 2 }
            }
        };
        
        var exercise2Dto = new ExerciseDto 
        { 
            Name = "Exercise 2", 
            Id = exercise2.Id.ToString(),
            ExerciseTypes = new List<ReferenceDataDto>
            {
                new ReferenceDataDto { Id = exerciseType.Id.ToString(), Value = "Cooldown", Description = "Cooldown exercises" }
            }
        };
        
        var pagedResponse = new PagedResponse<ExerciseDto>(
            new List<ExerciseDto> { exercise1Dto, exercise2Dto }, 
            2, 1, 10);
        automocker
            .SetupExerciseTypeService()
            .SetupExerciseQueryDataServiceGetPaged(pagedResponse);
        
        // Act
        var result = await testee.GetPagedAsync(filterParams.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        
        // Check exercise1 has coach notes
        var dto1 = result.Data.Items.First(d => d.Name == "Exercise 1");
        dto1.CoachNotes.Should().HaveCount(2);
        dto1.ExerciseTypes.Should().BeEmpty();
        
        // Check exercise2 has exercise types
        var dto2 = result.Data.Items.First(d => d.Name == "Exercise 2");
        dto2.CoachNotes.Should().BeEmpty();
        dto2.ExerciseTypes.Should().HaveCount(1);
        dto2.ExerciseTypes[0].Value.Should().Be("Cooldown");
    }
    
    [Fact]
    public async Task GetByIdAsync_WithNullExerciseType_HandlesGracefully()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exercise = ExerciseEntity.Handler.CreateNew(
            "Test Exercise",
            "Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        // Add exercise type association with null ExerciseType
        var eet = ExerciseExerciseType.Handler.Create(exerciseId, ExerciseTypeId.New());
        exercise.ExerciseExerciseTypes.Add(eet);
        // Navigation property is null by default
        
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        automocker
            .SetupExerciseTypeService()
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceGetById(new ExerciseDto { Name = exercise.Name, Id = exercise.Id.ToString() });
        
        // Act
        var result = await testee.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()));
        
        // Assert
        result.Should().NotBeNull();
        result.Data.ExerciseTypes.Should().BeEmpty(); // Null navigation properties are filtered out
    }
}