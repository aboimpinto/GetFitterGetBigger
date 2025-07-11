using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

/// <summary>
/// Tests for creating REST exercises with empty KineticChainId and ExerciseWeightTypeId
/// </summary>
public class RestExerciseCreationTests : IClassFixture<PostgreSqlApiTestFixture>
{
    private readonly PostgreSqlApiTestFixture _fixture;
    private readonly IServiceProvider _serviceProvider;

    public RestExerciseCreationTests(PostgreSqlApiTestFixture fixture)
    {
        _fixture = fixture;
        _serviceProvider = _fixture.CreateServiceProvider();
    }

    [Fact]
    public async Task CreateAsync_RestExercise_ShouldSucceed_WithEmptyKineticChainAndWeightType()
    {
        // Arrange
        var exerciseService = _serviceProvider.GetRequiredService<IExerciseService>();
        var restExerciseTypeId = ExerciseTypeId.From(Guid.Parse("d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a")); // REST type
        
        var command = new CreateExerciseCommand
        {
            Name = $"Rest Period {Guid.NewGuid()}",
            Description = "Active recovery period between sets",
            DifficultyId = DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b")),
            KineticChainId = KineticChainTypeId.Empty, // Empty for REST
            ExerciseWeightTypeId = ExerciseWeightTypeId.Empty, // Empty for REST
            ExerciseTypeIds = new List<ExerciseTypeId> { restExerciseTypeId },
            MuscleGroups = new List<MuscleGroupAssignment>(), // Empty for REST
            CoachNotes = new List<CoachNoteCommand>
            {
                new() { Text = "Focus on breathing and recovery", Order = 0 }
            },
            EquipmentIds = new List<EquipmentId>(),
            BodyPartIds = new List<BodyPartId>(),
            MovementPatternIds = new List<MovementPatternId>()
        };

        // Act
        var result = await exerciseService.CreateAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Data.Should().NotBeNull();
        result.Data.Name.Should().StartWith("Rest Period");
        result.Data.ExerciseTypes.Should().HaveCount(1);
        result.Data.ExerciseTypes.First().Value.Should().Be("Rest");
        result.Data.KineticChain.Should().BeNull();
        result.Data.ExerciseWeightType.Should().BeNull();
        result.Data.MuscleGroups.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_ExistingExercise_ToRestExercise_ShouldSucceed()
    {
        // Arrange
        var exerciseService = _serviceProvider.GetRequiredService<IExerciseService>();
        var restExerciseTypeId = ExerciseTypeId.From(Guid.Parse("d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a")); // REST type
        var workoutExerciseTypeId = ExerciseTypeId.From(Guid.Parse("b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e")); // Workout type
        
        // First create a regular exercise
        var createCommand = new CreateExerciseCommand
        {
            Name = $"Test Exercise {Guid.NewGuid()}",
            Description = "Exercise that will be converted to REST",
            DifficultyId = DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b")),
            KineticChainId = KineticChainTypeId.From(Guid.Parse("f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4")), // Compound
            ExerciseWeightTypeId = ExerciseWeightTypeId.From(Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a")), // Weight Required
            ExerciseTypeIds = new List<ExerciseTypeId> { workoutExerciseTypeId },
            MuscleGroups = new List<MuscleGroupAssignment>
            {
                new()
                {
                    MuscleGroupId = MuscleGroupId.New(),
                    MuscleRoleId = MuscleRoleId.From(Guid.Parse("5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b")) // Primary
                }
            },
            CoachNotes = new List<CoachNoteCommand>(),
            EquipmentIds = new List<EquipmentId>(),
            BodyPartIds = new List<BodyPartId>(),
            MovementPatternIds = new List<MovementPatternId>()
        };

        var createResult = await exerciseService.CreateAsync(createCommand);
        createResult.IsSuccess.Should().BeTrue();
        var exerciseId = ExerciseId.ParseOrEmpty(createResult.Data.Id);

        // Now update it to be a REST exercise
        var updateCommand = new UpdateExerciseCommand
        {
            Name = createCommand.Name,
            Description = "Now a REST exercise",
            DifficultyId = createCommand.DifficultyId,
            KineticChainId = KineticChainTypeId.Empty, // Empty for REST
            ExerciseWeightTypeId = ExerciseWeightTypeId.Empty, // Empty for REST
            ExerciseTypeIds = new List<ExerciseTypeId> { restExerciseTypeId },
            MuscleGroups = new List<MuscleGroupAssignment>(), // Empty for REST
            CoachNotes = new List<CoachNoteCommand>
            {
                new() { Text = "This is now a rest period", Order = 0 }
            },
            EquipmentIds = new List<EquipmentId>(),
            BodyPartIds = new List<BodyPartId>(),
            MovementPatternIds = new List<MovementPatternId>(),
            IsActive = true
        };

        // Act
        var updateResult = await exerciseService.UpdateAsync(exerciseId, updateCommand);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Errors.Should().BeEmpty();
        updateResult.Data.Should().NotBeNull();
        updateResult.Data.Description.Should().Be("Now a REST exercise");
        updateResult.Data.ExerciseTypes.Should().HaveCount(1);
        updateResult.Data.ExerciseTypes.First().Value.Should().Be("Rest");
        updateResult.Data.KineticChain.Should().BeNull();
        updateResult.Data.ExerciseWeightType.Should().BeNull();
        updateResult.Data.MuscleGroups.Should().BeEmpty();
    }
}