using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.EntityFrameworkCore;

namespace GetFitterGetBigger.API.IntegrationTests.TestBuilders;

/// <summary>
/// Builder for creating consistent seed data across all tests
/// Uses "check and insert if not exists" pattern to avoid duplicate key errors
/// </summary>
public class SeedDataBuilder
{
    private readonly FitnessDbContext _context;
    private readonly List<Exercise> _exercises = new();
    
    // Standard IDs for consistency
    public static class StandardIds
    {
        // String versions for easy test usage (replaces TestConstants)
        public static class ExerciseTypeIds
        {
            public static readonly string Warmup = $"exercisetype-{WarmupExerciseTypeId}";
            public static readonly string Workout = $"exercisetype-{WorkoutExerciseTypeId}";
            public static readonly string Cooldown = $"exercisetype-{CooldownExerciseTypeId}";
            public static readonly string Rest = $"exercisetype-{RestExerciseTypeId}";
        }

        public static class DifficultyLevelIds
        {
            public static readonly string Beginner = $"difficultylevel-{BeginnerDifficultyId}";
            public static readonly string Intermediate = $"difficultylevel-{IntermediateDifficultyId}";
            public static readonly string Advanced = $"difficultylevel-{AdvancedDifficultyId}";
        }

        public static class KineticChainTypeIds
        {
            public static readonly string Compound = $"kineticchaintype-{CompoundKineticChainId}";
            public static readonly string Isolation = $"kineticchaintype-{IsolationKineticChainId}";
        }

        public static class ExerciseWeightTypeIds
        {
            public static readonly string BodyweightOnly = $"exerciseweighttype-{BodyweightOnlyWeightTypeId}";
            public static readonly string BodyweightOptional = $"exerciseweighttype-{BodyweightOptionalWeightTypeId}";
            public static readonly string WeightRequired = $"exerciseweighttype-{WeightRequiredWeightTypeId}";
            public static readonly string MachineWeight = $"exerciseweighttype-{MachineWeightWeightTypeId}";
            public static readonly string NoWeight = $"exerciseweighttype-{NoWeightWeightTypeId}";
        }

        public static class MuscleGroupIds
        {
            public static readonly string Chest = $"musclegroup-{ChestMuscleGroupId}";
            public static readonly string Back = $"musclegroup-{BackMuscleGroupId}";
            public static readonly string Quadriceps = $"musclegroup-{QuadricepsMuscleGroupId}";
            public static readonly string Hamstrings = $"musclegroup-{HamstringsMuscleGroupId}";
            public static readonly string Glutes = $"musclegroup-{GlutesMuscleGroupId}";
            public static readonly string Shoulders = $"musclegroup-{ShouldersMuscleGroupId}";
            public static readonly string Biceps = $"musclegroup-{BicepsMuscleGroupId}";
            public static readonly string Triceps = $"musclegroup-{TricepsMuscleGroupId}";
            public static readonly string Calves = $"musclegroup-{CalvesMuscleGroupId}";
            public static readonly string Abs = $"musclegroup-{AbsMuscleGroupId}";
        }

        public static class MuscleRoleIds
        {
            public static readonly string Primary = $"musclerole-{PrimaryMuscleRoleId}";
            public static readonly string Secondary = $"musclerole-{SecondaryMuscleRoleId}";
            public static readonly string Stabilizer = $"musclerole-{StabilizerMuscleRoleId}";
        }

        public static class EquipmentIds
        {
            public static readonly string Barbell = $"equipment-{BarbellEquipmentId}";
            public static readonly string Dumbbell = $"equipment-{DumbbellEquipmentId}";
            public static readonly string Kettlebell = $"equipment-{KettlebellEquipmentId}";
            public static readonly string Cable = $"equipment-{CableEquipmentId}";
            public static readonly string Machine = $"equipment-{MachineEquipmentId}";
        }

        public static class MovementPatternIds
        {
            public static readonly string HorizontalPush = $"movementpattern-{HorizontalPushMovementPatternId}";
            public static readonly string HorizontalPull = $"movementpattern-{HorizontalPullMovementPatternId}";
            public static readonly string Squat = $"movementpattern-{SquatMovementPatternId}";
            public static readonly string VerticalPush = $"movementpattern-{VerticalPushMovementPatternId}";
            public static readonly string VerticalPull = $"movementpattern-{VerticalPullMovementPatternId}";
            public static readonly string Hinge = $"movementpattern-{HingeMovementPatternId}";
            public static readonly string Lunge = $"movementpattern-{LungeMovementPatternId}";
            public static readonly string Carry = $"movementpattern-{CarryMovementPatternId}";
            public static readonly string Rotation = $"movementpattern-{RotationMovementPatternId}";
        }

        public static class BodyPartIds
        {
            public static readonly string Chest = $"bodypart-{ChestBodyPartId}";
            public static readonly string Back = $"bodypart-{BackBodyPartId}";
            public static readonly string Legs = $"bodypart-{LegsBodyPartId}";
            public static readonly string Shoulders = $"bodypart-{ShouldersBodyPartId}";
            public static readonly string Arms = $"bodypart-{ArmsBodyPartId}";
            public static readonly string Core = $"bodypart-{CoreBodyPartId}";
        }

        public static class WorkoutObjectiveIds
        {
            public static readonly string MuscularStrength = $"workoutobjective-{MuscularStrengthObjectiveId}";
            public static readonly string MuscularHypertrophy = $"workoutobjective-{MuscularHypertrophyObjectiveId}";
            public static readonly string MuscularEndurance = $"workoutobjective-{MuscularEnduranceObjectiveId}";
            public static readonly string PowerDevelopment = $"workoutobjective-{PowerDevelopmentObjectiveId}";
        }

