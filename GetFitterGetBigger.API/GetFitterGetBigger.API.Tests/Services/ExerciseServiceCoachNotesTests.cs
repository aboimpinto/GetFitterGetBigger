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

public class ExerciseServiceCoachNotesTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _readOnlyUnitOfWorkMock;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _writableUnitOfWorkMock;
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
    private readonly ExerciseService _exerciseService;
    
    public ExerciseServiceCoachNotesTests()
    {
        _unitOfWorkProviderMock = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _readOnlyUnitOfWorkMock = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _writableUnitOfWorkMock = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _exerciseRepositoryMock = new Mock<IExerciseRepository>();
        
        _readOnlyUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _writableUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _unitOfWorkProviderMock.Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _unitOfWorkProviderMock.Setup(p => p.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);
        
        _exerciseService = new ExerciseService(_unitOfWorkProviderMock.Object);
    }
    
    [Fact]
    public async Task CreateAsync_WithCoachNotes_CreatesExerciseWithOrderedNotes()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Exercise with Notes",
            Description = "Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Second note", Order = 2 },
                new() { Text = "First note", Order = 1 },
                new() { Text = "Third note", Order = 3 }
            },
            ExerciseTypeIds = new List<string>(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        Exercise? capturedExercise = null;
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.CoachNotes.Count);
        Assert.Equal("First note", result.CoachNotes[0].Text);
        Assert.Equal(1, result.CoachNotes[0].Order);
        Assert.Equal("Second note", result.CoachNotes[1].Text);
        Assert.Equal(2, result.CoachNotes[1].Order);
        Assert.Equal("Third note", result.CoachNotes[2].Text);
        Assert.Equal(3, result.CoachNotes[2].Order);
        
        // Verify the entity was created with coach notes
        Assert.NotNull(capturedExercise);
        Assert.Equal(3, capturedExercise.CoachNotes.Count);
    }
    
    [Fact]
    public async Task CreateAsync_WithExerciseTypes_CreatesExerciseWithTypes()
    {
        // Arrange
        var exerciseTypeId1 = ExerciseTypeId.New();
        var exerciseTypeId2 = ExerciseTypeId.New();
        
        var request = new CreateExerciseRequest
        {
            Name = "Exercise with Types",
            Description = "Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string>
            {
                exerciseTypeId1.ToString(),
                exerciseTypeId2.ToString()
            },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        Exercise? capturedExercise = null;
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.ExerciseTypes.Count);
        
        // Verify the entity was created with exercise types
        Assert.NotNull(capturedExercise);
        Assert.Equal(2, capturedExercise.ExerciseExerciseTypes.Count);
        Assert.Contains(capturedExercise.ExerciseExerciseTypes, eet => eet.ExerciseTypeId == exerciseTypeId1);
        Assert.Contains(capturedExercise.ExerciseExerciseTypes, eet => eet.ExerciseTypeId == exerciseTypeId2);
    }
    
    [Fact]
    public async Task CreateAsync_WithEmptyCoachNotes_CreatesExerciseWithoutNotes()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Exercise without Notes",
            Description = "Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            CoachNotes = new List<CoachNoteRequest>(),
            ExerciseTypeIds = new List<string>(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.CoachNotes);
    }
    
    [Fact]
    public async Task CreateAsync_WithInvalidExerciseTypeId_IgnoresInvalidId()
    {
        // Arrange
        var validId = ExerciseTypeId.New();
        
        var request = new CreateExerciseRequest
        {
            Name = "Exercise with Invalid Type",
            Description = "Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            ExerciseTypeIds = new List<string>
            {
                validId.ToString(),
                "invalid-id",
                "exercisetype-not-a-guid"
            },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        Exercise? capturedExercise = null;
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result.ExerciseTypes); // Only the valid ID should be processed
        
        // Verify only valid ID was added to entity
        Assert.NotNull(capturedExercise);
        Assert.Single(capturedExercise.ExerciseExerciseTypes);
        Assert.Equal(validId, capturedExercise.ExerciseExerciseTypes.First().ExerciseTypeId);
    }
    
    [Fact]
    public async Task CreateAsync_WithCoachNotesReordersSequentially()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Exercise with Gaps",
            Description = "Description",
            DifficultyId = DifficultyLevelId.New().ToString(),
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Note at 10", Order = 10 },
                new() { Text = "Note at 5", Order = 5 },
                new() { Text = "Note at 20", Order = 20 }
            },
            ExerciseTypeIds = new List<string>(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(),
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        Exercise? capturedExercise = null;
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => e);
        
        _writableUnitOfWorkMock.Setup(uow => uow.CommitAsync())
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _exerciseService.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.CoachNotes.Count);
        
        // Verify notes are reordered sequentially starting from 1
        Assert.Equal("Note at 5", result.CoachNotes[0].Text);
        Assert.Equal(1, result.CoachNotes[0].Order);
        Assert.Equal("Note at 10", result.CoachNotes[1].Text);
        Assert.Equal(2, result.CoachNotes[1].Order);
        Assert.Equal("Note at 20", result.CoachNotes[2].Text);
        Assert.Equal(3, result.CoachNotes[2].Order);
    }
}