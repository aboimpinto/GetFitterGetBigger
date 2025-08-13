using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Mappers;
using GetFitterGetBigger.API.Constants;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Tests for Exercise service weight type functionality
/// </summary>
public class ExerciseServiceWeightTypeTests
{
    
    [Fact]
    public async Task CreateAsync_WithExerciseWeightTypeId_CreatesExerciseWithWeightType()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
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
            
        var expectedDto = new ExerciseDto
        {
            Id = "exercise-" + Guid.NewGuid(),
            Name = "Barbell Bench Press",
            Description = "Classic chest exercise",
            IsActive = true,
            IsUnilateral = false,
            ExerciseWeightType = new ReferenceDataDto
            {
                Id = "exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a",
                Value = "Weight Required",
                Description = "Exercises that must have external weight specified"
            }
        };
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: false)
            .SetupExerciseQueryDataServiceExistsByName("Barbell Bench Press", false)
            .SetupExerciseCommandDataServiceCreate(expectedDto);
        
        // Act
        var result = await testee.CreateAsync(request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.ExerciseWeightType.Should().NotBeNull();
        result.Data.ExerciseWeightType.Id.Should().Be("exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a");
        result.Data.ExerciseWeightType.Value.Should().Be("Weight Required");
        result.Data.ExerciseWeightType.Description.Should().Be("Exercises that must have external weight specified");
    }
    
    [Fact]
    public async Task CreateAsync_WithoutExerciseWeightTypeId_Succeeds()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Running")
            .WithDescription("Basic cardio exercise")
            .WithExerciseWeightTypeId(null)
            .AddMuscleGroup(
                TestIds.MuscleGroupIds.Quadriceps,
                TestIds.MuscleRoleIds.Primary)
            .Build();
            
        var expectedDto = new ExerciseDto
        {
            Id = "exercise-" + Guid.NewGuid(),
            Name = "Running",
            Description = "Basic cardio exercise",
            IsActive = true,
            IsUnilateral = false
        };
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: false)
            .SetupExerciseQueryDataServiceExistsByName("Running", false)
            .SetupExerciseCommandDataServiceCreate(expectedDto);
        
        // Act
        var result = await testee.CreateAsync(request.ToCommand());
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.IsEmpty.Should().BeFalse();
    }
    
    [Fact]
    public async Task CreateAsync_RestExerciseWithoutWeightType_CreatesSuccessfully()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var request = CreateExerciseRequestBuilder.ForRestExercise()
            .WithName("Rest Period")
            .WithDescription("Recovery time between sets")
            .Build();
            
        var expectedDto = new ExerciseDto
        {
            Id = "exercise-" + Guid.NewGuid(),
            Name = "Rest Period",
            Description = "Recovery time between sets",
            IsActive = true,
            IsUnilateral = false
        };
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: true)
            .SetupExerciseQueryDataServiceExistsByName("Rest Period", false)
            .SetupExerciseCommandDataServiceCreate(expectedDto);
        
        // Act
        var result = await testee.CreateAsync(request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.ExerciseWeightType.Should().BeNull();
    }
    
    [Fact]
    public async Task CreateAsync_WithInvalidExerciseWeightTypeId_Succeeds()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Invalid Weight Type Exercise")
            .WithDescription("Exercise with invalid weight type")
            .WithExerciseWeightTypeId("invalid-weight-type-id")
            .AddMuscleGroup(
                TestIds.MuscleGroupIds.Chest,
                TestIds.MuscleRoleIds.Primary)
            .Build();
            
        var expectedDto = new ExerciseDto
        {
            Id = "exercise-" + Guid.NewGuid(),
            Name = "Invalid Weight Type Exercise",
            Description = "Exercise with invalid weight type",
            IsActive = true,
            IsUnilateral = false
        };
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: false)
            .SetupExerciseQueryDataServiceExistsByName("Invalid Weight Type Exercise", false)
            .SetupExerciseCommandDataServiceCreate(expectedDto);
        
        // Act
        var result = await testee.CreateAsync(request.ToCommand());
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.IsEmpty.Should().BeFalse();
    }
    
    [Fact]
    public async Task GetByIdAsync_ReturnsExerciseWithWeightType()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var exerciseId = ExerciseId.New();
        
        var expectedDto = new ExerciseDto
        {
            Id = exerciseId.ToString(),
            Name = "Lat Pulldown",
            Description = "Back exercise on machine",
            IsActive = true,
            IsUnilateral = false,
            ExerciseWeightType = new ReferenceDataDto
            {
                Id = "exerciseweighttype-d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b",
                Value = "Machine Weight",
                Description = "Exercises performed on machines with weight stacks"
            }
        };
        
        autoMocker
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceGetById(expectedDto)
            .SetupExerciseTypeService();
        
        // Act
        var result = await testee.GetByIdAsync(exerciseId);
        
        // Assert
        result.Should().NotBeNull();
        result.Data.ExerciseWeightType.Should().NotBeNull();
        result.Data.ExerciseWeightType.Id.Should().Be("exerciseweighttype-d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b");
        result.Data.ExerciseWeightType.Value.Should().Be("Machine Weight");
        result.Data.ExerciseWeightType.Description.Should().Be("Exercises performed on machines with weight stacks");
    }
}