using System.Net.Http.Headers;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace GetFitterGetBigger.API.Tests;

/// <summary>
/// Test fixture that maintains a single database instance across all requests within a test class.
/// This solves the issue of NotFound errors when retrieving entities created in previous requests.
/// </summary>
public class SharedDatabaseTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private string DatabaseName { get; } = $"InMemoryDbForTesting_{Guid.NewGuid()}";
    
    public async Task InitializeAsync()
    {
        // Ensure database is seeded once when the fixture is created
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        await context.Database.EnsureCreatedAsync();
        await SeedTestDataAsync(context);
    }

    public new Task DisposeAsync() => Task.CompletedTask;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: false);
        });
        
        builder.ConfigureServices(services =>
        {
            // Remove any existing database-related services
            var descriptorsToRemove = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<FitnessDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(FitnessDbContext) ||
                (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)) ||
                (d.ServiceType.FullName?.Contains("Npgsql") == true)
            ).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }
            
            // Add DbContext using the shared in-memory database
            services.AddDbContext<FitnessDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseName);
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });
        });
    }
    
    /// <summary>
    /// Creates a new scope and provides access to the database context for test setup/verification
    /// </summary>
    public async Task<T> ExecuteDbContextAsync<T>(Func<FitnessDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        return await action(context);
    }
    
    /// <summary>
    /// Creates a new scope and provides access to the database context for test setup/verification
    /// </summary>
    public async Task ExecuteDbContextAsync(Func<FitnessDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        await action(context);
    }
    
    private async Task SeedTestDataAsync(FitnessDbContext context)
    {
        // Seed BodyParts
        if (!await context.BodyParts.AnyAsync())
        {
            context.BodyParts.AddRange(
                BodyPart.Handler.Create(
                    BodyPartId.From(Guid.Parse("7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c")),
                    "Chest",
                    "Chest muscles including pectoralis major and minor",
                    1,
                    true
                ),
                BodyPart.Handler.Create(
                    BodyPartId.From(Guid.Parse("b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a")),
                    "Back",
                    "Back muscles including latissimus dorsi and trapezius",
                    2,
                    true
                ),
                BodyPart.Handler.Create(
                    BodyPartId.From(Guid.Parse("4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5")),
                    "Legs",
                    "Leg muscles including quadriceps and hamstrings",
                    3,
                    true
                ),
                BodyPart.Handler.Create(
                    BodyPartId.From(Guid.Parse("d7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a")),
                    "Shoulders",
                    "Shoulder muscles including deltoids",
                    4,
                    true
                )
            );
        }

        // Seed DifficultyLevels
        if (!await context.DifficultyLevels.AnyAsync())
        {
            context.DifficultyLevels.AddRange(
                DifficultyLevel.Handler.Create(
                    DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b")),
                    "Beginner",
                    "Suitable for those new to fitness",
                    1,
                    true
                ),
                DifficultyLevel.Handler.Create(
                    DifficultyLevelId.From(Guid.Parse("9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a")),
                    "Intermediate",
                    "Suitable for those with some fitness experience",
                    2,
                    true
                ),
                DifficultyLevel.Handler.Create(
                    DifficultyLevelId.From(Guid.Parse("3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c")),
                    "Advanced",
                    "Suitable for those with significant fitness experience",
                    3,
                    true
                )
            );
        }

        // Seed MuscleRoles
        if (!await context.MuscleRoles.AnyAsync())
        {
            context.MuscleRoles.AddRange(
                MuscleRole.Handler.Create(
                    MuscleRoleId.From(Guid.Parse("abcdef12-3456-7890-abcd-ef1234567890")),
                    "Primary",
                    "The main muscle targeted by the exercise",
                    1,
                    true
                ),
                MuscleRole.Handler.Create(
                    MuscleRoleId.From(Guid.Parse("11223344-5566-7788-99aa-bbccddeeff00")),
                    "Secondary",
                    "A muscle that assists in the exercise",
                    2,
                    true
                ),
                MuscleRole.Handler.Create(
                    MuscleRoleId.From(Guid.Parse("22334455-6677-8899-aabb-ccddeeff0011")),
                    "Stabilizer",
                    "A muscle that helps stabilize the body during the exercise",
                    3,
                    true
                )
            );
        }
        
        // Seed Equipment
        if (!await context.Equipment.AnyAsync())
        {
            context.Equipment.AddRange(
                Equipment.Handler.Create(
                    EquipmentId.From(Guid.Parse("33445566-7788-99aa-bbcc-ddeeff001122")),
                    "Barbell"
                ),
                Equipment.Handler.Create(
                    EquipmentId.From(Guid.Parse("44556677-8899-aabb-ccdd-eeff00112233")),
                    "Dumbbell"
                ),
                Equipment.Handler.Create(
                    EquipmentId.From(Guid.Parse("55667788-99aa-bbcc-ddee-ff0011223344")),
                    "Kettlebell"
                )
            );
        }
        
        // Seed MovementPatterns
        if (!await context.MovementPatterns.AnyAsync())
        {
            context.MovementPatterns.AddRange(
                MovementPattern.Handler.Create(
                    MovementPatternId.From(Guid.Parse("99aabbcc-ddee-ff00-1122-334455667788")),
                    "Push",
                    "Pushing movement away from the body"
                ),
                MovementPattern.Handler.Create(
                    MovementPatternId.From(Guid.Parse("aabbccdd-eeff-0011-2233-445566778899")),
                    "Pull",
                    "Pulling movement towards the body"
                ),
                MovementPattern.Handler.Create(
                    MovementPatternId.From(Guid.Parse("bbccddee-ff00-1122-3344-556677889900")),
                    "Squat",
                    "Bending at the knees and hips"
                )
            );
        }
        
        // Seed MuscleGroups
        if (!await context.MuscleGroups.AnyAsync())
        {
            var chestId = BodyPartId.From(Guid.Parse("7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"));
            var backId = BodyPartId.From(Guid.Parse("b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a"));
            var legsId = BodyPartId.From(Guid.Parse("4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5"));
            
            context.MuscleGroups.AddRange(
                MuscleGroup.Handler.Create(
                    MuscleGroupId.From(Guid.Parse("ccddeeff-0011-2233-4455-667788990011")),
                    "Pectoralis",
                    chestId
                ),
                MuscleGroup.Handler.Create(
                    MuscleGroupId.From(Guid.Parse("ddeeff00-1122-3344-5566-778899001122")),
                    "Latissimus Dorsi",
                    backId
                ),
                MuscleGroup.Handler.Create(
                    MuscleGroupId.From(Guid.Parse("eeff0011-2233-4455-6677-889900112233")),
                    "Quadriceps",
                    legsId
                )
            );
        }

        // Seed ExerciseTypes
        if (!await context.ExerciseTypes.AnyAsync())
        {
            context.ExerciseTypes.AddRange(
                ExerciseType.Handler.Create(
                    ExerciseTypeId.From(Guid.Parse("11223344-5566-7788-99aa-bbccddeeff00")),
                    "Warmup",
                    "Exercises for warming up",
                    1,
                    true
                ),
                ExerciseType.Handler.Create(
                    ExerciseTypeId.From(Guid.Parse("b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e")),
                    "Workout",
                    "Main workout exercises",
                    2,
                    true
                ),
                ExerciseType.Handler.Create(
                    ExerciseTypeId.From(Guid.Parse("33445566-7788-99aa-bbcc-ddeeff001122")),
                    "Cooldown",
                    "Exercises for cooling down",
                    3,
                    true
                ),
                ExerciseType.Handler.Create(
                    ExerciseTypeId.From(Guid.Parse("44556677-8899-aabb-ccdd-eeff00112233")),
                    "Rest",
                    "Rest period",
                    4,
                    true
                )
            );
        }

        await context.SaveChangesAsync();
    }
}

/// <summary>
/// Collection definition for tests that should share the same database instance
/// </summary>
[CollectionDefinition("SharedDatabase")]
public class SharedDatabaseCollection : ICollectionFixture<SharedDatabaseTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}