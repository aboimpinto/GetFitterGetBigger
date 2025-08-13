using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Mappers;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

public class ExerciseServiceEquipmentValidationTests
{
    
    [Fact]
    public async Task CreateAsync_RestExerciseWithoutEquipment_CreatesSuccessfully()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        var restTypeId = ExerciseTypeId.New();
        
        var request = CreateExerciseRequestBuilder.ForRestExercise()
            .WithName("Rest Period")
            .WithDescription("Recovery time between sets")
            .WithExerciseTypes(restTypeId.ToString())
            .Build();
        
        var expectedResult = new ExerciseDto
        {
            Id = ExerciseId.New().ToString(),
            Name = "Rest Period",
            Description = "Recovery time between sets",
            Equipment = new List<ReferenceDataDto>()
        };
        
        automocker
            .SetupExerciseTypeService(allExist: true, isRestType: true)
            .SetupExerciseQueryDataServiceExistsByName("Rest Period", false)
            .SetupExerciseCommandDataServiceCreate(expectedResult);
        
        // Act
        var result = await testee.CreateAsync(request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue($"Create failed with errors: {string.Join(", ", result.Errors)}");
        result.Data.Name.Should().Be("Rest Period");
        result.Data.Equipment.Should().BeEmpty("Equipment is empty and that's OK for rest exercises");
    }
    
    [Fact]
    public async Task CreateAsync_NonRestExerciseWithoutEquipment_CreatesSuccessfully()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        var workoutTypeId = ExerciseTypeId.New();
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Push Up")
            .WithDescription("Bodyweight upper body exercise")
            .WithExerciseTypes(workoutTypeId.ToString())
            .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
            .WithEquipmentIds() // Empty equipment for bodyweight exercise
            .Build();
        
        var expectedResult = new ExerciseDto
        {
            Id = ExerciseId.New().ToString(),
            Name = "Push Up",
            Description = "Bodyweight upper body exercise",
            Equipment = new List<ReferenceDataDto>()
        };
        
        automocker
            .SetupExerciseTypeService(allExist: true, isRestType: false)
            .SetupExerciseQueryDataServiceExistsByName("Push Up", false)
            .SetupExerciseCommandDataServiceCreate(expectedResult);
        
        // Act
        var result = await testee.CreateAsync(request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue($"Create failed with errors: {string.Join(", ", result.Errors)}");
        result.Data.Name.Should().Be("Push Up");
        result.Data.Equipment.Should().BeEmpty("Equipment is empty and that's OK for bodyweight exercises");
    }
    
    [Fact]
    public async Task CreateAsync_ExerciseWithEquipment_CreatesSuccessfully()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        var workoutTypeId = ExerciseTypeId.New();
        var equipmentId = EquipmentId.New();
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Bench Press")
            .WithDescription("Chest exercise with barbell")
            .WithExerciseTypes(workoutTypeId.ToString())
            .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
            .Build();
        
        // Add equipment manually for this specific test
        request.EquipmentIds = new List<string> { equipmentId.ToString() };
        
        var expectedResult = new ExerciseDto
        {
            Id = ExerciseId.New().ToString(),
            Name = "Bench Press",
            Description = "Chest exercise with barbell",
            Equipment = new List<ReferenceDataDto>
            {
                new() { Id = equipmentId.ToString(), Value = "Barbell" }
            }
        };
        
        automocker
            .SetupExerciseTypeService(allExist: true, isRestType: false)
            .SetupExerciseQueryDataServiceExistsByName("Bench Press", false)
            .SetupExerciseCommandDataServiceCreate(expectedResult);
        
        // Act
        var result = await testee.CreateAsync(request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue($"Create failed with errors: {string.Join(", ", result.Errors)}");
        result.Data.Equipment.Should().HaveCount(1, "Exercise should have equipment");
        result.Data.Equipment.First().Id.Should().Be(equipmentId.ToString());
    }
    
    [Fact]
    public async Task UpdateAsync_RemoveAllEquipment_UpdatesSuccessfully()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        var exerciseId = ExerciseId.New();
        var workoutTypeId = ExerciseTypeId.New();
        
        // Update to remove all equipment
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Push Up")
            .WithDescription("Changed to bodyweight exercise")
            .WithExerciseTypes(workoutTypeId.ToString())
            .WithMuscleGroups((MuscleGroupId.New().ToString(), MuscleRoleId.New().ToString()))
            .Build();
        
        // Remove all equipment for this specific test
        request.EquipmentIds = new List<string>();
        
        var expectedResult = new ExerciseDto
        {
            Id = exerciseId.ToString(),
            Name = "Push Up",
            Description = "Changed to bodyweight exercise",
            Equipment = new List<ReferenceDataDto>() // Empty equipment after update
        };
        
        automocker
            .SetupExerciseTypeService(allExist: true, isRestType: false)
            .SetupExerciseQueryDataServiceExists(exerciseId, true)
            .SetupExerciseQueryDataServiceExistsByName("Push Up", false, exerciseId)
            .SetupExerciseCommandDataServiceUpdate(expectedResult);
        
        // Act
        var result = await testee.UpdateAsync(exerciseId, request.ToCommand());
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue($"Update failed with errors: {string.Join(", ", result.Errors)}");
        result.Data.Equipment.Should().BeEmpty("Equipment should be removed after update");
    }
}