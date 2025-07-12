using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Tests.TestBuilders;
using Microsoft.Extensions.DependencyInjection;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

/// <summary>
/// Tests for creating REST exercises with empty KineticChainId and ExerciseWeightTypeId
/// </summary>
public class RestExerciseCreationTests : IClassFixture<PostgreSqlApiTestFixture>
{
    private readonly PostgreSqlApiTestFixture _fixture;

    public RestExerciseCreationTests(PostgreSqlApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateAsync_RestExercise_ShouldSucceed_WithEmptyKineticChainAndWeightType()
    {
        // Ensure test data is seeded
        await _fixture.CleanupDatabaseAsync();
        using var seedScope = _fixture.Services.CreateScope();
        var context = seedScope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        var seedBuilder = new SeedDataBuilder(context);
        await seedBuilder.WithAllReferenceDataAsync();
        
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var exerciseService = scope.ServiceProvider.GetRequiredService<IExerciseService>();
        var restExerciseTypeId = ExerciseTypeId.From(SeedDataBuilder.StandardIds.RestExerciseTypeId);
        
        var command = new CreateExerciseCommand
        {
            Name = $"Rest Period {Guid.NewGuid()}",
            Description = "Active recovery period between sets",
            DifficultyId = DifficultyLevelId.From(SeedDataBuilder.StandardIds.BeginnerDifficultyId),
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
        if (!result.IsSuccess)
        {
            var errors = string.Join(", ", result.Errors);
            throw new Exception($"Expected success but got errors: {errors}");
        }
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Data);
        Assert.StartsWith("Rest Period", result.Data.Name);
        Assert.Single(result.Data.ExerciseTypes);
        Assert.Equal("Rest", result.Data.ExerciseTypes.First().Value);
        Assert.Null(result.Data.KineticChain);
        Assert.Null(result.Data.ExerciseWeightType);
        Assert.Empty(result.Data.MuscleGroups);
    }

    [Fact]
    public async Task UpdateAsync_ExistingExercise_ToRestExercise_ShouldSucceed()
    {
        // Ensure test data is seeded
        await _fixture.CleanupDatabaseAsync();
        using var seedScope = _fixture.Services.CreateScope();
        var context = seedScope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        var seedBuilder = new SeedDataBuilder(context);
        await seedBuilder.WithAllReferenceDataAsync();
        
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var exerciseService = scope.ServiceProvider.GetRequiredService<IExerciseService>();
        var restExerciseTypeId = ExerciseTypeId.From(SeedDataBuilder.StandardIds.RestExerciseTypeId);
        var workoutExerciseTypeId = ExerciseTypeId.From(SeedDataBuilder.StandardIds.WorkoutExerciseTypeId);
        
        // First create a regular exercise
        var createCommand = new CreateExerciseCommand
        {
            Name = $"Test Exercise {Guid.NewGuid()}",
            Description = "Exercise that will be converted to REST",
            DifficultyId = DifficultyLevelId.From(SeedDataBuilder.StandardIds.BeginnerDifficultyId),
            KineticChainId = KineticChainTypeId.From(SeedDataBuilder.StandardIds.CompoundKineticChainId),
            ExerciseWeightTypeId = ExerciseWeightTypeId.From(SeedDataBuilder.StandardIds.WeightRequiredWeightTypeId),
            ExerciseTypeIds = new List<ExerciseTypeId> { workoutExerciseTypeId },
            MuscleGroups = new List<MuscleGroupAssignment>
            {
                new()
                {
                    MuscleGroupId = MuscleGroupId.From(SeedDataBuilder.StandardIds.ChestMuscleGroupId),
                    MuscleRoleId = MuscleRoleId.From(SeedDataBuilder.StandardIds.PrimaryMuscleRoleId)
                }
            },
            CoachNotes = new List<CoachNoteCommand>(),
            EquipmentIds = new List<EquipmentId>(),
            BodyPartIds = new List<BodyPartId>(),
            MovementPatternIds = new List<MovementPatternId>()
        };

        var createResult = await exerciseService.CreateAsync(createCommand);
        Assert.True(createResult.IsSuccess);
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
        Assert.True(updateResult.IsSuccess);
        Assert.Empty(updateResult.Errors);
        Assert.NotNull(updateResult.Data);
        Assert.Equal("Now a REST exercise", updateResult.Data.Description);
        Assert.Single(updateResult.Data.ExerciseTypes);
        Assert.Equal("Rest", updateResult.Data.ExerciseTypes.First().Value);
        Assert.Null(updateResult.Data.KineticChain);
        Assert.Null(updateResult.Data.ExerciseWeightType);
        Assert.Empty(updateResult.Data.MuscleGroups);
    }
}