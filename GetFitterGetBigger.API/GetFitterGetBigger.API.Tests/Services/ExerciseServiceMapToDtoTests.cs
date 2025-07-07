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

public class ExerciseServiceMapToDtoTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _readOnlyUnitOfWorkMock;
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
    private readonly Mock<IExerciseTypeRepository> _exerciseTypeRepositoryMock;
    private readonly ExerciseService _exerciseService;
    
    public ExerciseServiceMapToDtoTests()
    {
        _unitOfWorkProviderMock = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _readOnlyUnitOfWorkMock = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _exerciseRepositoryMock = new Mock<IExerciseRepository>();
        _exerciseTypeRepositoryMock = new Mock<IExerciseTypeRepository>();
        
        _readOnlyUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_exerciseRepositoryMock.Object);
        
        _readOnlyUnitOfWorkMock.Setup(uow => uow.GetRepository<IExerciseTypeRepository>())
            .Returns(_exerciseTypeRepositoryMock.Object);
        
        _unitOfWorkProviderMock.Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
        
        _exerciseService = new ExerciseService(_unitOfWorkProviderMock.Object);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithKineticChain_MapsKineticChainCorrectly()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var difficultyId = DifficultyLevelId.New();
        var kineticChainId = KineticChainTypeId.New();
        
        var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Intermediate", "Medium difficulty", 3);
        var kineticChain = KineticChainType.Handler.Create(kineticChainId, "Open Chain", "Open kinetic chain movement", 1);
        
        var exercise = Exercise.Handler.CreateNew(
            "Test Exercise",
            "Test Description",
            null,
            null,
            false,
            difficultyId,
            kineticChainId);
            
        // Use reflection to set navigation properties for testing
        typeof(Exercise).GetProperty(nameof(Exercise.Difficulty))!.SetValue(exercise, difficulty);
        typeof(Exercise).GetProperty(nameof(Exercise.KineticChain))!.SetValue(exercise, kineticChain);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);
        
        // Act
        var result = await _exerciseService.GetByIdAsync(exerciseId.ToString());
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.KineticChain);
        Assert.Equal(kineticChainId.ToString(), result.KineticChain.Id);
        Assert.Equal("Open Chain", result.KineticChain.Value);
        Assert.Equal("Open kinetic chain movement", result.KineticChain.Description);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithoutKineticChain_MapsKineticChainAsNull()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var difficultyId = DifficultyLevelId.New();
        
        var difficulty = DifficultyLevel.Handler.Create(difficultyId, "Beginner", "Easy difficulty", 1);
        
        var exercise = Exercise.Handler.CreateNew(
            "Rest Exercise",
            "Rest period",
            null,
            null,
            false,
            difficultyId,
            null); // No kinetic chain
            
        // Use reflection to set navigation properties for testing
        typeof(Exercise).GetProperty(nameof(Exercise.Difficulty))!.SetValue(exercise, difficulty);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);
        
        // Act
        var result = await _exerciseService.GetByIdAsync(exerciseId.ToString());
        
        // Assert
        Assert.NotNull(result);
        Assert.Null(result.KineticChain);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithCoachNotes_MapsCoachNotesCorrectly()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exercise = Exercise.Handler.CreateNew(
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
        
        // Add them out of order to test ordering
        exercise.CoachNotes.Add(note3);
        exercise.CoachNotes.Add(note1);
        exercise.CoachNotes.Add(note2);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);
        
        // Act
        var result = await _exerciseService.GetByIdAsync(exerciseId.ToString());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.CoachNotes.Count);
        
        // Verify ordering
        Assert.Equal(note1.Id.ToString(), result.CoachNotes[0].Id);
        Assert.Equal("First note", result.CoachNotes[0].Text);
        Assert.Equal(1, result.CoachNotes[0].Order);
        
        Assert.Equal(note2.Id.ToString(), result.CoachNotes[1].Id);
        Assert.Equal("Second note", result.CoachNotes[1].Text);
        Assert.Equal(2, result.CoachNotes[1].Order);
        
        Assert.Equal(note3.Id.ToString(), result.CoachNotes[2].Id);
        Assert.Equal("Third note", result.CoachNotes[2].Text);
        Assert.Equal(3, result.CoachNotes[2].Order);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithExerciseTypes_MapsExerciseTypesCorrectly()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exercise = Exercise.Handler.CreateNew(
            "Test Exercise",
            "Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        // Create exercise types
        var warmupType = ExerciseType.Handler.CreateNew("Warmup", "Warmup exercises", 1);
        var workoutType = ExerciseType.Handler.CreateNew("Workout", "Main workout", 2);
        
        // Add exercise types through junction table
        var eet1 = ExerciseExerciseType.Handler.Create(exerciseId, warmupType.Id);
        exercise.ExerciseExerciseTypes.Add(eet1);
        // Manually set navigation property for testing
        eet1.GetType().GetProperty("ExerciseType")?.SetValue(eet1, warmupType);
        
        var eet2 = ExerciseExerciseType.Handler.Create(exerciseId, workoutType.Id);
        exercise.ExerciseExerciseTypes.Add(eet2);
        // Manually set navigation property for testing
        eet2.GetType().GetProperty("ExerciseType")?.SetValue(eet2, workoutType);
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);
        
        // Act
        var result = await _exerciseService.GetByIdAsync(exerciseId.ToString());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.ExerciseTypes.Count);
        
        var warmupDto = result.ExerciseTypes.FirstOrDefault(et => et.Value == "Warmup");
        Assert.NotNull(warmupDto);
        Assert.Equal(warmupType.Id.ToString(), warmupDto.Id);
        Assert.Equal("Warmup exercises", warmupDto.Description);
        
        var workoutDto = result.ExerciseTypes.FirstOrDefault(et => et.Value == "Workout");
        Assert.NotNull(workoutDto);
        Assert.Equal(workoutType.Id.ToString(), workoutDto.Id);
        Assert.Equal("Main workout", workoutDto.Description);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyCoachNotesAndTypes_ReturnsEmptyCollections()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exercise = Exercise.Handler.CreateNew(
            "Test Exercise",
            "Description",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);
        
        // Act
        var result = await _exerciseService.GetByIdAsync(exerciseId.ToString());
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.CoachNotes);
        Assert.Empty(result.ExerciseTypes);
    }
    
    [Fact]
    public async Task GetPagedAsync_WithCoachNotesAndTypes_MapsAllCorrectly()
    {
        // Arrange
        var exercise1 = Exercise.Handler.CreateNew(
            "Exercise 1",
            "Description 1",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        var exercise2 = Exercise.Handler.CreateNew(
            "Exercise 2",
            "Description 2",
            null,
            null,
            false,
            DifficultyLevelId.New());
        
        // Add coach notes to exercise1
        exercise1.CoachNotes.Add(CoachNote.Handler.CreateNew(exercise1.Id, "Note 1", 1));
        exercise1.CoachNotes.Add(CoachNote.Handler.CreateNew(exercise1.Id, "Note 2", 2));
        
        // Add exercise types to exercise2
        var exerciseType = ExerciseType.Handler.CreateNew("Cooldown", "Cooldown exercises", 3);
        var eet = ExerciseExerciseType.Handler.Create(exercise2.Id, exerciseType.Id);
        exercise2.ExerciseExerciseTypes.Add(eet);
        // Manually set navigation property for testing
        eet.GetType().GetProperty("ExerciseType")?.SetValue(eet, exerciseType);
        
        var exercises = new List<Exercise> { exercise1, exercise2 };
        
        _exerciseRepositoryMock.Setup(r => r.GetPagedAsync(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<DifficultyLevelId?>(),
            It.IsAny<IEnumerable<MuscleGroupId>?>(),
            It.IsAny<IEnumerable<EquipmentId>?>(),
            It.IsAny<IEnumerable<MovementPatternId>?>(),
            It.IsAny<IEnumerable<BodyPartId>?>(),
            It.IsAny<bool>()))
            .ReturnsAsync((exercises, 2));
        
        var filterParams = new ExerciseFilterParams
        {
            Page = 1,
            PageSize = 10
        };
        
        // Act
        var result = await _exerciseService.GetPagedAsync(filterParams);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        
        // Check exercise1 has coach notes
        var dto1 = result.Items.First(d => d.Name == "Exercise 1");
        Assert.Equal(2, dto1.CoachNotes.Count);
        Assert.Empty(dto1.ExerciseTypes);
        
        // Check exercise2 has exercise types
        var dto2 = result.Items.First(d => d.Name == "Exercise 2");
        Assert.Empty(dto2.CoachNotes);
        Assert.Single(dto2.ExerciseTypes);
        Assert.Equal("Cooldown", dto2.ExerciseTypes[0].Value);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithNullExerciseType_HandlesGracefully()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exercise = Exercise.Handler.CreateNew(
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
        
        _exerciseRepositoryMock.Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);
        
        // Act
        var result = await _exerciseService.GetByIdAsync(exerciseId.ToString());
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result.ExerciseTypes);
        Assert.Empty(result.ExerciseTypes[0].Value); // Should handle null gracefully
        Assert.Null(result.ExerciseTypes[0].Description);
    }
}