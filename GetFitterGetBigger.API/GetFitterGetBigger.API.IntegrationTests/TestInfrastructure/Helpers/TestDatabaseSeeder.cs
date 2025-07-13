using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.IntegrationTests.TestBuilders;
using Microsoft.EntityFrameworkCore;

namespace GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;

/// <summary>
/// Handles seeding of test data for integration tests.
/// Wraps the existing SeedDataBuilder to provide consistent test data.
/// </summary>
public class TestDatabaseSeeder
{
    private readonly FitnessDbContext _context;
    private readonly SeedDataBuilder _seedBuilder;
    
    public TestDatabaseSeeder(FitnessDbContext context)
    {
        _context = context;
        _seedBuilder = new SeedDataBuilder(context);
    }
    
    /// <summary>
    /// Seeds all reference data needed for tests
    /// </summary>
    public async Task SeedReferenceDataAsync()
    {
        await _seedBuilder.WithAllReferenceDataAsync();
    }
    
    /// <summary>
    /// Seeds specific exercise types
    /// </summary>
    public async Task SeedExerciseTypesAsync()
    {
        await _seedBuilder.WithExerciseTypesAsync();
    }
    
    /// <summary>
    /// Seeds specific difficulty levels
    /// </summary>
    public async Task SeedDifficultyLevelsAsync()
    {
        await _seedBuilder.WithDifficultyLevelsAsync();
    }
    
    /// <summary>
    /// Seeds specific equipment
    /// </summary>
    public async Task SeedEquipmentAsync()
    {
        await _seedBuilder.WithEquipmentAsync();
    }
    
    /// <summary>
    /// Seeds specific body parts
    /// </summary>
    public async Task SeedBodyPartsAsync()
    {
        await _seedBuilder.WithBodyPartsAsync();
    }
    
    /// <summary>
    /// Seeds specific muscle groups
    /// </summary>
    public async Task SeedMuscleGroupsAsync()
    {
        await _seedBuilder.WithMuscleGroupsAsync();
    }
    
    /// <summary>
    /// Seeds specific muscle roles
    /// </summary>
    public async Task SeedMuscleRolesAsync()
    {
        await _seedBuilder.WithMuscleRolesAsync();
    }
    
    /// <summary>
    /// Seeds specific kinetic chain types
    /// </summary>
    public async Task SeedKineticChainTypesAsync()
    {
        await _seedBuilder.WithKineticChainTypesAsync();
    }
    
    /// <summary>
    /// Seeds specific movement patterns
    /// </summary>
    public async Task SeedMovementPatternsAsync()
    {
        await _seedBuilder.WithMovementPatternsAsync();
    }
    
    /// <summary>
    /// Seeds specific exercise weight types
    /// </summary>
    public async Task SeedExerciseWeightTypesAsync()
    {
        await _seedBuilder.WithExerciseWeightTypesAsync();
    }
    
    /// <summary>
    /// Seeds specific workout objectives
    /// </summary>
    public async Task SeedWorkoutObjectivesAsync()
    {
        await _seedBuilder.WithWorkoutObjectivesAsync();
    }
    
    /// <summary>
    /// Seeds specific workout categories
    /// </summary>
    public async Task SeedWorkoutCategoriesAsync()
    {
        await _seedBuilder.WithWorkoutCategoriesAsync();
    }
    
    /// <summary>
    /// Seeds specific execution protocols
    /// </summary>
    public async Task SeedExecutionProtocolsAsync()
    {
        await _seedBuilder.WithExecutionProtocolsAsync();
    }
    
    /// <summary>
    /// Clears all non-reference data from the database
    /// </summary>
    public async Task ClearTestDataAsync()
    {
        // Remove all exercise-related data
        _context.ExerciseLinks.RemoveRange(_context.ExerciseLinks);
        _context.Exercises.RemoveRange(_context.Exercises);
        
        // Add more entity cleanups here as the project grows
        // For example:
        // _context.WorkoutPlans.RemoveRange(_context.WorkoutPlans);
        // _context.WorkoutSessions.RemoveRange(_context.WorkoutSessions);
        
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Creates a specific entity from the seed builder
    /// </summary>
    public SeedDataBuilder GetSeedBuilder() => _seedBuilder;
}