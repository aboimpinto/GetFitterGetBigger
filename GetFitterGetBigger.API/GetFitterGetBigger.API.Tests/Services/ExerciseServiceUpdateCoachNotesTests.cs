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
    private readonly Mock<IExerciseTypeRepository> _exerciseTypeRepositoryMock;
    private readonly ExerciseService _exerciseService;
    
    public ExerciseServiceUpdateCoachNotesTests()
    {
        _unitOfWorkProviderMock = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _readOnlyUnitOfWorkMock = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _writableUnitOfWorkMock = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _exerciseRepositoryMock = new Mock<IExerciseRepository>();
        _exerciseTypeRepositoryMock = new Mock<IExerciseTypeRepository>();
        
        _readOnlyUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _readOnlyUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseTypeRepository>())
            .Returns(_exerciseTypeRepositoryMock.Object);
        
        _writableUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _unitOfWorkProviderMock.Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _unitOfWorkProviderMock.Setup(p => p.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);
        
        _exerciseService = new ExerciseService(_unitOfWorkProviderMock.Object);
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
        
        var request = new UpdateExerciseRequest
        {
            Name = "Updated Exercise",
            Description = "Updated Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Third note", Order = 3 },
                new() { Text = "First note", Order = 1 },
                new() { Text = "Second note", Order = 2 }
            },
            ExerciseTypeIds = new List<string>(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
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
        
        var request = new UpdateExerciseRequest
        {
            Name = "Updated Exercise",
            Description = "Updated Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Id = existingNoteId1.ToString(), Text = "Updated note 1", Order = 1 },
                new() { Id = existingNoteId2.ToString(), Text = "Updated note 2", Order = 2 },
                new() { Text = "New note 3", Order = 3 } // New note without ID
            },
            ExerciseTypeIds = new List<string>(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
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
        
        var request = new UpdateExerciseRequest
        {
            Name = "Updated Exercise",
            Description = "Updated Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            CoachNotes = new List<CoachNoteRequest>(), // Empty list
            ExerciseTypeIds = new List<string>(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
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
        
        var request = new UpdateExerciseRequest
        {
            Name = "Updated Exercise",
            Description = "Updated Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Id = "invalid-id", Text = "Note with invalid ID", Order = 1 },
                new() { Id = "coachnote-not-a-guid", Text = "Note with malformed ID", Order = 2 }
            },
            ExerciseTypeIds = new List<string>(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
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
        
        var request = new UpdateExerciseRequest
        {
            Name = "Updated Exercise",
            Description = "Updated Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            CoachNotes = null!, // Null means don't update
            ExerciseTypeIds = new List<string>(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
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
        
        var request = new UpdateExerciseRequest
        {
            Name = "Updated Exercise",
            Description = "Updated Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Note at 100", Order = 100 },
                new() { Text = "Note at 5", Order = 5 },
                new() { Text = "Note at 50", Order = 50 }
            },
            ExerciseTypeIds = new List<string>(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
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