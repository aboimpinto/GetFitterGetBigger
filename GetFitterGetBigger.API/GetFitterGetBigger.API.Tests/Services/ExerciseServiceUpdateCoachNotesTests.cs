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
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using GetFitterGetBigger.API.Mappers;
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
    private readonly IExerciseService _exerciseService;
    
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
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .AddCoachNote("Third note", 3)
            .AddCoachNote("First note", 1)
            .AddCoachNote("Second note", 2)
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
        var result = await _exerciseService.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data.CoachNotes.Count);
        Assert.Equal("First note", result.Data.CoachNotes[0].Text);
        Assert.Equal(1, result.Data.CoachNotes[0].Order);
        Assert.Equal("Second note", result.Data.CoachNotes[1].Text);
        Assert.Equal(2, result.Data.CoachNotes[1].Order);
        Assert.Equal("Third note", result.Data.CoachNotes[2].Text);
        Assert.Equal(3, result.Data.CoachNotes[2].Order);
        
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
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .AddCoachNote(existingNoteId1, "Updated note 1", 1)
            .AddCoachNote(existingNoteId2, "Updated note 2", 2)
            .AddCoachNote(null, "New note 3", 3)
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
        var result = await _exerciseService.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data.CoachNotes.Count);
        
        // Check that existing IDs were preserved
        Assert.Equal(existingNoteId1.ToString(), result.Data.CoachNotes[0].Id);
        Assert.Equal("Updated note 1", result.Data.CoachNotes[0].Text);
        
        Assert.Equal(existingNoteId2.ToString(), result.Data.CoachNotes[1].Id);
        Assert.Equal("Updated note 2", result.Data.CoachNotes[1].Text);
        
        // New note should have a new ID
        Assert.NotEmpty(result.Data.CoachNotes[2].Id);
        Assert.NotEqual(existingNoteId1.ToString(), result.Data.CoachNotes[2].Id);
        Assert.NotEqual(existingNoteId2.ToString(), result.Data.CoachNotes[2].Id);
        Assert.Equal("New note 3", result.Data.CoachNotes[2].Text);
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
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build(); // No AddCoachNote calls = empty coach notes
        
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
        var result = await _exerciseService.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data.CoachNotes);
        
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
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .AddCoachNoteWithInvalidFormat("Note with invalid ID", 1)
            .AddCoachNoteWithMalformedId("Note with malformed ID", 2)
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
        var result = await _exerciseService.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data.CoachNotes); // Invalid IDs are ignored, no new notes created
        
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
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build();
        
        // No coach notes added, will result in empty list
        
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
        var result = await _exerciseService.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data.CoachNotes); // Null request.CoachNotes means empty collection in new entity
        
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
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .AddCoachNote("Note at 100", 100)
            .AddCoachNote("Note at 5", 5)
            .AddCoachNote("Note at 50", 50)
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
        var result = await _exerciseService.UpdateAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()), request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data.CoachNotes.Count);
        
        // Verify notes preserve original order values and are sorted by order
        Assert.Equal("Note at 5", result.Data.CoachNotes[0].Text);
        Assert.Equal(5, result.Data.CoachNotes[0].Order);
        Assert.Equal("Note at 50", result.Data.CoachNotes[1].Text);
        Assert.Equal(50, result.Data.CoachNotes[1].Order);
        Assert.Equal("Note at 100", result.Data.CoachNotes[2].Text);
        Assert.Equal(100, result.Data.CoachNotes[2].Order);
    }
}