        public static class WorkoutCategoryIds
        {
            public static readonly string UpperBodyPush = $"workoutcategory-{UpperBodyPushCategoryId}";
            public static readonly string UpperBodyPull = $"workoutcategory-{UpperBodyPullCategoryId}";
            public static readonly string LowerBody = $"workoutcategory-{LowerBodyCategoryId}";
            public static readonly string Core = $"workoutcategory-{CoreCategoryId}";
            public static readonly string FullBody = $"workoutcategory-{FullBodyCategoryId}";
        }

        public static class ExecutionProtocolIds
        {
            public static readonly string Standard = $"executionprotocol-{StandardProtocolId}";
            public static readonly string Superset = $"executionprotocol-{SupersetProtocolId}";
            public static readonly string DropSet = $"executionprotocol-{DropSetProtocolId}";
            public static readonly string AMRAP = $"executionprotocol-{AMRAPProtocolId}";
            public static readonly string RestPause = $"executionprotocol-{RestPauseProtocolId}";
        }

        /// <summary>
        /// Generate a new GUID-based ID for the given entity type
        /// Use this when you need unique IDs that don't conflict with seeded data
        /// </summary>
        public static string NewId(string entityType)
        {
            return $"{entityType.ToLowerInvariant()}-{Guid.NewGuid()}";
        }

        // GUID versions (existing)
        // BodyParts - matching FitnessDbContext.cs exactly
        public static readonly Guid ChestBodyPartId = Guid.Parse("7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c");
        public static readonly Guid BackBodyPartId = Guid.Parse("b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a");
        public static readonly Guid LegsBodyPartId = Guid.Parse("4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5");
        public static readonly Guid ShouldersBodyPartId = Guid.Parse("d7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a");
        public static readonly Guid ArmsBodyPartId = Guid.Parse("9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1");
        public static readonly Guid CoreBodyPartId = Guid.Parse("3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c");
        
        // DifficultyLevels - matching FitnessDbContext.cs
        public static readonly Guid BeginnerDifficultyId = Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b");
        public static readonly Guid IntermediateDifficultyId = Guid.Parse("9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a");
        public static readonly Guid AdvancedDifficultyId = Guid.Parse("3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c");
        
        // KineticChainTypes - matching FitnessDbContext.cs
        public static readonly Guid CompoundKineticChainId = Guid.Parse("f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4");
        public static readonly Guid IsolationKineticChainId = Guid.Parse("2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b");
        
        // MuscleRoles - matching FitnessDbContext.cs exactly
        public static readonly Guid PrimaryMuscleRoleId = Guid.Parse("5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b");
        public static readonly Guid SecondaryMuscleRoleId = Guid.Parse("8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a");
        public static readonly Guid StabilizerMuscleRoleId = Guid.Parse("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d");
        
        // Equipment
        public static readonly Guid BarbellEquipmentId = Guid.Parse("33445566-7788-99aa-bbcc-ddeeff001122");
        public static readonly Guid DumbbellEquipmentId = Guid.Parse("44556677-8899-aabb-ccdd-eeff00112233");
        public static readonly Guid KettlebellEquipmentId = Guid.Parse("55667788-99aa-bbcc-ddee-ff0011223344");
        public static readonly Guid CableEquipmentId = Guid.Parse("66778899-aabb-ccdd-eeff-001122334455");
        public static readonly Guid MachineEquipmentId = Guid.Parse("778899aa-bbcc-ddee-ff00-112233445566");
        
        // MetricTypes
        public static readonly Guid WeightMetricTypeId = Guid.Parse("66778899-aabb-ccdd-eeff-001122334455");
        public static readonly Guid RepsMetricTypeId = Guid.Parse("778899aa-bbcc-ddee-ff00-112233445566");
        public static readonly Guid TimeMetricTypeId = Guid.Parse("8899aabb-ccdd-eeff-0011-223344556677");
        
        // MovementPatterns
        public static readonly Guid SquatMovementPatternId = Guid.Parse("bbccddee-ff00-1122-3344-556677889900");
        public static readonly Guid HingeMovementPatternId = Guid.Parse("a760cc8f-b32e-408b-b0ec-1ba053ee4bed");
        public static readonly Guid LungeMovementPatternId = Guid.Parse("5c4b8fe7-a66e-4be2-9e3f-30c1de46e7ad");
        public static readonly Guid HorizontalPushMovementPatternId = Guid.Parse("99aabbcc-ddee-ff00-1122-334455667788");
        public static readonly Guid VerticalPushMovementPatternId = Guid.Parse("71b77ae2-e8d2-4547-bd90-b7a69d975124");
        public static readonly Guid HorizontalPullMovementPatternId = Guid.Parse("aabbccdd-eeff-0011-2233-445566778899");
        public static readonly Guid VerticalPullMovementPatternId = Guid.Parse("efab6dba-4bcd-4381-9fd1-cbb86f1f2301");
        public static readonly Guid CarryMovementPatternId = Guid.Parse("a2c67018-196d-45ff-b596-c2d8bc845c20");
        public static readonly Guid RotationMovementPatternId = Guid.Parse("9019d05b-c822-4aa9-8181-751f16cfbc75");
        
