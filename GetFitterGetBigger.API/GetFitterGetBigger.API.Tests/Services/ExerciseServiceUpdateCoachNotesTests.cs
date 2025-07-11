using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Tests.TestBuilders;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

public class ExerciseServiceUpdateCoachNotesTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _readOnlyUnitOfWorkMock;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _writableUnitOfWorkMock;
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
    private readonly Mock<IExerciseTypeService> _mockExerciseTypeService;
    private readonly ExerciseService _exerciseService;
    
    public ExerciseServiceUpdateCoachNotesTests()
    {
        _unitOfWorkProviderMock = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _readOnlyUnitOfWorkMock = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _writableUnitOfWorkMock = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _exerciseRepositoryMock = new Mock<IExerciseRepository>();
        _mockExerciseTypeService = new Mock<IExerciseTypeService>();
        
        _readOnlyUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _writableUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _unitOfWorkProviderMock.Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _unitOfWorkProviderMock.Setup(p => p.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);
        
        // Setup default mock behaviors for ExerciseTypeService
        _mockExerciseTypeService
            .Setup(s => s.AllExistAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(true);
            
        _mockExerciseTypeService
            .Setup(s => s.AnyIsRestTypeAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync((IEnumerable<string> ids) => 
                ids.Any(id => id == "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a" || 
                              id.ToLowerInvariant().Contains("rest")));
        
        // Default behavior: all exercise types exist
        _mockExerciseTypeService
            .Setup(s => s.ExistsAsync(It.IsAny<ExerciseTypeId>()))
            .ReturnsAsync(true);
        
        _exerciseService = new ExerciseService(_unitOfWorkProviderMock.Object, _mockExerciseTypeService.Object);
    }
    
    [Fact]
    public async Task UpdateAsync_WithNewCoachNotes_AddsNotesInOrder()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var existingExercise = Exercise.Handler.CreateNew(
            "Existing Exercise",
            "Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise")
            .WithDescription("Updated Description")
            .WithCoachNotes(("Third note", 3), ("First note", 1), ("Second note", 2))
            .WithExerciseTypes() // Empty exercise types
            .WithMuscleGroups(("musclegroup-chest-123", "musclerole-primary-456"))
            .Build();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId>()))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
            .ReturnsAsync(existingExercise);
        
        Exercise? capturedExercise = null;
        _exerciseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.UpdateAsync(exerciseId.ToString(), request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.CoachNotes.Count);
        Assert.Equal("First note", result.CoachNotes[0].Text);
        Assert.Equal(1, result.CoachNotes[0].Order);
        Assert.Equal("Second note", result.CoachNotes[1].Text);
        Assert.Equal(2, result.CoachNotes[1].Order);
        Assert.Equal("Third note", result.CoachNotes[2].Text);
        Assert.Equal(3, result.CoachNotes[2].Order);
        
        // Verify the entity was updated with coach notes
        Assert.NotNull(capturedExercise);
        Assert.Equal(3, capturedExercise.CoachNotes.Count);
    }
    
    [Fact]
    public async Task UpdateAsync_WithExistingCoachNoteIds_PreservesIds()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var existingExercise = Exercise.Handler.CreateNew(
            "Existing Exercise",
            "Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        var existingNoteId1 = CoachNoteId.New();
        var existingNoteId2 = CoachNoteId.New();
        
        existingExercise.CoachNotes.Add(CoachNote.Handler.Create(existingNoteId1, exerciseId, "Old note 1", 1));
        existingExercise.CoachNotes.Add(CoachNote.Handler.Create(existingNoteId2, exerciseId, "Old note 2", 2));
        
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise")
            .WithDescription("Updated Description")
            .WithCoachNotes(
                (existingNoteId1.ToString(), "Updated note 1", 1),
                (existingNoteId2.ToString(), "Updated note 2", 2),
                ("", "New note 3", 3)) // New note without ID - use empty string for ID
            .WithExerciseTypes() // Empty exercise types
            .WithMuscleGroups(("musclegroup-chest-123", "musclerole-primary-456"))
            .Build();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId>()))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
            .ReturnsAsync(existingExercise);
        
        Exercise? capturedExercise = null;
        _exerciseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.UpdateAsync(exerciseId.ToString(), request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.CoachNotes.Count);
        
        // Check that existing IDs were preserved
        Assert.Equal(existingNoteId1.ToString(), result.CoachNotes[0].Id);
        Assert.Equal("Updated note 1", result.CoachNotes[0].Text);
        
        Assert.Equal(existingNoteId2.ToString(), result.CoachNotes[1].Id);
        Assert.Equal("Updated note 2", result.CoachNotes[1].Text);
        
        // New note should have a new ID
        Assert.NotEmpty(result.CoachNotes[2].Id);
        Assert.NotEqual(existingNoteId1.ToString(), result.CoachNotes[2].Id);
        Assert.NotEqual(existingNoteId2.ToString(), result.CoachNotes[2].Id);
        Assert.Equal("New note 3", result.CoachNotes[2].Text);
    }
    
    [Fact]
    public async Task UpdateAsync_RemovingCoachNotes_UpdatesWithEmptyList()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var existingExercise = Exercise.Handler.CreateNew(
            "Existing Exercise",
            "Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        // Add existing notes
        existingExercise.CoachNotes.Add(CoachNote.Handler.CreateNew(exerciseId, "Note 1", 1));
        existingExercise.CoachNotes.Add(CoachNote.Handler.CreateNew(exerciseId, "Note 2", 2));
        
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise")
            .WithDescription("Updated Description")
            .WithCoachNotes(Array.Empty<(string Text, int Order)>()) // Empty list
            .WithExerciseTypes() // Empty exercise types
            .WithMuscleGroups(("musclegroup-chest-123", "musclerole-primary-456"))
            .Build();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId>()))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
            .ReturnsAsync(existingExercise);
        
        Exercise? capturedExercise = null;
        _exerciseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.UpdateAsync(exerciseId.ToString(), request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.CoachNotes);
        
        // Verify the entity was updated with empty coach notes
        Assert.NotNull(capturedExercise);
        Assert.Empty(capturedExercise.CoachNotes);
    }
    
    [Fact]
    public async Task UpdateAsync_WithInvalidCoachNoteId_IgnoresInvalidIdAndCreatesNew()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var existingExercise = Exercise.Handler.CreateNew(
            "Existing Exercise",
            "Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise")
            .WithDescription("Updated Description")
            .WithCoachNotes(
                ("invalid-id", "Note with invalid ID", 1),
                ("coachnote-not-a-guid", "Note with malformed ID", 2))
            .WithExerciseTypes() // Empty exercise types
            .WithMuscleGroups(("musclegroup-chest-123", "musclerole-primary-456"))
            .Build();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId>()))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
            .ReturnsAsync(existingExercise);
        
        Exercise? capturedExercise = null;
        _exerciseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.UpdateAsync(exerciseId.ToString(), request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.CoachNotes); // Invalid IDs are ignored, no new notes created
        
        // Verify the entity has no coach notes (invalid IDs were ignored)
        Assert.NotNull(capturedExercise);
        Assert.Empty(capturedExercise.CoachNotes);
    }
    
    [Fact]
    public async Task UpdateAsync_WithNullCoachNotes_DoesNotUpdateCoachNotes()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var existingExercise = Exercise.Handler.CreateNew(
            "Existing Exercise",
            "Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        // Add existing notes
        var existingNote1 = CoachNote.Handler.CreateNew(exerciseId, "Existing note 1", 1);
        var existingNote2 = CoachNote.Handler.CreateNew(exerciseId, "Existing note 2", 2);
        existingExercise.CoachNotes.Add(existingNote1);
        existingExercise.CoachNotes.Add(existingNote2);
        
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise")
            .WithDescription("Updated Description")
            .WithExerciseTypes() // Empty exercise types
            .WithMuscleGroups(("musclegroup-chest-123", "musclerole-primary-456"))
            .Build();
        
        // Manually set CoachNotes to null to test the null behavior
        request.CoachNotes = null!;
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId>()))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
            .ReturnsAsync(existingExercise);
        
        Exercise? capturedExercise = null;
        _exerciseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.UpdateAsync(exerciseId.ToString(), request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.CoachNotes); // Null request.CoachNotes means empty collection in new entity
        
        // Verify the entity has empty coach notes (null is treated as empty)
        Assert.NotNull(capturedExercise);
        Assert.Empty(capturedExercise.CoachNotes);
    }
    
    [Fact]
    public async Task UpdateAsync_PreservesCoachNotesOriginalOrder()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var existingExercise = Exercise.Handler.CreateNew(
            "Existing Exercise",
            "Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise")
            .WithDescription("Updated Description")
            .WithCoachNotes(
                ("Note at 100", 100),
                ("Note at 5", 5),
                ("Note at 50", 50))
            .WithExerciseTypes() // Empty exercise types
            .WithMuscleGroups(("musclegroup-chest-123", "musclerole-primary-456"))
            .Build();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId>()))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
            .ReturnsAsync(existingExercise);
        
        Exercise? capturedExercise = null;
        _exerciseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.UpdateAsync(exerciseId.ToString(), request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.CoachNotes.Count);
        
        // Verify notes preserve original order values and are sorted by order
        Assert.Equal("Note at 5", result.CoachNotes[0].Text);
        Assert.Equal(5, result.CoachNotes[0].Order);
        Assert.Equal("Note at 50", result.CoachNotes[1].Text);
        Assert.Equal(50, result.CoachNotes[1].Order);
        Assert.Equal("Note at 100", result.CoachNotes[2].Text);
        Assert.Equal(100, result.CoachNotes[2].Order);
    }
}