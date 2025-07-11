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

public class ExerciseServiceCoachNotesTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _readOnlyUnitOfWorkMock;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _writableUnitOfWorkMock;
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
    private readonly Mock<IExerciseTypeService> _mockExerciseTypeService;
    private readonly ExerciseService _exerciseService;
    
    public ExerciseServiceCoachNotesTests()
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
    public async Task CreateAsync_WithCoachNotes_CreatesExerciseWithOrderedNotes()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise with Notes")
            .WithDescription("Description")
            .WithCoachNotes(("Second note", 2), ("First note", 1), ("Third note", 3))
            .Build();
        
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
        
        var exerciseType1 = ExerciseType.Handler.Create(exerciseTypeId1, "Type1", "Description1", 1, false);
        var exerciseType2 = ExerciseType.Handler.Create(exerciseTypeId2, "Type2", "Description2", 2, false);
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise with Types")
            .WithDescription("Description")
            .WithExerciseTypes(exerciseTypeId1.ToString(), exerciseTypeId2.ToString())
            .Build();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        // Override the default mock to return false for non-REST types
        _mockExerciseTypeService
            .Setup(s => s.AnyIsRestTypeAsync(It.Is<IEnumerable<string>>(ids => 
                ids.Contains(exerciseTypeId1.ToString()) || ids.Contains(exerciseTypeId2.ToString()))))
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
        // Since we're testing that the service creates the exercise with types,
        // we verify that the entity was created correctly
        Assert.NotNull(capturedExercise);
        Assert.Equal(2, capturedExercise.ExerciseExerciseTypes.Count);
        Assert.Contains(capturedExercise.ExerciseExerciseTypes, eet => eet.ExerciseTypeId == exerciseTypeId1);
        Assert.Contains(capturedExercise.ExerciseExerciseTypes, eet => eet.ExerciseTypeId == exerciseTypeId2);
    }
    
    [Fact]
    public async Task CreateAsync_WithEmptyCoachNotes_CreatesExerciseWithoutNotes()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise without Notes")
            .WithDescription("Description")
            .WithCoachNotes() // Empty coach notes
            .Build();
        
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
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise with Invalid Type")
            .WithDescription("Description")
            .WithExerciseTypes(validId.ToString(), "invalid-id", "exercisetype-not-a-guid")
            .Build();
        
        _exerciseRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        
        // Override the default mock to return false for non-REST type
        _mockExerciseTypeService
            .Setup(s => s.AnyIsRestTypeAsync(It.Is<IEnumerable<string>>(ids => 
                ids.Contains(validId.ToString()))))
            .ReturnsAsync(false);
            
        // Mock ExistsAsync to return true only for the valid ID
        _mockExerciseTypeService
            .Setup(s => s.ExistsAsync(validId))
            .ReturnsAsync(true);
        
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
    public async Task CreateAsync_WithCoachNotesPreservesOriginalOrder()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise with Gaps")
            .WithDescription("Description")
            .WithCoachNotes(("Note at 10", 10), ("Note at 5", 5), ("Note at 20", 20))
            .Build();
        
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
        
        // Verify notes preserve original order values and are sorted by order
        Assert.Equal("Note at 5", result.CoachNotes[0].Text);
        Assert.Equal(5, result.CoachNotes[0].Order);
        Assert.Equal("Note at 10", result.CoachNotes[1].Text);
        Assert.Equal(10, result.CoachNotes[1].Order);
        Assert.Equal("Note at 20", result.CoachNotes[2].Text);
        Assert.Equal(20, result.CoachNotes[2].Order);
    }
}