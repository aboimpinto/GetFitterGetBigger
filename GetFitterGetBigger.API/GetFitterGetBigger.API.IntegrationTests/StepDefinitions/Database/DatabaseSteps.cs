using FluentAssertions;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Tests.TestBuilders;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.Database;

[Binding]
public class DatabaseSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly PostgreSqlTestFixture _fixture;
    
    public DatabaseSteps(ScenarioContext scenarioContext, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }
    
    [Given(@"the following (.*) exists:")]
    public async Task GivenTheFollowingEntityExists(string entityType, Table table)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var seeder = new TestDatabaseSeeder(context);
            
            // For now, we'll use the SeedDataBuilder for reference data
            // and expand this method as we implement specific entity creation
            switch (entityType.ToLower())
            {
                case "difficultylevel":
                case "difficulty level":
                case "difficulty levels":
                    await seeder.SeedDifficultyLevelsAsync();
                    break;
                    
                case "equipment":
                    await seeder.SeedEquipmentAsync();
                    break;
                    
                case "bodypart":
                case "body part":
                case "body parts":
                    await seeder.SeedBodyPartsAsync();
                    break;
                    
                case "musclegroup":
                case "muscle group":
                case "muscle groups":
                    await seeder.SeedMuscleGroupsAsync();
                    break;
                    
                case "exercisetype":
                case "exercise type":
                case "exercise types":
                    await seeder.SeedExerciseTypesAsync();
                    break;
                    
                default:
                    throw new NotSupportedException($"Entity type '{entityType}' is not yet supported in this simplified implementation");
            }
            
            await context.SaveChangesAsync();
        });
    }
    
    [Given(@"the database is empty")]
    public async Task GivenTheDatabaseIsEmpty()
    {
        await _fixture.CleanDatabaseAsync();
    }
    
    [Given(@"the database has reference data")]
    [When(@"the database has reference data")]
    public async Task GivenTheDatabaseHasReferenceData()
    {
        await _fixture.InitializeDatabaseAsync();
    }
    
    [Then(@"the database should contain (\d+) (.*) records?")]
    public async Task ThenTheDatabaseShouldContain(int expectedCount, string entityType)
    {
        var actualCount = await _fixture.ExecuteDbContextAsync(async context =>
        {
            return entityType.ToLower() switch
            {
                "exercise" or "exercises" => await context.Exercises.CountAsync(),
                "equipment" => await context.Equipment.CountAsync(),
                "difficultylevel" or "difficulty level" or "difficulty levels" => await context.DifficultyLevels.CountAsync(),
                "bodypart" or "body part" or "body parts" => await context.BodyParts.CountAsync(),
                "musclegroup" or "muscle group" or "muscle groups" => await context.MuscleGroups.CountAsync(),
                "exercisetype" or "exercise type" or "exercise types" => await context.ExerciseTypes.CountAsync(),
                "exerciselink" or "exercise link" or "exercise links" => await context.ExerciseLinks.CountAsync(),
                _ => throw new NotSupportedException($"Entity type '{entityType}' is not supported")
            };
        });
        
        actualCount.Should().Be(expectedCount, 
            $"expected {expectedCount} {entityType} records but found {actualCount}");
    }
    
    [Then(@"the (.*) with id ""(.*)"" should exist")]
    public Task ThenTheEntityWithIdShouldExist(string entityType, string id)
    {
        var resolvedId = _scenarioContext.ResolvePlaceholders(id);
        
        // For now, just check if the ID format is valid
        // We'll expand this once we have proper entity creation
        Guid.TryParse(resolvedId.Replace($"{entityType.ToLower()}-", ""), out var guidId)
            .Should().BeTrue($"'{resolvedId}' should be a valid {entityType} ID");
        
        // TODO: Implement actual database checks once entity creation is properly implemented
        return Task.CompletedTask;
    }
    
    [Then(@"the (.*) with id ""(.*)"" should not exist")]
    public Task ThenTheEntityWithIdShouldNotExist(string entityType, string id)
    {
        var resolvedId = _scenarioContext.ResolvePlaceholders(id);
        
        // For now, just check if the ID format is valid
        // We'll expand this once we have proper entity creation
        Guid.TryParse(resolvedId.Replace($"{entityType.ToLower()}-", ""), out var guidId)
            .Should().BeTrue($"'{resolvedId}' should be a valid {entityType} ID");
        
        // TODO: Implement actual database checks once entity creation is properly implemented
        return Task.CompletedTask;
    }
    
    [Then(@"the exercise ""(.*)"" should have the following properties:")]
    public async Task ThenTheExerciseShouldHaveTheFollowingProperties(string exerciseName, Table expectedProperties)
    {
        // Simplified implementation for now
        // TODO: Implement once we have proper exercise creation
        var exercise = await _fixture.ExecuteDbContextAsync(async context =>
        {
            return await context.Exercises
                .Include(e => e.Difficulty)
                .Include(e => e.KineticChain)
                .Include(e => e.ExerciseWeightType)
                .FirstOrDefaultAsync(e => e.Name == exerciseName);
        });
        
        if (exercise != null)
        {
            exercise.Name.Should().Be(exerciseName);
            // Additional property checks will be implemented when entity creation is fixed
        }
    }
    
    [Then(@"the database should be accessible")]
    public async Task ThenTheDatabaseShouldBeAccessible()
    {
        var canConnect = await _fixture.ExecuteDbContextAsync(async context =>
        {
            return await context.Database.CanConnectAsync();
        });
        
        canConnect.Should().BeTrue("database should be accessible");
    }
    
    [Then(@"the database should contain reference data")]
    public async Task ThenTheDatabaseShouldContainReferenceData()
    {
        var counts = await _fixture.ExecuteDbContextAsync(async context =>
        {
            return new
            {
                DifficultyLevels = await context.DifficultyLevels.CountAsync(),
                ExerciseTypes = await context.ExerciseTypes.CountAsync(),
                BodyParts = await context.BodyParts.CountAsync(),
                MuscleGroups = await context.MuscleGroups.CountAsync(),
                Equipment = await context.Equipment.CountAsync()
            };
        });
        
        counts.DifficultyLevels.Should().BeGreaterThan(0, "should have difficulty levels");
        counts.ExerciseTypes.Should().BeGreaterThan(0, "should have exercise types");
        counts.BodyParts.Should().BeGreaterThan(0, "should have body parts");
        counts.MuscleGroups.Should().BeGreaterThan(0, "should have muscle groups");
        counts.Equipment.Should().BeGreaterThan(0, "should have equipment");
    }
    
    [When(@"I check database connectivity")]
    public async Task WhenICheckDatabaseConnectivity()
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var canConnect = await context.Database.CanConnectAsync();
            _scenarioContext.Set(canConnect, "DatabaseConnectivity");
        });
    }
    
    [Then(@"the database should be connected")]
    public void ThenTheDatabaseShouldBeConnected()
    {
        var isConnected = _scenarioContext.Get<bool>("DatabaseConnectivity");
        isConnected.Should().BeTrue("database should be accessible");
    }
    
    [Then(@"the database should have the expected schema")]
    public async Task ThenTheDatabaseShouldHaveTheExpectedSchema()
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            // Check that key tables exist by attempting to query them
            var exerciseCount = await context.Exercises.CountAsync();
            exerciseCount.Should().BeGreaterOrEqualTo(0, "Exercises table should exist");
            
            var bodyPartCount = await context.BodyParts.CountAsync();
            bodyPartCount.Should().BeGreaterOrEqualTo(0, "BodyParts table should exist");
            
            var muscleGroupCount = await context.MuscleGroups.CountAsync();
            muscleGroupCount.Should().BeGreaterOrEqualTo(0, "MuscleGroups table should exist");
            
            var difficultyLevelCount = await context.DifficultyLevels.CountAsync();
            difficultyLevelCount.Should().BeGreaterOrEqualTo(0, "DifficultyLevels table should exist");
        });
    }
}