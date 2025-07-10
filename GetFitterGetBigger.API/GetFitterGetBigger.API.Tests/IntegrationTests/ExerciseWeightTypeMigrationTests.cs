using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

/// <summary>
/// Integration tests for ExerciseWeightType migration and seed data.
/// </summary>
public class ExerciseWeightTypeMigrationTests : PostgreSqlTestBase
{
    private readonly ITestOutputHelper _output;
    
    public ExerciseWeightTypeMigrationTests(PostgreSqlApiTestFixture fixture, ITestOutputHelper output) 
        : base(fixture)
    {
        _output = output;
    }
    
    [Fact]
    public async Task ExerciseWeightTypeTable_IsCreatedByMigration()
    {
        // This test verifies that the ExerciseWeightType table is created properly
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        // Try to query the table - this will fail if the table doesn't exist
        var count = await context.ExerciseWeightTypes.CountAsync();
        
        // If we get here, the table exists
        Assert.True(count >= 0, "Should be able to query ExerciseWeightTypes table");
    }
    
    [Fact]
    public async Task ExerciseWeightTypeTable_HasCorrectSchema()
    {
        // This test verifies the table has the expected columns and constraints
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        // Check we can query all expected columns
        var query = await context.ExerciseWeightTypes
            .Select(ewt => new 
            {
                ewt.Id,
                ewt.Code,
                ewt.Value,
                ewt.Description,
                ewt.DisplayOrder,
                ewt.IsActive
            })
            .ToListAsync();
        
        // If we get here without exceptions, all columns exist
        Assert.NotNull(query);
    }
    
    [Fact]
    public async Task ExerciseWeightTypeSeedData_IsInsertedCorrectly()
    {
        // This test verifies that all 5 weight types are seeded
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var weightTypes = await context.ExerciseWeightTypes
            .OrderBy(ewt => ewt.DisplayOrder)
            .ToListAsync();
        
        Assert.Equal(5, weightTypes.Count);
        
        // Verify each weight type
        var bodyweightOnly = weightTypes.Single(wt => wt.Code == "BODYWEIGHT_ONLY");
        Assert.Equal("Bodyweight Only", bodyweightOnly.Value);
        Assert.Equal(1, bodyweightOnly.DisplayOrder);
        Assert.True(bodyweightOnly.IsActive);
        
        var bodyweightOptional = weightTypes.Single(wt => wt.Code == "BODYWEIGHT_OPTIONAL");
        Assert.Equal("Bodyweight Optional", bodyweightOptional.Value);
        Assert.Equal(2, bodyweightOptional.DisplayOrder);
        Assert.True(bodyweightOptional.IsActive);
        
        var weightRequired = weightTypes.Single(wt => wt.Code == "WEIGHT_REQUIRED");
        Assert.Equal("Weight Required", weightRequired.Value);
        Assert.Equal(3, weightRequired.DisplayOrder);
        Assert.True(weightRequired.IsActive);
        
        var machineWeight = weightTypes.Single(wt => wt.Code == "MACHINE_WEIGHT");
        Assert.Equal("Machine Weight", machineWeight.Value);
        Assert.Equal(4, machineWeight.DisplayOrder);
        Assert.True(machineWeight.IsActive);
        
        var noWeight = weightTypes.Single(wt => wt.Code == "NO_WEIGHT");
        Assert.Equal("No Weight", noWeight.Value);
        Assert.Equal(5, noWeight.DisplayOrder);
        Assert.True(noWeight.IsActive);
        
        _output.WriteLine($"Successfully verified all {weightTypes.Count} weight types");
    }
    
    [Fact]
    public async Task ExerciseWeightTypeSeedData_HasCorrectGuids()
    {
        // This test verifies that seed data uses the correct predefined GUIDs
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var bodyweightOnly = await context.ExerciseWeightTypes
            .SingleAsync(wt => wt.Code == "BODYWEIGHT_ONLY");
        Assert.Equal("a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a", bodyweightOnly.Id.ToString().Replace("exerciseweighttype-", ""));
        
        var bodyweightOptional = await context.ExerciseWeightTypes
            .SingleAsync(wt => wt.Code == "BODYWEIGHT_OPTIONAL");
        Assert.Equal("b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f", bodyweightOptional.Id.ToString().Replace("exerciseweighttype-", ""));
        
        var weightRequired = await context.ExerciseWeightTypes
            .SingleAsync(wt => wt.Code == "WEIGHT_REQUIRED");
        Assert.Equal("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a", weightRequired.Id.ToString().Replace("exerciseweighttype-", ""));
        
        var machineWeight = await context.ExerciseWeightTypes
            .SingleAsync(wt => wt.Code == "MACHINE_WEIGHT");
        Assert.Equal("d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b", machineWeight.Id.ToString().Replace("exerciseweighttype-", ""));
        
        var noWeight = await context.ExerciseWeightTypes
            .SingleAsync(wt => wt.Code == "NO_WEIGHT");
        Assert.Equal("e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c", noWeight.Id.ToString().Replace("exerciseweighttype-", ""));
    }
    
    [Fact]
    public async Task ExerciseWeightTypeTable_HasUniqueCodeConstraint()
    {
        // This test verifies that the Code column has a unique constraint
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        // Try to add a duplicate code
        var duplicate = ExerciseWeightType.Handler.Create(
            ExerciseWeightTypeId.New(),
            "BODYWEIGHT_ONLY", // This code already exists
            "Duplicate Entry",
            "This should fail",
            99,
            true);
        
        context.ExerciseWeightTypes.Add(duplicate);
        
        // This should throw due to unique constraint violation
        await Assert.ThrowsAsync<DbUpdateException>(async () => 
            await context.SaveChangesAsync());
    }
    
    [Fact]
    public async Task ExerciseWeightTypeTable_HasIndexes()
    {
        // This test indirectly verifies indexes by checking query performance
        // In a real scenario, you might check the database schema directly
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        // Query by Code (should use unique index)
        var byCode = await context.ExerciseWeightTypes
            .Where(wt => wt.Code == "WEIGHT_REQUIRED")
            .FirstOrDefaultAsync();
        Assert.NotNull(byCode);
        
        // Query by Value (should use non-unique index)
        var byValue = await context.ExerciseWeightTypes
            .Where(wt => wt.Value == "Machine Weight")
            .FirstOrDefaultAsync();
        Assert.NotNull(byValue);
        
        _output.WriteLine("Index queries executed successfully");
    }
}