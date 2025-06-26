
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GetFitterGetBigger.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GetFitterGetBigger.API.Tests;

public class ApiTestFixture : WebApplicationFactory<Program>
{

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: false);
        });

        builder.ConfigureServices((context, services) =>
        {
            // Remove all DbContext registrations
            var descriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<FitnessDbContext>) ||
                     d.ServiceType == typeof(DbContextOptions) ||
                     (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Remove Npgsql registrations
            var npgsqlDescriptors = services.Where(
                d => d.ServiceType.FullName?.Contains("Npgsql") == true).ToList();

            foreach (var descriptor in npgsqlDescriptors)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using an in-memory database for testing with a unique name
            services.AddDbContext<FitnessDbContext>(options =>
            {
                options.UseInMemoryDatabase($"InMemoryDbForTesting_{Guid.NewGuid()}");
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });
        });

        var host = base.CreateHost(builder);

        // Seed the database after the host is created
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var db = services.GetRequiredService<FitnessDbContext>();
            
            // Ensure the database is created
            db.Database.EnsureCreated();
            
            // Seed the database with test data
            SeedTestData(db);
        }

        return host;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    private void SeedTestData(FitnessDbContext context)
    {
        // No need to clear data as we're using a new database for each test
        
        // Seed BodyParts
        if (!context.BodyParts.Any())
        {
            context.BodyParts.AddRange(
                Models.Entities.BodyPart.Handler.Create(
                    Models.SpecializedIds.BodyPartId.From(Guid.Parse("7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c")),
                    "Chest",
                    "Chest muscles including pectoralis major and minor",
                    1,
                    true
                ),
                Models.Entities.BodyPart.Handler.Create(
                    Models.SpecializedIds.BodyPartId.From(Guid.Parse("b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a")),
                    "Back",
                    "Back muscles including latissimus dorsi and trapezius",
                    2,
                    true
                ),
                Models.Entities.BodyPart.Handler.Create(
                    Models.SpecializedIds.BodyPartId.From(Guid.Parse("4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5")),
                    "Legs",
                    "Leg muscles including quadriceps and hamstrings",
                    3,
                    true
                ),
                Models.Entities.BodyPart.Handler.Create(
                    Models.SpecializedIds.BodyPartId.From(Guid.Parse("d7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a")),
                    "Shoulders",
                    "Shoulder muscles including deltoids",
                    4,
                    true
                )
            );
        }

        // Seed DifficultyLevels
        if (!context.DifficultyLevels.Any())
        {
            context.DifficultyLevels.AddRange(
                Models.Entities.DifficultyLevel.Handler.Create(
                    Models.SpecializedIds.DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b")),
                    "Beginner",
                    "Suitable for those new to fitness",
                    1,
                    true
                ),
                Models.Entities.DifficultyLevel.Handler.Create(
                    Models.SpecializedIds.DifficultyLevelId.From(Guid.Parse("9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a")),
                    "Intermediate",
                    "Suitable for those with some fitness experience",
                    2,
                    true
                ),
                Models.Entities.DifficultyLevel.Handler.Create(
                    Models.SpecializedIds.DifficultyLevelId.From(Guid.Parse("3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c")),
                    "Advanced",
                    "Suitable for those with significant fitness experience",
                    3,
                    true
                )
            );
        }

        // Seed KineticChainTypes
        if (!context.KineticChainTypes.Any())
        {
            context.KineticChainTypes.AddRange(
                Models.Entities.KineticChainType.Handler.Create(
                    Models.SpecializedIds.KineticChainTypeId.From(Guid.Parse("f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4")),
                    "Compound",
                    "Exercises that work multiple muscle groups",
                    1,
                    true
                ),
                Models.Entities.KineticChainType.Handler.Create(
                    Models.SpecializedIds.KineticChainTypeId.From(Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890")),
                    "Isolation",
                    "Exercises that work a single muscle group",
                    2,
                    true
                )
            );
        }

        // Seed MuscleRoles
        if (!context.MuscleRoles.Any())
        {
            context.MuscleRoles.AddRange(
                Models.Entities.MuscleRole.Handler.Create(
                    Models.SpecializedIds.MuscleRoleId.From(Guid.Parse("abcdef12-3456-7890-abcd-ef1234567890")),
                    "Primary",
                    "The main muscle targeted by the exercise",
                    1,
                    true
                ),
                Models.Entities.MuscleRole.Handler.Create(
                    Models.SpecializedIds.MuscleRoleId.From(Guid.Parse("11223344-5566-7788-99aa-bbccddeeff00")),
                    "Secondary",
                    "A muscle that assists in the exercise",
                    2,
                    true
                ),
                Models.Entities.MuscleRole.Handler.Create(
                    Models.SpecializedIds.MuscleRoleId.From(Guid.Parse("22334455-6677-8899-aabb-ccddeeff0011")),
                    "Stabilizer",
                    "A muscle that helps stabilize the body during the exercise",
                    3,
                    true
                )
            );
        }
        
        // Seed Equipment
        if (!context.Equipment.Any())
        {
            context.Equipment.AddRange(
                Models.Entities.Equipment.Handler.Create(
                    Models.SpecializedIds.EquipmentId.From(Guid.Parse("33445566-7788-99aa-bbcc-ddeeff001122")),
                    "Barbell"
                ),
                Models.Entities.Equipment.Handler.Create(
                    Models.SpecializedIds.EquipmentId.From(Guid.Parse("44556677-8899-aabb-ccdd-eeff00112233")),
                    "Dumbbell"
                ),
                Models.Entities.Equipment.Handler.Create(
                    Models.SpecializedIds.EquipmentId.From(Guid.Parse("55667788-99aa-bbcc-ddee-ff0011223344")),
                    "Kettlebell"
                )
            );
        }
        
        // Seed MetricTypes
        if (!context.MetricTypes.Any())
        {
            context.MetricTypes.AddRange(
                Models.Entities.MetricType.Handler.Create(
                    Models.SpecializedIds.MetricTypeId.From(Guid.Parse("66778899-aabb-ccdd-eeff-001122334455")),
                    "Weight"
                ),
                Models.Entities.MetricType.Handler.Create(
                    Models.SpecializedIds.MetricTypeId.From(Guid.Parse("778899aa-bbcc-ddee-ff00-112233445566")),
                    "Reps"
                ),
                Models.Entities.MetricType.Handler.Create(
                    Models.SpecializedIds.MetricTypeId.From(Guid.Parse("8899aabb-ccdd-eeff-0011-223344556677")),
                    "Time"
                )
            );
        }
        
        // Seed MovementPatterns
        if (!context.MovementPatterns.Any())
        {
            context.MovementPatterns.AddRange(
                Models.Entities.MovementPattern.Handler.Create(
                    Models.SpecializedIds.MovementPatternId.From(Guid.Parse("99aabbcc-ddee-ff00-1122-334455667788")),
                    "Push",
                    "Pushing movement away from the body"
                ),
                Models.Entities.MovementPattern.Handler.Create(
                    Models.SpecializedIds.MovementPatternId.From(Guid.Parse("aabbccdd-eeff-0011-2233-445566778899")),
                    "Pull",
                    "Pulling movement towards the body"
                ),
                Models.Entities.MovementPattern.Handler.Create(
                    Models.SpecializedIds.MovementPatternId.From(Guid.Parse("bbccddee-ff00-1122-3344-556677889900")),
                    "Squat",
                    "Bending at the knees and hips"
                )
            );
        }
        
        // Seed MuscleGroups
        if (!context.MuscleGroups.Any())
        {
            var chestId = Models.SpecializedIds.BodyPartId.From(Guid.Parse("7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"));
            var backId = Models.SpecializedIds.BodyPartId.From(Guid.Parse("b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a"));
            var legsId = Models.SpecializedIds.BodyPartId.From(Guid.Parse("4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5"));
            
            context.MuscleGroups.AddRange(
                Models.Entities.MuscleGroup.Handler.Create(
                    Models.SpecializedIds.MuscleGroupId.From(Guid.Parse("ccddeeff-0011-2233-4455-667788990011")),
                    "Pectoralis",
                    chestId
                ),
                Models.Entities.MuscleGroup.Handler.Create(
                    Models.SpecializedIds.MuscleGroupId.From(Guid.Parse("ddeeff00-1122-3344-5566-778899001122")),
                    "Latissimus Dorsi",
                    backId
                ),
                Models.Entities.MuscleGroup.Handler.Create(
                    Models.SpecializedIds.MuscleGroupId.From(Guid.Parse("eeff0011-2233-4455-6677-889900112233")),
                    "Quadriceps",
                    legsId
                )
            );
        }

        context.SaveChanges();
    }
}