        // MuscleGroups
        public static readonly Guid ChestMuscleGroupId = Guid.Parse("ccddeeff-0011-2233-4455-667788990011");
        public static readonly Guid BackMuscleGroupId = Guid.Parse("ddeeff00-1122-3344-5566-778899001122");
        public static readonly Guid QuadricepsMuscleGroupId = Guid.Parse("eeff0011-2233-4455-6677-889900112233");
        public static readonly Guid HamstringsMuscleGroupId = Guid.Parse("ff001122-3344-5566-7788-990011223344");
        public static readonly Guid GlutesMuscleGroupId = Guid.Parse("00112233-4455-6677-8899-001122334455");
        public static readonly Guid ShouldersMuscleGroupId = Guid.Parse("11223344-5566-7788-99aa-112233445566");
        public static readonly Guid BicepsMuscleGroupId = Guid.Parse("22334455-6677-8899-aabb-223344556677");
        public static readonly Guid TricepsMuscleGroupId = Guid.Parse("33445566-7788-99aa-bbcc-334455667788");
        public static readonly Guid CalvesMuscleGroupId = Guid.Parse("44556677-8899-aabb-ccdd-445566778899");
        public static readonly Guid AbsMuscleGroupId = Guid.Parse("55667788-99aa-bbcc-ddee-556677889900");
        
        // ExerciseTypes - matching FitnessDbContext.cs exactly
        public static readonly Guid WarmupExerciseTypeId = Guid.Parse("a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d");
        public static readonly Guid WorkoutExerciseTypeId = Guid.Parse("b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e");
        public static readonly Guid CooldownExerciseTypeId = Guid.Parse("c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f");
        public static readonly Guid RestExerciseTypeId = Guid.Parse("d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a");
        
        // ExerciseWeightTypes - matching FitnessDbContext.cs exactly
        public static readonly Guid BodyweightOnlyWeightTypeId = Guid.Parse("a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a");
        public static readonly Guid BodyweightOptionalWeightTypeId = Guid.Parse("b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f");
        public static readonly Guid WeightRequiredWeightTypeId = Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a");
        public static readonly Guid MachineWeightWeightTypeId = Guid.Parse("d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b");
        public static readonly Guid NoWeightWeightTypeId = Guid.Parse("e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c");
        
        // WorkoutObjectives - Using WO prefix for uniqueness
        public static readonly Guid MuscularStrengthObjectiveId = Guid.Parse("10000001-1000-4000-8000-100000000001");
        public static readonly Guid MuscularHypertrophyObjectiveId = Guid.Parse("10000001-1000-4000-8000-100000000002");
        public static readonly Guid MuscularEnduranceObjectiveId = Guid.Parse("10000001-1000-4000-8000-100000000003");
        public static readonly Guid PowerDevelopmentObjectiveId = Guid.Parse("10000001-1000-4000-8000-100000000004");
        
        // WorkoutCategories - Using WC prefix for uniqueness
        public static readonly Guid UpperBodyPushCategoryId = Guid.Parse("20000002-2000-4000-8000-200000000001");
        public static readonly Guid UpperBodyPullCategoryId = Guid.Parse("20000002-2000-4000-8000-200000000002");
        public static readonly Guid LowerBodyCategoryId = Guid.Parse("20000002-2000-4000-8000-200000000003");
        public static readonly Guid CoreCategoryId = Guid.Parse("20000002-2000-4000-8000-200000000004");
        public static readonly Guid FullBodyCategoryId = Guid.Parse("20000002-2000-4000-8000-200000000005");
        
        // ExecutionProtocols - Using EP prefix for uniqueness
        public static readonly Guid StandardProtocolId = Guid.Parse("30000003-3000-4000-8000-300000000001");
        public static readonly Guid SupersetProtocolId = Guid.Parse("30000003-3000-4000-8000-300000000002");
        public static readonly Guid DropSetProtocolId = Guid.Parse("30000003-3000-4000-8000-300000000003");
        public static readonly Guid AMRAPProtocolId = Guid.Parse("30000003-3000-4000-8000-300000000004");
        public static readonly Guid RestPauseProtocolId = Guid.Parse("30000003-3000-4000-8000-300000000005");
    }
    
