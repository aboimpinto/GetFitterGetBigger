using System.Net;
using GetFitterGetBigger.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

/// <summary>
/// Integration tests for database migration functionality using real PostgreSQL database.
/// These tests verify that the automatic database migration on startup works correctly
/// with actual PostgreSQL migrations.
/// </summary>
[Collection("PostgreSQL Integration Tests")]
public class PostgreSqlMigrationTests : PostgreSqlTestBase
{
    private readonly ITestOutputHelper _output;
    
    public PostgreSqlMigrationTests(PostgreSqlApiTestFixture factory, ITestOutputHelper output) 
        : base(factory)
    {
        _output = output;
    }
    
    [Fact]
    public async Task Application_WithFreshDatabase_AppliesMigrationsSuccessfully()
    {
        // Arrange - Get a scope to access the database context
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        // Act - The migrations should have been applied during application startup
        var canConnect = await context.Database.CanConnectAsync();
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        
        // Assert
        Assert.True(canConnect, "Should be able to connect to the database");
        Assert.NotEmpty(appliedMigrations);
        
        foreach (var migration in appliedMigrations)
        {
            _output.WriteLine($"Applied migration: {migration}");
        }
        
        // Verify we can query tables (which means schema exists)
        var exerciseCount = await context.Exercises.CountAsync();
        Assert.True(exerciseCount >= 0, "Should be able to query exercises table");
        
        // Verify the API is responding
        var response = await Client.GetAsync("/api/exercises");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Application_WithExistingMigratedDatabase_ChecksMigrationsQuickly()
    {
        // This test verifies that when migrations are already applied,
        // the startup process is fast (just checking, not applying)
        
        // Arrange - Database is already migrated from the first test or fixture setup
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        // Act - Check how many migrations are applied
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        
        // Assert
        Assert.NotEmpty(appliedMigrations);
        Assert.Empty(pendingMigrations);
        
        _output.WriteLine($"Found {appliedMigrations.Count()} applied migrations");
        _output.WriteLine($"Found {pendingMigrations.Count()} pending migrations");
        
        // The API should be working
        var response = await Client.GetAsync("/api/exercises");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task DatabaseMigration_CreatesAllRequiredTables()
    {
        // This test verifies that all expected tables are created by migrations
        
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        // List of expected tables (based on the DbContext entities)
        var expectedTables = new[]
        {
            "Exercises",
            "MuscleGroups",
            "Equipment",
            "BodyParts",
            "MovementPatterns",
            "ExerciseTypes",
            "DifficultyLevels",
            "KineticChainTypes",
            "MuscleRoles",
            "MetricTypes",
            "Users",
            "Claims",
            "__EFMigrationsHistory" // EF Core migrations table
        };
        
        // Check each table exists by attempting a count query
        foreach (var tableName in expectedTables)
        {
            try
            {
                // This is a simple way to check if table exists
                // More sophisticated checks could query information_schema
                var sql = $"SELECT COUNT(*) FROM \"{tableName}\"";
                var count = await context.Database.ExecuteSqlRawAsync(sql);
                _output.WriteLine($"Table '{tableName}' exists");
            }
            catch (Exception ex)
            {
                // If we get here, the table might not exist
                Assert.Fail($"Expected table '{tableName}' was not found: {ex.Message}");
            }
        }
    }
    
    [Fact]
    public async Task DatabaseMigration_SeedsReferenceData()
    {
        // This test verifies that reference data is properly seeded
        
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        // Check that reference data has been seeded
        var difficultyLevels = await context.DifficultyLevels.CountAsync();
        var muscleRoles = await context.MuscleRoles.CountAsync();
        var kineticChainTypes = await context.KineticChainTypes.CountAsync();
        
        Assert.True(difficultyLevels > 0, "DifficultyLevels should be seeded");
        Assert.True(muscleRoles > 0, "MuscleRoles should be seeded");
        Assert.True(kineticChainTypes > 0, "KineticChainTypes should be seeded");
        
        _output.WriteLine($"Seeded data counts:");
        _output.WriteLine($"- DifficultyLevels: {difficultyLevels}");
        _output.WriteLine($"- MuscleRoles: {muscleRoles}");
        _output.WriteLine($"- KineticChainTypes: {kineticChainTypes}");
    }
}