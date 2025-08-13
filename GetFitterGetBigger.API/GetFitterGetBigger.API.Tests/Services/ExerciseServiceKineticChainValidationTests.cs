using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Exercise;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using GetFitterGetBigger.API.Tests.TestBuilders.ServiceCommands;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Constants;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class ExerciseServiceKineticChainValidationTests
    {
        // Reference data IDs
        private readonly DifficultyLevelId _difficultyId = DifficultyLevelId.New();
        private readonly ExerciseTypeId _restTypeId = ExerciseTypeId.New();
        private readonly ExerciseTypeId _strengthTypeId = ExerciseTypeId.New();
        private readonly KineticChainTypeId _kineticChainId = KineticChainTypeId.New();

        [Fact]
        public async Task CreateAsync_NonRestExerciseWithoutKineticChain_ShouldReturnFailure()
        {
            // Arrange
            var autoMocker = new AutoMocker();
            var testee = autoMocker.CreateInstance<ExerciseService>();
            
            var command = CreateExerciseCommandBuilder.ForWorkoutExercise()
                .WithName("Test Exercise")
                .WithDescription("Test Description")
                .WithDifficultyId(_difficultyId)
                .WithExerciseTypes(_strengthTypeId)
                .WithKineticChainId(null) // No kinetic chain provided
                .WithMuscleGroups((MuscleGroupId.New(), MuscleRoleId.New()))
                .Build();

            // Setup mocks - exercise name doesn't exist, exercise types exist, not REST type
            autoMocker.SetupExerciseQueryDataServiceExistsByName(command.Name, false);
            autoMocker.SetupExerciseTypeService(allExist: true, isRestType: false);

            // Act
            var result = await testee.CreateAsync(command);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        }

        [Fact]
        public async Task CreateAsync_RestExerciseWithKineticChain_ShouldReturnFailure()
        {
            // Arrange
            var autoMocker = new AutoMocker();
            var testee = autoMocker.CreateInstance<ExerciseService>();
            
            var command = CreateExerciseCommandBuilder.ForRestExercise()
                .WithName("Rest Period")
                .WithDescription("Rest between sets")
                .WithDifficultyId(_difficultyId)
                .WithExerciseTypes(_restTypeId)
                .WithKineticChainId(_kineticChainId) // Kinetic chain provided for rest
                .Build();

            // Setup mocks - exercise name doesn't exist, exercise types exist, is REST type
            autoMocker.SetupExerciseQueryDataServiceExistsByName(command.Name, false);
            autoMocker.SetupExerciseTypeService(allExist: true, isRestType: true);

            // Act
            var result = await testee.CreateAsync(command);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        }

        [Fact]
        public async Task CreateAsync_NonRestExerciseWithKineticChain_ShouldSucceed()
        {
            // Arrange
            var autoMocker = new AutoMocker();
            var testee = autoMocker.CreateInstance<ExerciseService>();
            
            var exerciseDto = new ExerciseDto
            {
                Id = ExerciseId.New().ToString(),
                Name = "Test Exercise",
                Description = "Test Description",
                Difficulty = new ReferenceDataDto { Id = _difficultyId.ToString(), Value = "Test Difficulty" },
                KineticChain = new ReferenceDataDto { Id = _kineticChainId.ToString(), Value = "Test Kinetic Chain" }
            };
            
            var command = CreateExerciseCommandBuilder.ForWorkoutExercise()
                .WithName("Test Exercise")
                .WithDescription("Test Description")
                .WithDifficultyId(_difficultyId)
                .WithExerciseTypes(_strengthTypeId)
                .WithKineticChainId(_kineticChainId)
                .WithMuscleGroups((MuscleGroupId.New(), MuscleRoleId.New()))
                .Build();

            // Setup mocks - exercise name doesn't exist, exercise types exist, not REST type, successful create
            autoMocker.SetupExerciseQueryDataServiceExistsByName(command.Name, false);
            autoMocker.SetupExerciseTypeService(allExist: true, isRestType: false);
            autoMocker.SetupExerciseCommandDataServiceCreate(exerciseDto);

            // Act
            var result = await testee.CreateAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.KineticChain.Should().NotBeNull();
            result.Data.KineticChain!.Id.Should().Be(_kineticChainId.ToString());
        }

        [Fact]
        public async Task CreateAsync_RestExerciseWithoutKineticChain_ShouldSucceed()
        {
            // Arrange
            var autoMocker = new AutoMocker();
            var testee = autoMocker.CreateInstance<ExerciseService>();
            
            var exerciseDto = new ExerciseDto
            {
                Id = ExerciseId.New().ToString(),
                Name = "Rest Period",
                Description = "Rest between sets",
                Difficulty = new ReferenceDataDto { Id = _difficultyId.ToString(), Value = "Test Difficulty" },
                KineticChain = null
            };
            
            var command = CreateExerciseCommandBuilder.ForRestExercise()
                .WithName("Rest Period")
                .WithDescription("Rest between sets")
                .WithDifficultyId(_difficultyId)
                .WithExerciseTypes(_restTypeId)
                .Build();

            // Setup mocks - exercise name doesn't exist, exercise types exist, is REST type, successful create
            autoMocker.SetupExerciseQueryDataServiceExistsByName(command.Name, false);
            autoMocker.SetupExerciseTypeService(allExist: true, isRestType: true);
            autoMocker.SetupExerciseCommandDataServiceCreate(exerciseDto);

            // Act
            var result = await testee.CreateAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.KineticChain.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_NonRestExerciseWithoutKineticChain_ShouldReturnFailure()
        {
            // Arrange
            var autoMocker = new AutoMocker();
            var testee = autoMocker.CreateInstance<ExerciseService>();
            
            var exerciseId = ExerciseId.New();
            var command = UpdateExerciseCommandBuilder.ForWorkoutExercise()
                .WithName("Updated Exercise")
                .WithDescription("Updated Description")
                .WithDifficultyId(_difficultyId)
                .WithExerciseTypes(_strengthTypeId)
                .WithKineticChainId(null) // No kinetic chain provided
                .WithMuscleGroups((MuscleGroupId.New(), MuscleRoleId.New()))
                .Build();

            // Setup mocks - exercise exists, name doesn't exist for another exercise, exercise types exist, not REST type
            autoMocker.SetupExerciseQueryDataServiceExists(exerciseId, true);
            autoMocker.SetupExerciseQueryDataServiceExistsByName(command.Name, false, exerciseId);
            autoMocker.SetupExerciseTypeService(allExist: true, isRestType: false);

            // Act
            var result = await testee.UpdateAsync(exerciseId, command);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        }

        [Fact]
        public async Task UpdateAsync_RestExerciseWithKineticChain_ShouldReturnFailure()
        {
            // Arrange
            var autoMocker = new AutoMocker();
            var testee = autoMocker.CreateInstance<ExerciseService>();
            
            var exerciseId = ExerciseId.New();
            var command = UpdateExerciseCommandBuilder.ForRestExercise()
                .WithName("Updated Rest")
                .WithDescription("Updated rest period")
                .WithDifficultyId(_difficultyId)
                .WithExerciseTypes(_restTypeId)
                .WithKineticChainId(_kineticChainId) // Kinetic chain provided for rest
                .Build();

            // Setup mocks - exercise exists, name doesn't exist for another exercise, exercise types exist, is REST type
            autoMocker.SetupExerciseQueryDataServiceExists(exerciseId, true);
            autoMocker.SetupExerciseQueryDataServiceExistsByName(command.Name, false, exerciseId);
            autoMocker.SetupExerciseTypeService(allExist: true, isRestType: true);

            // Act
            var result = await testee.UpdateAsync(exerciseId, command);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        }

        [Fact]
        public async Task UpdateAsync_NonRestExerciseWithKineticChain_ShouldSucceed()
        {
            // Arrange
            var autoMocker = new AutoMocker();
            var testee = autoMocker.CreateInstance<ExerciseService>();
            
            var exerciseId = ExerciseId.New();
            var exerciseDto = new ExerciseDto
            {
                Id = exerciseId.ToString(),
                Name = "Updated Exercise",
                Description = "Updated Description",
                Difficulty = new ReferenceDataDto { Id = _difficultyId.ToString(), Value = "Test Difficulty" },
                KineticChain = new ReferenceDataDto { Id = _kineticChainId.ToString(), Value = "Test Kinetic Chain" }
            };
            
            var command = UpdateExerciseCommandBuilder.ForWorkoutExercise()
                .WithName("Updated Exercise")
                .WithDescription("Updated Description")
                .WithDifficultyId(_difficultyId)
                .WithExerciseTypes(_strengthTypeId)
                .WithKineticChainId(_kineticChainId)
                .WithMuscleGroups((MuscleGroupId.New(), MuscleRoleId.New()))
                .Build();

            // Setup mocks - exercise exists, name doesn't exist for another exercise, exercise types exist, not REST type, successful update
            autoMocker.SetupExerciseQueryDataServiceExists(exerciseId, true);
            autoMocker.SetupExerciseQueryDataServiceExistsByName(command.Name, false, exerciseId);
            autoMocker.SetupExerciseTypeService(allExist: true, isRestType: false);
            autoMocker.SetupExerciseCommandDataServiceUpdate(exerciseDto);

            // Act
            var result = await testee.UpdateAsync(exerciseId, command);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.KineticChain.Should().NotBeNull();
            result.Data.KineticChain!.Id.Should().Be(_kineticChainId.ToString());
        }

        [Fact]
        public async Task CreateAsync_InvalidKineticChainId_ShouldReturnFailure()
        {
            // Arrange - This test is for validation at the request level, but commands use domain IDs
            // So we'd test this at the controller level or skip it since domain IDs prevent invalid formats
            // For now, let's simulate the validation by using an empty KineticChainId on a non-REST exercise
            var autoMocker = new AutoMocker();
            var testee = autoMocker.CreateInstance<ExerciseService>();
            
            var command = CreateExerciseCommandBuilder.ForWorkoutExercise()
                .WithName("Test Exercise")
                .WithDescription("Test Description")
                .WithDifficultyId(_difficultyId)
                .WithExerciseTypes(_strengthTypeId)
                .WithKineticChainId(KineticChainTypeId.Empty) // Invalid: empty ID for non-REST
                .WithMuscleGroups((MuscleGroupId.New(), MuscleRoleId.New()))
                .Build();

            // Setup mocks - exercise name doesn't exist, exercise types exist, not REST type
            autoMocker.SetupExerciseQueryDataServiceExistsByName(command.Name, false);
            autoMocker.SetupExerciseTypeService(allExist: true, isRestType: false);

            // Act
            var result = await testee.CreateAsync(command);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        }

        [Fact]
        public async Task UpdateAsync_InvalidKineticChainId_ShouldReturnFailure()
        {
            // Arrange - Similar to create test: using empty KineticChainId on a non-REST exercise
            var autoMocker = new AutoMocker();
            var testee = autoMocker.CreateInstance<ExerciseService>();
            
            var exerciseId = ExerciseId.New();
            var command = UpdateExerciseCommandBuilder.ForWorkoutExercise()
                .WithName("Updated Exercise")
                .WithDescription("Updated Description")
                .WithDifficultyId(_difficultyId)
                .WithExerciseTypes(_strengthTypeId)
                .WithKineticChainId(KineticChainTypeId.Empty) // Invalid: empty ID for non-REST
                .WithMuscleGroups((MuscleGroupId.New(), MuscleRoleId.New()))
                .Build();

            // Setup mocks - exercise exists, name doesn't exist for another exercise, exercise types exist, not REST type
            autoMocker.SetupExerciseQueryDataServiceExists(exerciseId, true);
            autoMocker.SetupExerciseQueryDataServiceExistsByName(command.Name, false, exerciseId);
            autoMocker.SetupExerciseTypeService(allExist: true, isRestType: false);

            // Act
            var result = await testee.UpdateAsync(exerciseId, command);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        }
    }
}