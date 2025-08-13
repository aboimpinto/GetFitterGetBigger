using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Tests.TestBuilders.ServiceCommands;
using GetFitterGetBigger.API.Constants;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

public class ExerciseServiceMuscleGroupValidationTests
{
    [Fact]
    public async Task CreateAsync_RestExerciseWithoutMuscleGroups_CreatesSuccessfully()
    {
        // Arrange
        const string exerciseName = "Rest Period";
        
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var command = CreateExerciseCommandBuilder.ForRestExercise()
            .WithName(exerciseName)
            .WithMuscleGroups() // Empty muscle groups for REST
            .Build();
        
        var expectedDto = new ExerciseDto 
        {
            Name = exerciseName,
            MuscleGroups = []
        };
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: true)
            .SetupExerciseQueryDataServiceExistsByName(exerciseName, false)
            .SetupExerciseCommandDataServiceCreate(expectedDto);
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Name.Should().Be(exerciseName);
        result.Data.MuscleGroups.Should().BeEmpty();
    }
    
    [Fact]
    public async Task CreateAsync_NonRestExerciseWithoutMuscleGroups_ReturnsValidationError()
    {
        // Arrange
        const string exerciseName = "Push Up";
        
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var command = CreateExerciseCommandBuilder.ForWorkoutExercise()
            .WithName(exerciseName)
            .WithMuscleGroups() // Empty muscle groups for non-REST exercise
            .Build();
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: false)
            .SetupExerciseQueryDataServiceExistsByName(exerciseName, false);
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
    }
    
    [Fact]
    public async Task CreateAsync_RestExerciseWithMuscleGroups_ReturnsValidationError()
    {
        // Arrange
        const string exerciseName = "Rest Period";
        
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var primaryMuscleId = MuscleGroupId.New();
        var primaryRoleId = MuscleRoleId.New();
        
        var command = CreateExerciseCommandBuilder.ForRestExercise()
            .WithName(exerciseName)
            .WithMuscleGroups((primaryMuscleId, primaryRoleId)) // REST with muscle groups - invalid
            .Build();
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: true)
            .SetupExerciseQueryDataServiceExistsByName(exerciseName, false);
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
    }
    
    [Fact]
    public async Task UpdateAsync_RestExerciseWithoutMuscleGroups_UpdatesSuccessfully()
    {
        // Arrange
        const string updatedName = "Rest Period Updated";
        var exerciseId = ExerciseId.New();
        
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var command = UpdateExerciseCommandBuilder.ForRestExercise()
            .WithName(updatedName)
            .WithMuscleGroups() // Empty muscle groups for REST
            .Build();
        
        var expectedDto = new ExerciseDto 
        {
            Name = updatedName,
            MuscleGroups = []
        };
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: true)
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceExistsByName(updatedName, false, exerciseId)
            .SetupExerciseCommandDataServiceUpdate(expectedDto);
        
        // Act
        var result = await testee.UpdateAsync(exerciseId, command);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Name.Should().Be(updatedName);
        result.Data.MuscleGroups.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateAsync_NonRestExerciseWithoutMuscleGroups_ReturnsValidationError()
    {
        // Arrange
        const string updatedName = "Push Up Updated";
        var exerciseId = ExerciseId.New();
        
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var command = UpdateExerciseCommandBuilder.ForWorkoutExercise()
            .WithName(updatedName)
            .WithMuscleGroups() // Empty muscle groups for non-REST exercise
            .Build();
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: false)
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceExistsByName(updatedName, false, exerciseId);
        
        // Act
        var result = await testee.UpdateAsync(exerciseId, command);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
    }
    
    [Theory]
    [InlineData("rest")]
    [InlineData("Rest")]
    [InlineData("REST")]
    [InlineData("rEsT")]
    public async Task CreateAsync_RestExerciseWithVariousCapitalization_CreatesSuccessfully(string restValue)
    {
        // Arrange
        var exerciseName = $"{restValue} Period";
        
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var command = CreateExerciseCommandBuilder.ForRestExercise()
            .WithName(exerciseName)
            .WithMuscleGroups() // Empty muscle groups for REST
            .Build();
        
        var expectedDto = new ExerciseDto 
        {
            Name = exerciseName,
            MuscleGroups = []
        };
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: true)
            .SetupExerciseQueryDataServiceExistsByName(exerciseName, false)
            .SetupExerciseCommandDataServiceCreate(expectedDto);
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Name.Should().Be(exerciseName);
        result.Data.MuscleGroups.Should().BeEmpty();
    }
    
    [Fact]
    public async Task CreateAsync_NoExerciseTypesWithEmptyMuscleGroups_CreatesSuccessfully()
    {
        // Arrange
        const string exerciseName = "Generic Exercise";
        
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var command = CreateExerciseCommandBuilder.ForWorkoutExercise()
            .WithName(exerciseName)
            .WithExerciseTypes() // No exercise types
            .WithMuscleGroups() // Empty muscle groups
            .Build();
        
        var expectedDto = new ExerciseDto 
        {
            Name = exerciseName,
            MuscleGroups = []
        };
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: false)
            .SetupExerciseQueryDataServiceExistsByName(exerciseName, false)
            .SetupExerciseCommandDataServiceCreate(expectedDto);
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBe(ExerciseDto.Empty);
    }
    
    [Fact]
    public async Task CreateAsync_MultipleTypesIncludingRest_ReturnsValidationError()
    {
        // Arrange - REST can't be combined with other types (REST exclusivity rule)
        const string exerciseName = "Invalid Exercise";
        
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var restTypeId = ExerciseTypeId.New();
        var workoutTypeId = ExerciseTypeId.New();
        
        var command = CreateExerciseCommandBuilder.ForWorkoutExercise()
            .WithName(exerciseName)
            .WithExerciseTypes(restTypeId, workoutTypeId) // Multiple types including REST
            .WithMuscleGroups() // Empty muscle groups
            .Build();
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: true)
            .SetupExerciseQueryDataServiceExistsByName(exerciseName, false);
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
    }
    
    [Fact]
    public async Task CreateAsync_NonRestExerciseWithPrimaryMuscleGroup_CreatesSuccessfully()
    {
        // Arrange
        const string exerciseName = "Bench Press";
        
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var primaryMuscleId = MuscleGroupId.New();
        var primaryRoleId = MuscleRoleId.New();
        
        var command = CreateExerciseCommandBuilder.ForWorkoutExercise()
            .WithName(exerciseName)
            .WithMuscleGroups((primaryMuscleId, primaryRoleId))
            .Build();
        
        var expectedDto = new ExerciseDto 
        {
            Name = exerciseName,
            MuscleGroups = 
            [
                new MuscleGroupWithRoleDto
                {
                    MuscleGroup = new ReferenceDataDto { Id = primaryMuscleId.ToString() },
                    Role = new ReferenceDataDto { Id = primaryRoleId.ToString() }
                }
            ]
        };
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: false)
            .SetupExerciseQueryDataServiceExistsByName(exerciseName, false)
            .SetupExerciseCommandDataServiceCreate(expectedDto);
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.MuscleGroups.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task CreateAsync_NonRestExerciseWithMultipleMuscleGroups_CreatesSuccessfully()
    {
        // Arrange
        const string exerciseName = "Squat";
        
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseService>();
        
        var primaryMuscleId = MuscleGroupId.New();
        var primaryRoleId = MuscleRoleId.New();
        var secondaryMuscleId = MuscleGroupId.New();
        var secondaryRoleId = MuscleRoleId.New();
        
        var command = CreateExerciseCommandBuilder.ForWorkoutExercise()
            .WithName(exerciseName)
            .WithMuscleGroups(
                (primaryMuscleId, primaryRoleId),
                (secondaryMuscleId, secondaryRoleId)
            )
            .Build();
        
        var expectedDto = new ExerciseDto 
        {
            Name = exerciseName,
            MuscleGroups = 
            [
                new MuscleGroupWithRoleDto
                {
                    MuscleGroup = new ReferenceDataDto { Id = primaryMuscleId.ToString() },
                    Role = new ReferenceDataDto { Id = primaryRoleId.ToString() }
                },
                new MuscleGroupWithRoleDto
                {
                    MuscleGroup = new ReferenceDataDto { Id = secondaryMuscleId.ToString() },
                    Role = new ReferenceDataDto { Id = secondaryRoleId.ToString() }
                }
            ]
        };
        
        autoMocker
            .SetupExerciseTypeService(allExist: true, isRestType: false)
            .SetupExerciseQueryDataServiceExistsByName(exerciseName, false)
            .SetupExerciseCommandDataServiceCreate(expectedDto);
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.MuscleGroups.Should().HaveCount(2);
    }
}