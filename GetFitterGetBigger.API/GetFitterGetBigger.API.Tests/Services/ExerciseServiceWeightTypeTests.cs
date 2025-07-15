using System;
using System.Collections.Generic;
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
using GetFitterGetBigger.API.Constants;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Tests for Exercise service weight type functionality
/// </summary>
public class ExerciseServiceWeightTypeTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _mockWritableUnitOfWork;
    private readonly Mock<IExerciseRepository> _mockExerciseRepository;
    private readonly Mock<IExerciseTypeService> _mockExerciseTypeService;
    private readonly IExerciseService _service;
    
    public ExerciseServiceWeightTypeTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockWritableUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _mockExerciseRepository = new Mock<IExerciseRepository>();
        _mockExerciseTypeService = new Mock<IExerciseTypeService>();
        
        _mockReadOnlyUnitOfWork
            .Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_mockExerciseRepository.Object);
        
        _mockWritableUnitOfWork
            .Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_mockExerciseRepository.Object);
        
        _mockUnitOfWorkProvider
            .Setup(p => p.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
        
        _mockUnitOfWorkProvider
            .Setup(p => p.CreateWritable())
            .Returns(_mockWritableUnitOfWork.Object);
        
        // Setup default mock behaviors for ExerciseTypeService
        _mockExerciseTypeService
            .Setup(s => s.AllExistAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(true);
        
        _mockExerciseTypeService
            .Setup(s => s.AnyIsRestTypeAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(false);
        
        _mockExerciseTypeService
            .Setup(s => s.ExistsAsync(It.IsAny<ExerciseTypeId>()))
            .ReturnsAsync(true);
        
        _service = new ExerciseService(_mockUnitOfWorkProvider.Object, _mockExerciseTypeService.Object);
    }
    
    [Fact]
    public async Task CreateAsync_WithExerciseWeightTypeId_CreatesExerciseWithWeightType()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Barbell Bench Press")
            .WithDescription("Classic chest exercise")
            .WithDifficulty(DifficultyLevelTestBuilder.Beginner())
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.WeightRequired()
                .WithId(ExerciseWeightTypeId.From(Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a")))
                .WithCode("WEIGHT_REQUIRED")
                .WithValue("Weight Required")
                .WithDescription("Exercises that must have external weight specified")
                .WithDisplayOrder(3))
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build();
        
        var exerciseWeightType = ExerciseWeightType.Handler.Create(
            ExerciseWeightTypeId.From(Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a")),
            "WEIGHT_REQUIRED",
            "Weight Required",
            "Exercises that must have external weight specified",
            3,
            true
        );
        
        // Create a proper difficulty for testing
        var difficulty = DifficultyLevel.Handler.Create(
            DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b")),
            "Beginner",
            "For beginners",
            1,
            true
        ).Value;
        
        // Set up mocks
        _mockExerciseRepository.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>())).ReturnsAsync(false);
        
        Exercise? capturedExercise = null;
        _mockExerciseRepository.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => 
            {
                // Simulate the repository returning the entity with navigation properties loaded
                var exerciseType = e.GetType();
                exerciseType.GetProperty("ExerciseWeightType")?.SetValue(e, exerciseWeightType);
                exerciseType.GetProperty("Difficulty")?.SetValue(e, difficulty);
                return e;
            });
        
        // Mock unit of work commit
        _mockWritableUnitOfWork.Setup(uow => uow.CommitAsync()).Returns(Task.FromResult(true));
        
        // Act
        var result = await _service.CreateAsync(request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data.ExerciseWeightType);
        Assert.Equal("exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a", result.Data.ExerciseWeightType.Id);
        Assert.Equal("Weight Required", result.Data.ExerciseWeightType.Value);
        Assert.Equal("Exercises that must have external weight specified", result.Data.ExerciseWeightType.Description);
        
        // Verify the entity was created with the weight type ID
        Assert.NotNull(capturedExercise);
        Assert.NotNull(capturedExercise.ExerciseWeightTypeId);
        Assert.Equal(ExerciseWeightTypeId.From(Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a")), capturedExercise.ExerciseWeightTypeId);
    }
    
    [Fact]
    public async Task CreateAsync_WithoutExerciseWeightTypeId_ReturnsFailure()
    {
        // Arrange - Non-REST exercise without ExerciseWeightTypeId should fail
        // Arrange - Non-REST exercise without ExerciseWeightTypeId should fail
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Running")
            .WithDescription("Basic cardio exercise")
            .WithExerciseWeightTypeId(null) // Explicitly null to test validation
            .AddMuscleGroup(
                TestIds.MuscleGroupIds.Quadriceps,
                TestIds.MuscleRoleIds.Primary)
            .Build();
        
        // Set up mocks
        _mockExerciseRepository.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>())).ReturnsAsync(false);
        
        // Act
        var result = await _service.CreateAsync(request.ToCommand());
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(ExerciseErrorMessages.NonRestExerciseMustHaveWeightType, result.Errors);
    }
    
    [Fact]
    public async Task CreateAsync_RestExerciseWithoutWeightType_CreatesSuccessfully()
    {
        // Arrange - REST exercise without ExerciseWeightTypeId should succeed
        var request = CreateExerciseRequestBuilder.ForRestExercise()
            .WithName("Rest Period")
            .WithDescription("Recovery time between sets")
            .Build();
        
        // Create a proper difficulty for testing
        var difficulty = DifficultyLevel.Handler.Create(
            DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b")),
            "Beginner",
            "For beginners",
            1,
            true
        ).Value;
        
        // Set up mocks
        _mockExerciseRepository.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>())).ReturnsAsync(false);
        
        // Override default mock to return true for REST type
        _mockExerciseTypeService
            .Setup(s => s.AnyIsRestTypeAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(true);
        
        Exercise? capturedExercise = null;
        _mockExerciseRepository.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e)
            .ReturnsAsync((Exercise e) => e);
        
        _mockWritableUnitOfWork.Setup(uow => uow.CommitAsync()).Returns(Task.FromResult(true));
        
        // Act
        var result = await _service.CreateAsync(request.ToCommand());
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Data.ExerciseWeightType);
        
        // Verify the entity was created without weight type ID (Empty is treated as null in Null Object Pattern)
        Assert.NotNull(capturedExercise);
        Assert.True(capturedExercise.ExerciseWeightTypeId == null || capturedExercise.ExerciseWeightTypeId.Value.IsEmpty);
    }
    
    [Fact]
    public async Task CreateAsync_WithInvalidExerciseWeightTypeId_ReturnsFailure()
    {
        // Arrange
        // Arrange - Build request to test invalid ID validation
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Invalid Weight Type Exercise")
            .WithDescription("Exercise with invalid weight type")
            .WithExerciseWeightTypeId("invalid-weight-type-id") // Invalid format
            .AddMuscleGroup(
                TestIds.MuscleGroupIds.Chest,
                TestIds.MuscleRoleIds.Primary)
            .Build();
        
        // Set up mocks
        _mockExerciseRepository.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ExerciseId?>())).ReturnsAsync(false);
        
        // Act
        var result = await _service.CreateAsync(request.ToCommand());
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(ExerciseErrorMessages.NonRestExerciseMustHaveWeightType, result.Errors);
    }
    
    [Fact]
    public async Task GetByIdAsync_ReturnsExerciseWithWeightType()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exerciseWeightType = ExerciseWeightType.Handler.Create(
            ExerciseWeightTypeId.From(Guid.Parse("d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b")),
            "MACHINE_WEIGHT",
            "Machine Weight",
            "Exercises performed on machines with weight stacks",
            4,
            true
        );
        
        var difficulty = DifficultyLevel.Handler.Create(
            DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b")),
            "Beginner",
            "For beginners",
            1,
            true
        ).Value;
        
        var exercise = ExerciseBuilder.AWorkoutExercise()
            .WithId(exerciseId)
            .WithName("Lat Pulldown")
            .WithDescription("Back exercise on machine")
            .WithDifficultyId(difficulty.DifficultyLevelId)
            .WithKineticChainId(KineticChainTypeId.From(Guid.Parse("f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4")))
            .WithExerciseWeightTypeId(exerciseWeightType.Id)
            .Build();
        
        // Set the navigation properties using reflection
        var exerciseType = exercise.GetType();
        exerciseType.GetProperty("ExerciseWeightType")?.SetValue(exercise, exerciseWeightType);
        exerciseType.GetProperty("Difficulty")?.SetValue(exercise, difficulty);
        
        // Set up mocks
        _mockExerciseRepository.Setup(r => r.GetByIdAsync(exerciseId)).ReturnsAsync(exercise);
        
        // Act
        var result = await _service.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId.ToString()));
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ExerciseWeightType);
        Assert.Equal("exerciseweighttype-d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b", result.ExerciseWeightType.Id);
        Assert.Equal("Machine Weight", result.ExerciseWeightType.Value);
        Assert.Equal("Exercises performed on machines with weight stacks", result.ExerciseWeightType.Description);
    }
}