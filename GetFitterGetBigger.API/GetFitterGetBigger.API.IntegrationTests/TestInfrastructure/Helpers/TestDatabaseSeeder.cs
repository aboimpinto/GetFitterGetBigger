using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
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
    /// Seeds workout states if they don't exist
    /// </summary>
    public async Task SeedWorkoutStatesAsync()
    {
        // Check if workout states already exist from migrations
        var existingStates = await _context.WorkoutStates.AnyAsync();
        if (!existingStates)
        {
            // Manually seed workout states if migrations didn't create them
            var states = new[]
            {
                WorkoutState.Handler.Create(
                    WorkoutStateId.From(Guid.Parse("02000001-0000-0000-0000-000000000001")),
                    "DRAFT",
                    "Template under construction",
                    1,
                    true).Value,
                WorkoutState.Handler.Create(
                    WorkoutStateId.From(Guid.Parse("02000001-0000-0000-0000-000000000002")),
                    "PRODUCTION",
                    "Active template for use",
                    2,
                    true).Value,
                WorkoutState.Handler.Create(
                    WorkoutStateId.From(Guid.Parse("02000001-0000-0000-0000-000000000003")),
                    "ARCHIVED",
                    "Retired template",
                    3,
                    true).Value
            };
            
            _context.WorkoutStates.AddRange(states);
            await _context.SaveChangesAsync();
        }
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
    
    /// <summary>
    /// Seeds a test workout template with the specified ID
    /// </summary>
    public async Task SeedTestWorkoutTemplateAsync(WorkoutTemplateId templateId)
    {
        // Check if template already exists
        var existingTemplate = await _context.WorkoutTemplates
            .FirstOrDefaultAsync(wt => wt.Id == templateId);
        
        if (existingTemplate != null)
        {
            return; // Template already seeded
        }
        
        var categoryId = await GetOrCreateTestCategoryId();
        var difficultyId = await GetOrCreateTestDifficultyId();
        var stateId = await GetOrCreateTestWorkoutStateId();
        
        var now = DateTime.UtcNow;
        var templateResult = API.Models.Entities.WorkoutTemplate.Handler.Create(
            templateId,
            "Test Workout Template",
            "A test workout template for integration testing",
            categoryId,
            difficultyId,
            60,
            new List<string> { "test" },
            true,
            stateId,
            now,
            now);
        
        if (templateResult.IsSuccess)
        {
            _context.WorkoutTemplates.Add(templateResult.Value);
            await _context.SaveChangesAsync();
            
            // Detach the entity to ensure fresh load with navigation properties
            _context.Entry(templateResult.Value).State = EntityState.Detached;
        }
        else
        {
            throw new InvalidOperationException($"Failed to create test workout template: {templateResult.FirstError}");
        }
    }
    
    private async Task<WorkoutCategoryId> GetOrCreateTestCategoryId()
    {
        // First try to find any of the expected categories
        var category = await _context.WorkoutCategories.FirstOrDefaultAsync(c => 
            c.Value == "Upper Body" || c.Value == "Full Body");
            
        if (category == null)
        {
            await SeedWorkoutCategoriesAsync();
            category = await _context.WorkoutCategories.FirstOrDefaultAsync(c => 
                c.Value == "Upper Body" || c.Value == "Full Body");
            
            if (category == null)
            {
                // If still not found, just use the first available category
                category = await _context.WorkoutCategories.FirstOrDefaultAsync();
                
                if (category == null)
                {
                    throw new InvalidOperationException("Failed to find or create any workout category. Available categories: " + 
                        string.Join(", ", await _context.WorkoutCategories.Select(c => c.Value).ToListAsync()));
                }
            }
        }
        return category.WorkoutCategoryId;
    }
    
    private async Task<DifficultyLevelId> GetOrCreateTestDifficultyId()
    {
        var difficulty = await _context.DifficultyLevels.FirstOrDefaultAsync(d => d.Value == "Intermediate");
        if (difficulty == null)
        {
            await SeedDifficultyLevelsAsync();
            difficulty = await _context.DifficultyLevels.FirstOrDefaultAsync(d => d.Value == "Intermediate");
            
            if (difficulty == null)
            {
                throw new InvalidOperationException("Failed to find or create 'Intermediate' difficulty level. Available levels: " + 
                    string.Join(", ", await _context.DifficultyLevels.Select(d => d.Value).ToListAsync()));
            }
        }
        return difficulty.DifficultyLevelId;
    }
    
    private async Task<WorkoutStateId> GetOrCreateTestWorkoutStateId()
    {
        var state = await _context.WorkoutStates.FirstOrDefaultAsync(s => s.Value == "DRAFT");
        if (state == null)
        {
            // Seed workout states if they don't exist
            await SeedWorkoutStatesAsync();
            state = await _context.WorkoutStates.FirstOrDefaultAsync(s => s.Value == "DRAFT");
        }
        
        if (state == null)
        {
            throw new InvalidOperationException("Failed to find or create DRAFT workout state");
        }
        
        return state.WorkoutStateId;
    }
}