    public SeedDataBuilder(FitnessDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Ensures BodyParts exist in the database. Only inserts if they don't already exist.
    /// </summary>
    public async Task<SeedDataBuilder> WithBodyPartsAsync()
    {
        var bodyPartsToCheck = new[]
        {
            (StandardIds.ChestBodyPartId, "Chest", "Chest muscles including pectoralis major and minor", 1),
            (StandardIds.BackBodyPartId, "Back", "Back muscles including latissimus dorsi and trapezius", 2),
            (StandardIds.LegsBodyPartId, "Legs", "Leg muscles including quadriceps and hamstrings", 3),
            (StandardIds.ShouldersBodyPartId, "Shoulders", "Shoulder muscles including deltoids", 4),
            (StandardIds.ArmsBodyPartId, "Arms", "Arm muscles including biceps and triceps", 5),
            (StandardIds.CoreBodyPartId, "Core", "Core muscles including abs and obliques", 6)
        };

        foreach (var (id, name, description, displayOrder) in bodyPartsToCheck)
        {
            var bodyPartId = BodyPartId.From(id);
            var exists = await _context.BodyParts.AnyAsync(bp => bp.BodyPartId == bodyPartId);
            
            if (!exists)
            {
                var bodyPart = BodyPart.Handler.Create(bodyPartId, name, description, displayOrder, true).Value;
                await _context.BodyParts.AddAsync(bodyPart);
            }
        }
        
        await _context.SaveChangesAsync();
        return this;
    }
    
    /// <summary>
    /// Ensures DifficultyLevels exist in the database. Only inserts if they don't already exist.
    /// </summary>
    public async Task<SeedDataBuilder> WithDifficultyLevelsAsync()
    {
        var difficultyLevelsToCheck = new[]
        {
            (StandardIds.BeginnerDifficultyId, "Beginner", "Suitable for those new to fitness", 1),
            (StandardIds.IntermediateDifficultyId, "Intermediate", "Suitable for those with some fitness experience", 2),
            (StandardIds.AdvancedDifficultyId, "Advanced", "Suitable for those with significant fitness experience", 3)
        };

        foreach (var (id, name, description, displayOrder) in difficultyLevelsToCheck)
        {
            var difficultyId = DifficultyLevelId.From(id);
            var exists = await _context.DifficultyLevels.AnyAsync(dl => dl.DifficultyLevelId == difficultyId);
            
            if (!exists)
            {
                var difficulty = DifficultyLevel.Handler.Create(difficultyId, name, description, displayOrder, true).Value!;
                await _context.DifficultyLevels.AddAsync(difficulty);
            }
        }
        
        await _context.SaveChangesAsync();
        return this;
    }
    
    /// <summary>
    /// Ensures KineticChainTypes exist in the database. Only inserts if they don't already exist.
    /// </summary>
    public async Task<SeedDataBuilder> WithKineticChainTypesAsync()
    {
        var kineticChainTypesToCheck = new[]
        {
            (StandardIds.CompoundKineticChainId, "Compound", "Exercises that work multiple muscle groups", 1),
            (StandardIds.IsolationKineticChainId, "Isolation", "Exercises that work a single muscle group", 2)
        };

        foreach (var (id, name, description, displayOrder) in kineticChainTypesToCheck)
        {
            var kineticChainId = KineticChainTypeId.From(id);
            var exists = await _context.KineticChainTypes.AnyAsync(kct => kct.KineticChainTypeId == kineticChainId);
            
            if (!exists)
            {
                var kineticChain = KineticChainType.Handler.Create(kineticChainId, name, description, displayOrder, true);
                await _context.KineticChainTypes.AddAsync(kineticChain.Value);
            }
        }
        
        await _context.SaveChangesAsync();
        return this;
    }
    
    /// <summary>
    /// Ensures MuscleRoles exist in the database. Only inserts if they don't already exist.
    /// </summary>
    public async Task<SeedDataBuilder> WithMuscleRolesAsync()
    {
        var muscleRolesToCheck = new[]
        {
            (StandardIds.PrimaryMuscleRoleId, "Primary", "The main muscle targeted by the exercise", 1),
            (StandardIds.SecondaryMuscleRoleId, "Secondary", "A muscle that assists in the exercise", 2),
            (StandardIds.StabilizerMuscleRoleId, "Stabilizer", "A muscle that helps stabilize the body during the exercise", 3)
        };

        foreach (var (id, name, description, displayOrder) in muscleRolesToCheck)
        {
            var muscleRoleId = MuscleRoleId.From(id);
            var exists = await _context.MuscleRoles.AnyAsync(mr => mr.Id == muscleRoleId);
            
            if (!exists)
            {
                var muscleRole = MuscleRole.Handler.Create(muscleRoleId, name, description, displayOrder, true);
                await _context.MuscleRoles.AddAsync(muscleRole);
            }
        }
        
        await _context.SaveChangesAsync();
        return this;
    }
    
    /// <summary>
    /// Ensures Equipment exist in the database. Only inserts if they don't already exist.
    /// </summary>
    public async Task<SeedDataBuilder> WithEquipmentAsync()
    {
        var equipmentToCheck = new[]
        {
            (StandardIds.BarbellEquipmentId, "Barbell"),
            (StandardIds.DumbbellEquipmentId, "Dumbbell"),
            (StandardIds.KettlebellEquipmentId, "Kettlebell"),
            (StandardIds.CableEquipmentId, "Cable"),
            (StandardIds.MachineEquipmentId, "Machine")
        };

        foreach (var (id, name) in equipmentToCheck)
        {
            var equipmentId = EquipmentId.From(id);
            var exists = await _context.Equipment.AnyAsync(e => e.Id == equipmentId);
            
            if (!exists)
            {
                var equipment = Equipment.Handler.Create(equipmentId, name);
                await _context.Equipment.AddAsync(equipment);
            }
        }
        
        await _context.SaveChangesAsync();
        return this;
    }
    
    /// <summary>
    /// Ensures MetricTypes exist in the database. Only inserts if they don't already exist.
    /// </summary>
    public async Task<SeedDataBuilder> WithMetricTypesAsync()
    {
        var metricTypesToCheck = new[]
        {
            (StandardIds.WeightMetricTypeId, "Weight"),
            (StandardIds.RepsMetricTypeId, "Reps"),
            (StandardIds.TimeMetricTypeId, "Time")
        };

        foreach (var (id, name) in metricTypesToCheck)
        {
            var metricTypeId = MetricTypeId.From(id);
            var exists = await _context.MetricTypes.AnyAsync(mt => mt.Id == metricTypeId);
            
            if (!exists)
            {
                var metricType = MetricType.Handler.Create(metricTypeId, name);
                await _context.MetricTypes.AddAsync(metricType);
            }
        }
        
        await _context.SaveChangesAsync();
        return this;
    }
    
    /// <summary>
    /// Ensures MovementPatterns exist in the database. Only inserts if they don't already exist.
    /// </summary>
    public async Task<SeedDataBuilder> WithMovementPatternsAsync()
    {
        var movementPatternsToCheck = new[]
        {
            (StandardIds.HorizontalPushMovementPatternId, "Horizontal Push", "Pushing forward, parallel to the ground. Examples: Bench Press, Push-up, Cable Chest Press."),
            (StandardIds.HorizontalPullMovementPatternId, "Horizontal Pull", "Pulling backward, parallel to the ground. Examples: Bent-Over Row, Seated Cable Row, Inverted Row."),
            (StandardIds.SquatMovementPatternId, "Squat", "A lower-body, knee-dominant movement characterized by the simultaneous bending of the hips, knees, and ankles while maintaining a relatively upright torso. Examples: Barbell Back Squat, Goblet Squat, Air Squat."),
            (StandardIds.VerticalPushMovementPatternId, "Vertical Push", "Pushing upward, overhead. Examples: Overhead Press, Dumbbell Shoulder Press, Handstand Push-up."),
            (StandardIds.VerticalPullMovementPatternId, "Vertical Pull", "Pulling downward from an overhead position. Examples: Pull-up, Chin-up, Lat Pulldown."),
            (StandardIds.HingeMovementPatternId, "Hinge", "A lower-body, hip-dominant movement involving flexion and extension primarily at the hip joint with minimal knee bend. This pattern is fundamental for lifting objects from the floor and developing the posterior chain. Examples: Deadlift, Kettlebell Swing, Good Morning."),
            (StandardIds.LungeMovementPatternId, "Lunge", "A unilateral (single-leg focused) movement pattern that challenges balance, stability, and strength in a split stance. It is a key pattern for locomotion and single-leg stability. Examples: Forward Lunge, Reverse Lunge, Bulgarian Split Squat."),
            (StandardIds.CarryMovementPatternId, "Carry", "A pattern of locomotion (walking or running) while under an external load. This is considered highly functional as it integrates core stability with grip strength and full-body coordination. Examples: Farmer's Walk, Suitcase Carry, Overhead Carry."),
            (StandardIds.RotationMovementPatternId, "Rotation/Anti-Rotation", "A core-focused pattern involving either generating rotational force (twisting) or resisting it. This is crucial for athletic power transfer and spinal stability. Examples: Medicine Ball Rotational Throw (Rotation), Pallof Press (Anti-Rotation), Bird-Dog (Anti-Rotation).")
        };

        foreach (var (id, name, description) in movementPatternsToCheck)
        {
            var movementPatternId = MovementPatternId.From(id);
            var exists = await _context.MovementPatterns.AnyAsync(mp => mp.MovementPatternId == movementPatternId);
            
            if (!exists)
            {
                var displayOrder = Array.IndexOf(movementPatternsToCheck, (id, name, description)) + 1;
                var movementPattern = MovementPattern.Handler.Create(movementPatternId, name, description, displayOrder, true).Value;
                await _context.MovementPatterns.AddAsync(movementPattern);
            }
        }
        
        await _context.SaveChangesAsync();
        return this;
    }
    
    /// <summary>
    /// Ensures ExerciseTypes exist in the database. Only inserts if they don't already exist.
    /// </summary>
    public async Task<SeedDataBuilder> WithExerciseTypesAsync()
    {
        var exerciseTypesToCheck = new[]
        {
            (StandardIds.WarmupExerciseTypeId, "Warmup", "Exercises performed to prepare the body for more intense activity", 1),
            (StandardIds.WorkoutExerciseTypeId, "Workout", "Main exercises that form the core of the training session", 2),
            (StandardIds.CooldownExerciseTypeId, "Cooldown", "Exercises performed to help the body recover after intense activity", 3),
            (StandardIds.RestExerciseTypeId, "Rest", "Periods of rest between exercises or sets - cannot be combined with other exercise types", 4)
        };

        foreach (var (id, name, description, displayOrder) in exerciseTypesToCheck)
        {
            var exerciseTypeId = ExerciseTypeId.From(id);
            var exists = await _context.ExerciseTypes.AnyAsync(et => et.ExerciseTypeId == exerciseTypeId);
            
            if (!exists)
            {
                var result = ExerciseType.Handler.Create(exerciseTypeId, name, description, displayOrder, true);
                if (result.IsSuccess)
                {
                    await _context.ExerciseTypes.AddAsync(result.Value);
                }
            }
        }
        
        await _context.SaveChangesAsync();
        return this;
    }
    
    /// <summary>
    /// Ensures ExerciseWeightTypes exist in the database. Only inserts if they don't already exist.
    /// </summary>
    public async Task<SeedDataBuilder> WithExerciseWeightTypesAsync()
    {
        var exerciseWeightTypesToCheck = new[]
        {
            (StandardIds.BodyweightOnlyWeightTypeId, "BODYWEIGHT_ONLY", "Bodyweight Only", "Exercises that cannot have external weight added", 1),
            (StandardIds.BodyweightOptionalWeightTypeId, "BODYWEIGHT_OPTIONAL", "Bodyweight Optional", "Exercises that can be performed with or without additional weight", 2),
            (StandardIds.WeightRequiredWeightTypeId, "WEIGHT_REQUIRED", "Weight Required", "Exercises that must have external weight specified", 3),
            (StandardIds.MachineWeightWeightTypeId, "MACHINE_WEIGHT", "Machine Weight", "Exercises performed on machines with weight stacks", 4),
            (StandardIds.NoWeightWeightTypeId, "NO_WEIGHT", "No Weight", "Exercises that do not involve any weight", 5)
        };

        foreach (var (id, code, value, description, displayOrder) in exerciseWeightTypesToCheck)
        {
            var exerciseWeightTypeId = ExerciseWeightTypeId.From(id);
            var exists = await _context.ExerciseWeightTypes.AnyAsync(ewt => ewt.Id == exerciseWeightTypeId);
            
            if (!exists)
            {
                var exerciseWeightType = ExerciseWeightType.Handler.Create(exerciseWeightTypeId, code, value, description, displayOrder, true).Value;
                await _context.ExerciseWeightTypes.AddAsync(exerciseWeightType);
            }
        }
        
        await _context.SaveChangesAsync();
        return this;
    }
    
    /// <summary>
    /// Ensures MuscleGroups exist in the database. Only inserts if they don't already exist.
    /// Depends on BodyParts being available (either from FitnessDbContext or WithBodyPartsAsync).
    /// </summary>
    public async Task<SeedDataBuilder> WithMuscleGroupsAsync()
    {
        var muscleGroupsToCheck = new[]
        {
            (StandardIds.ChestMuscleGroupId, "Pectoralis", StandardIds.ChestBodyPartId),
            (StandardIds.BackMuscleGroupId, "Latissimus Dorsi", StandardIds.BackBodyPartId),
            (StandardIds.QuadricepsMuscleGroupId, "Quadriceps", StandardIds.LegsBodyPartId),
            (StandardIds.HamstringsMuscleGroupId, "Hamstrings", StandardIds.LegsBodyPartId),
            (StandardIds.GlutesMuscleGroupId, "Glutes", StandardIds.LegsBodyPartId),
            (StandardIds.ShouldersMuscleGroupId, "Deltoids", StandardIds.ShouldersBodyPartId),
            (StandardIds.BicepsMuscleGroupId, "Biceps", StandardIds.ArmsBodyPartId),
            (StandardIds.TricepsMuscleGroupId, "Triceps", StandardIds.ArmsBodyPartId),
            (StandardIds.CalvesMuscleGroupId, "Calves", StandardIds.LegsBodyPartId),
            (StandardIds.AbsMuscleGroupId, "Abs", StandardIds.CoreBodyPartId)
        };

        foreach (var (id, name, bodyPartId) in muscleGroupsToCheck)
        {
            var muscleGroupId = MuscleGroupId.From(id);
            var exists = await _context.MuscleGroups.AnyAsync(mg => mg.Id == muscleGroupId);
            
            if (!exists)
            {
                var muscleGroup = MuscleGroup.Handler.Create(muscleGroupId, name, BodyPartId.From(bodyPartId));
                await _context.MuscleGroups.AddAsync(muscleGroup);
            }
        }
        
        await _context.SaveChangesAsync();
        return this;
    }
    
    /// <summary>
    /// Ensures WorkoutObjectives exist in the database. Only inserts if they don't already exist.
    /// </summary>
    public async Task<SeedDataBuilder> WithWorkoutObjectivesAsync()
    {
        var workoutObjectivesToCheck = new[]
        {
            (StandardIds.MuscularStrengthObjectiveId, "Muscular Strength", "Build maximum strength through heavy loads and low repetitions", 1, true),
            (StandardIds.MuscularHypertrophyObjectiveId, "Muscular Hypertrophy", "Increase muscle size through moderate loads and volume", 2, true),
            (StandardIds.MuscularEnduranceObjectiveId, "Muscular Endurance", "Improve ability to sustain effort over time", 3, true),
            (StandardIds.PowerDevelopmentObjectiveId, "Power Development", "Develop explosive strength and speed", 4, true),
            (Guid.Parse("55555555-5555-5555-5555-555555555555"), "Inactive Objective", "This objective is no longer used", 5, false)
        };

        foreach (var (id, value, description, displayOrder, isActive) in workoutObjectivesToCheck)
        {
            var workoutObjectiveId = WorkoutObjectiveId.From(id);
            var exists = await _context.WorkoutObjectives.AnyAsync(wo => wo.Id == workoutObjectiveId);
            
            if (!exists)
            {
                var workoutObjective = WorkoutObjective.Handler.Create(workoutObjectiveId, value, description, displayOrder, isActive);
                await _context.WorkoutObjectives.AddAsync(workoutObjective);
            }
        }
        
        await _context.SaveChangesAsync();
        return this;
    }
    
    /// <summary>
    /// Ensures WorkoutCategories exist in the database. Only inserts if they don't already exist.
    /// </summary>
    public async Task<SeedDataBuilder> WithWorkoutCategoriesAsync()
    {
        var workoutCategoriesToCheck = new[]
        {
            (StandardIds.UpperBodyPushCategoryId, "Upper Body - Push", "Push exercises targeting chest, shoulders, and triceps", "💪", "#FF5722", "Chest,Shoulders,Triceps", 1, true),
            (StandardIds.UpperBodyPullCategoryId, "Upper Body - Pull", "Pull exercises targeting back and biceps", "🏋️", "#4CAF50", "Back,Biceps", 2, true),
            (StandardIds.LowerBodyCategoryId, "Lower Body", "Lower body exercises for legs and glutes", "🦵", "#2196F3", "Quadriceps,Hamstrings,Glutes,Calves", 3, true),
            (StandardIds.CoreCategoryId, "Core", "Core stability and strength exercises", "🎯", "#9C27B0", "Abs,Obliques,Lower Back", 4, true),
            (StandardIds.FullBodyCategoryId, "Full Body", "Compound exercises engaging multiple muscle groups", "🏃", "#FF9800", "Multiple", 5, true),
            (Guid.Parse("55555555-5555-5555-5555-555555555555"), "Inactive Category", "This category is no longer used", "❌", "#757575", "None", 6, false)
        };

        foreach (var (id, value, description, icon, color, primaryMuscleGroups, displayOrder, isActive) in workoutCategoriesToCheck)
        {
            var workoutCategoryId = WorkoutCategoryId.From(id);
            var exists = await _context.WorkoutCategories.AnyAsync(wc => wc.Id == workoutCategoryId);
            
            if (!exists)
            {
                var workoutCategory = WorkoutCategory.Handler.Create(workoutCategoryId, value, description, icon, color, primaryMuscleGroups, displayOrder, isActive);
                await _context.WorkoutCategories.AddAsync(workoutCategory);
            }
        }
        
        await _context.SaveChangesAsync();
        return this;
    }
    
    /// <summary>
    /// Ensures ExecutionProtocols exist in the database. Only inserts if they don't already exist.
    /// </summary>
    public async Task<SeedDataBuilder> WithExecutionProtocolsAsync()
    {
        var executionProtocolsToCheck = new[]
        {
            (StandardIds.StandardProtocolId, "Standard", "Standard protocol with balanced rep and time components", "STANDARD", true, true, "60-90 seconds between sets", "Moderate to High", 1, true),
            (StandardIds.SupersetProtocolId, "Superset", "Perform exercises back-to-back without rest", "SUPERSET", false, true, "Rest after completing both exercises", "High", 2, true),
            (StandardIds.DropSetProtocolId, "Drop Set", "Reduce weight after reaching failure", "DROP_SET", false, true, "Minimal rest between drops", "Very High", 3, true),
            (StandardIds.AMRAPProtocolId, "AMRAP", "As Many Reps As Possible in given time", "AMRAP", true, false, "Fixed rest periods", "High", 4, true),
            (Guid.Parse("55555555-5555-5555-5555-555555555555"), "Inactive", "This protocol is no longer used", "INACTIVE", false, false, "N/A", "N/A", 5, false)
        };

        foreach (var (id, value, description, code, timeBase, repBase, restPattern, intensityLevel, displayOrder, isActive) in executionProtocolsToCheck)
        {
            var executionProtocolId = ExecutionProtocolId.From(id);
            var exists = await _context.ExecutionProtocols.AnyAsync(ep => ep.ExecutionProtocolId == executionProtocolId);
            
            if (!exists)
            {
                var executionProtocolResult = ExecutionProtocol.Handler.Create(executionProtocolId, value, description, code, timeBase, repBase, restPattern, intensityLevel, displayOrder, isActive);
                if (executionProtocolResult.IsSuccess)
                {
                    await _context.ExecutionProtocols.AddAsync(executionProtocolResult.Value);
                }
            }
        }
        
        await _context.SaveChangesAsync();
        return this;
    }
    
    /// <summary>
    /// Ensures all reference data exists in the database.
    /// Calls all WithXXXAsync() methods in the correct dependency order.
    /// </summary>
    public async Task<SeedDataBuilder> WithAllReferenceDataAsync()
    {
        // Step 1: Tables with no dependencies
        await WithBodyPartsAsync();
        await WithDifficultyLevelsAsync();
        await WithKineticChainTypesAsync();
        await WithMuscleRolesAsync();
        await WithEquipmentAsync();
        await WithMetricTypesAsync();
        await WithMovementPatternsAsync();
        await WithExerciseTypesAsync();
        await WithExerciseWeightTypesAsync();
        await WithWorkoutObjectivesAsync();
        await WithWorkoutCategoriesAsync();
        await WithExecutionProtocolsAsync();
        
        // Step 2: Tables that depend on others (MuscleGroups depends on BodyParts)
        await WithMuscleGroupsAsync();
        
        return this;
    }
    
    /// <summary>
    /// Adds a workout exercise to the internal collection.
    /// Call CommitAsync() to save to database.
    /// </summary>
    public SeedDataBuilder WithWorkoutExercise(
        string? name = null,
        string? description = null,
        string? videoUrl = null,
        string? imageUrl = null,
        bool isUnilateral = false,
        Guid? difficultyId = null,
        Guid? kineticChainId = null,
        Guid? exerciseWeightTypeId = null,
        Action<Exercise>? configure = null)
    {
        var exercise = Exercise.Handler.CreateNew(
            name: name ?? $"Barbell Back Squat {Guid.NewGuid()}",
            description: description ?? "A compound lower body exercise",
            videoUrl: videoUrl ?? "https://example.com/squat-video.mp4",
            imageUrl: imageUrl ?? "https://example.com/squat-image.jpg",
            isUnilateral: isUnilateral,
            difficultyId: DifficultyLevelId.From(difficultyId ?? StandardIds.IntermediateDifficultyId),
            kineticChainId: KineticChainTypeId.From(kineticChainId ?? StandardIds.CompoundKineticChainId),
            exerciseWeightTypeId: ExerciseWeightTypeId.From(exerciseWeightTypeId ?? StandardIds.WeightRequiredWeightTypeId)
        );
        
        // Add default relationships
        exercise.ExerciseExerciseTypes.Add(ExerciseExerciseType.Handler.Create(
            exercise.Id,
            ExerciseTypeId.From(StandardIds.WorkoutExerciseTypeId)
        ));
        
        exercise.ExerciseMuscleGroups.Add(ExerciseMuscleGroup.Handler.Create(
            exercise.Id,
            MuscleGroupId.From(StandardIds.QuadricepsMuscleGroupId),
            MuscleRoleId.From(StandardIds.PrimaryMuscleRoleId)
        ));
        
        exercise.ExerciseMuscleGroups.Add(ExerciseMuscleGroup.Handler.Create(
            exercise.Id,
            MuscleGroupId.From(StandardIds.GlutesMuscleGroupId),
            MuscleRoleId.From(StandardIds.SecondaryMuscleRoleId)
        ));
        
        exercise.ExerciseEquipment.Add(ExerciseEquipment.Handler.Create(
            exercise.Id,
            EquipmentId.From(StandardIds.BarbellEquipmentId)
        ));
        
        exercise.ExerciseMovementPatterns.Add(ExerciseMovementPattern.Handler.Create(
            exercise.Id,
            MovementPatternId.From(StandardIds.SquatMovementPatternId)
        ));
        
        // Allow custom configuration
        configure?.Invoke(exercise);
        
        _exercises.Add(exercise);
        return this;
    }
    
    /// <summary>
    /// Adds a rest exercise to the internal collection.
    /// Call CommitAsync() to save to database.
    /// </summary>
    public SeedDataBuilder WithRestExercise(
        string? name = null,
        Action<Exercise>? configure = null)
    {
        var exercise = Exercise.Handler.CreateNew(
            name: name ?? $"Rest Period {Guid.NewGuid()}",
            description: "Rest between sets",
            videoUrl: null,
            imageUrl: null,
            isUnilateral: false,
            difficultyId: DifficultyLevelId.From(StandardIds.BeginnerDifficultyId),
            kineticChainId: null,
            exerciseWeightTypeId: null
        );
        
        // Add REST type
        exercise.ExerciseExerciseTypes.Add(ExerciseExerciseType.Handler.Create(
            exercise.Id,
            ExerciseTypeId.From(StandardIds.RestExerciseTypeId)
        ));
        
        // REST exercises don't need muscle groups, equipment, or movement patterns
        
        // Allow custom configuration
        configure?.Invoke(exercise);
        
        _exercises.Add(exercise);
        return this;
    }
    
    /// <summary>
    /// Adds a warmup exercise to the internal collection.
    /// Call CommitAsync() to save to database.
    /// </summary>
    public SeedDataBuilder WithWarmupExercise(
        string? name = null,
        Action<Exercise>? configure = null)
    {
        var exercise = Exercise.Handler.CreateNew(
            name: name ?? $"Dynamic Stretching {Guid.NewGuid()}",
            description: "Dynamic warmup movements",
            videoUrl: null,
            imageUrl: null,
            isUnilateral: false,
            difficultyId: DifficultyLevelId.From(StandardIds.BeginnerDifficultyId),
            kineticChainId: KineticChainTypeId.From(StandardIds.CompoundKineticChainId),
            exerciseWeightTypeId: ExerciseWeightTypeId.From(StandardIds.BodyweightOnlyWeightTypeId)
        );
        
        // Add warmup type
        exercise.ExerciseExerciseTypes.Add(ExerciseExerciseType.Handler.Create(
            exercise.Id,
            ExerciseTypeId.From(StandardIds.WarmupExerciseTypeId)
        ));
        
        // Add simple muscle groups for warmup
        exercise.ExerciseMuscleGroups.Add(ExerciseMuscleGroup.Handler.Create(
            exercise.Id,
            MuscleGroupId.From(StandardIds.QuadricepsMuscleGroupId),
            MuscleRoleId.From(StandardIds.PrimaryMuscleRoleId)
        ));
        
        // Allow custom configuration
        configure?.Invoke(exercise);
        
        _exercises.Add(exercise);
        return this;
    }
    
    /// <summary>
    /// Adds a cooldown exercise to the internal collection.
    /// Call CommitAsync() to save to database.
    /// </summary>
    public SeedDataBuilder WithCooldownExercise(
        string? name = null,
        Action<Exercise>? configure = null)
    {
        var exercise = Exercise.Handler.CreateNew(
            name: name ?? $"Static Stretching {Guid.NewGuid()}",
            description: "Static cooldown stretches",
            videoUrl: null,
            imageUrl: null,
            isUnilateral: false,
            difficultyId: DifficultyLevelId.From(StandardIds.BeginnerDifficultyId),
            kineticChainId: KineticChainTypeId.From(StandardIds.IsolationKineticChainId),
            exerciseWeightTypeId: ExerciseWeightTypeId.From(StandardIds.BodyweightOnlyWeightTypeId)
        );
        
        // Add cooldown type
        exercise.ExerciseExerciseTypes.Add(ExerciseExerciseType.Handler.Create(
            exercise.Id,
            ExerciseTypeId.From(StandardIds.CooldownExerciseTypeId)
        ));
        
        // Add simple muscle groups for cooldown
        exercise.ExerciseMuscleGroups.Add(ExerciseMuscleGroup.Handler.Create(
            exercise.Id,
            MuscleGroupId.From(StandardIds.HamstringsMuscleGroupId),
            MuscleRoleId.From(StandardIds.PrimaryMuscleRoleId)
        ));
        
        // Allow custom configuration
        configure?.Invoke(exercise);
        
        _exercises.Add(exercise);
        return this;
    }
    
    /// <summary>
    /// Commits all pending exercises to the database.
    /// This is the only method that saves exercises - no BuildAsync() needed.
    /// </summary>
    public async Task CommitAsync()
    {
        if (_exercises.Any())
        {
            await _context.Exercises.AddRangeAsync(_exercises);
            await _context.SaveChangesAsync();
            _exercises.Clear(); // Clear after committing
        }
    }
    
    /// <summary>
    /// Clear all data from the database (for test cleanup)
    /// </summary>
    public async Task ClearAllDataAsync()
    {
        // Delete in reverse order of dependencies
        _context.ExerciseLinks.RemoveRange(_context.ExerciseLinks);
        _context.Exercises.RemoveRange(_context.Exercises);
        _context.MuscleGroups.RemoveRange(_context.MuscleGroups);
        _context.ExerciseWeightTypes.RemoveRange(_context.ExerciseWeightTypes);
        _context.ExerciseTypes.RemoveRange(_context.ExerciseTypes);
        _context.MovementPatterns.RemoveRange(_context.MovementPatterns);
        _context.MetricTypes.RemoveRange(_context.MetricTypes);
        _context.Equipment.RemoveRange(_context.Equipment);
        _context.MuscleRoles.RemoveRange(_context.MuscleRoles);
        _context.KineticChainTypes.RemoveRange(_context.KineticChainTypes);
        _context.DifficultyLevels.RemoveRange(_context.DifficultyLevels);
        _context.BodyParts.RemoveRange(_context.BodyParts);
        
        await _context.SaveChangesAsync();
    }
}