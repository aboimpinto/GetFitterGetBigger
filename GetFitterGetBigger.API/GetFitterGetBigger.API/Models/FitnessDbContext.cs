using Microsoft.EntityFrameworkCore;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using System;

namespace GetFitterGetBigger.API.Models;

public class FitnessDbContext : DbContext
{
    // Core entities
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<MovementPattern> MovementPatterns => Set<MovementPattern>();
    public DbSet<MuscleGroup> MuscleGroups => Set<MuscleGroup>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<MetricType> MetricTypes => Set<MetricType>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>();
    public DbSet<WorkoutLogSet> WorkoutLogSets => Set<WorkoutLogSet>();
    
    // Reference data entities
    public DbSet<DifficultyLevel> DifficultyLevels => Set<DifficultyLevel>();
    public DbSet<KineticChainType> KineticChainTypes => Set<KineticChainType>();
    public DbSet<BodyPart> BodyParts => Set<BodyPart>();
    public DbSet<MuscleRole> MuscleRoles => Set<MuscleRole>();
    
    // Linking entities
    public DbSet<ExerciseMovementPattern> ExerciseMovementPatterns => Set<ExerciseMovementPattern>();
    public DbSet<ExerciseTargetedMuscle> ExerciseTargetedMuscles => Set<ExerciseTargetedMuscle>();
    public DbSet<ExerciseEquipment> ExerciseEquipment => Set<ExerciseEquipment>();
    public DbSet<ExerciseMetricSupport> ExerciseMetricSupport => Set<ExerciseMetricSupport>();
    public DbSet<ExerciseMuscleGroup> ExerciseMuscleGroups => Set<ExerciseMuscleGroup>();
    public DbSet<ExerciseBodyPart> ExerciseBodyParts => Set<ExerciseBodyPart>();
    
    public FitnessDbContext(DbContextOptions<FitnessDbContext> options) 
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure specialized ID types
        ConfigureSpecializedIdTypes(modelBuilder);
        
        // Configure many-to-many relationships
        ConfigureManyToManyRelationships(modelBuilder);
        
        // Configure one-to-many relationships
        ConfigureOneToManyRelationships(modelBuilder);
        
        // Seed reference data
        SeedReferenceData(modelBuilder);
    }
    
    private static void ConfigureSpecializedIdTypes(ModelBuilder modelBuilder)
    {
        // Exercise ID
        modelBuilder.Entity<Exercise>()
            .Property(e => e.Id)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        // Movement Pattern ID
        modelBuilder.Entity<MovementPattern>()
            .Property(mp => mp.Id)
            .HasConversion(
                id => (Guid)id,
                guid => MovementPatternId.From(guid));
                
        // Muscle Group ID
        modelBuilder.Entity<MuscleGroup>()
            .Property(mg => mg.Id)
            .HasConversion(
                id => (Guid)id,
                guid => MuscleGroupId.From(guid));
                
        // Equipment ID
        modelBuilder.Entity<Equipment>()
            .Property(e => e.Id)
            .HasConversion(
                id => (Guid)id,
                guid => EquipmentId.From(guid));
                
        // Metric Type ID
        modelBuilder.Entity<MetricType>()
            .Property(mt => mt.Id)
            .HasConversion(
                id => (Guid)id,
                guid => MetricTypeId.From(guid));
                
        // User ID
        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .HasConversion(
                id => (Guid)id,
                guid => UserId.From(guid));

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
                
        // Claim ID
        modelBuilder.Entity<Claim>()
            .Property(c => c.Id)
            .HasConversion(
                id => (Guid)id,
                guid => ClaimId.From(guid));

        modelBuilder.Entity<Claim>()
            .Property(c => c.UserId)
            .HasConversion(
                id => (Guid)id,
                guid => UserId.From(guid));
                
        // Workout Log ID
        modelBuilder.Entity<WorkoutLog>()
            .Property(wl => wl.Id)
            .HasConversion(
                id => (Guid)id,
                guid => WorkoutLogId.From(guid));
                
        // Workout Log Set ID
        modelBuilder.Entity<WorkoutLogSet>()
            .Property(wls => wls.Id)
            .HasConversion(
                id => (Guid)id,
                guid => WorkoutLogSetId.From(guid));
                
        // Reference data IDs
        modelBuilder.Entity<DifficultyLevel>()
            .Property(dl => dl.Id)
            .HasConversion(
                id => (Guid)id,
                guid => DifficultyLevelId.From(guid));
                
        modelBuilder.Entity<KineticChainType>()
            .Property(kct => kct.Id)
            .HasConversion(
                id => (Guid)id,
                guid => KineticChainTypeId.From(guid));
                
        modelBuilder.Entity<BodyPart>()
            .Property(bp => bp.Id)
            .HasConversion(
                id => (Guid)id,
                guid => BodyPartId.From(guid));
                
        modelBuilder.Entity<MuscleRole>()
            .Property(mr => mr.Id)
            .HasConversion(
                id => (Guid)id,
                guid => MuscleRoleId.From(guid));
                
        // Foreign key ID conversions
        modelBuilder.Entity<WorkoutLog>()
            .Property(wl => wl.UserId)
            .HasConversion(
                id => (Guid)id,
                guid => UserId.From(guid));
                
        modelBuilder.Entity<WorkoutLogSet>()
            .Property(wls => wls.LogId)
            .HasConversion(
                id => (Guid)id,
                guid => WorkoutLogId.From(guid));
                
        modelBuilder.Entity<WorkoutLogSet>()
            .Property(wls => wls.ExerciseId)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        modelBuilder.Entity<Exercise>()
            .Property(e => e.DifficultyId)
            .HasConversion(
                id => (Guid)id,
                guid => DifficultyLevelId.From(guid));
                
        // Configure Exercise entity constraints
        modelBuilder.Entity<Exercise>()
            .HasIndex(e => e.Name)
            .IsUnique();
            
        modelBuilder.Entity<Exercise>()
            .Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();
            
        modelBuilder.Entity<Exercise>()
            .Property(e => e.Description)
            .HasMaxLength(1000)
            .IsRequired();
            
        modelBuilder.Entity<Exercise>()
            .Property(e => e.Instructions)
            .HasMaxLength(5000)
            .IsRequired();
                
        modelBuilder.Entity<MuscleGroup>()
            .Property(mg => mg.BodyPartId)
            .HasConversion(
                id => (Guid)id,
                guid => BodyPartId.From(guid));
                
        modelBuilder.Entity<ExerciseTargetedMuscle>()
            .Property(etm => etm.MuscleRoleId)
            .HasConversion(
                id => (Guid)id,
                guid => MuscleRoleId.From(guid));
    }
    
    private static void ConfigureManyToManyRelationships(ModelBuilder modelBuilder)
    {
        // Exercise to Movement Pattern (many-to-many)
        modelBuilder.Entity<ExerciseMovementPattern>()
            .HasKey(emp => new { emp.ExerciseId, emp.MovementPatternId });
            
        modelBuilder.Entity<ExerciseMovementPattern>()
            .Property(emp => emp.ExerciseId)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        modelBuilder.Entity<ExerciseMovementPattern>()
            .Property(emp => emp.MovementPatternId)
            .HasConversion(
                id => (Guid)id,
                guid => MovementPatternId.From(guid));
                
        modelBuilder.Entity<ExerciseMovementPattern>()
            .HasOne(emp => emp.Exercise)
            .WithMany(e => e.ExerciseMovementPatterns)
            .HasForeignKey(emp => emp.ExerciseId);
            
        modelBuilder.Entity<ExerciseMovementPattern>()
            .HasOne(emp => emp.MovementPattern)
            .WithMany(mp => mp.Exercises)
            .HasForeignKey(emp => emp.MovementPatternId);
            
        // Exercise to Muscle Group (many-to-many with additional data)
        modelBuilder.Entity<ExerciseTargetedMuscle>()
            .HasKey(etm => new { etm.ExerciseId, etm.MuscleGroupId });
            
        modelBuilder.Entity<ExerciseTargetedMuscle>()
            .Property(etm => etm.ExerciseId)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        modelBuilder.Entity<ExerciseTargetedMuscle>()
            .Property(etm => etm.MuscleGroupId)
            .HasConversion(
                id => (Guid)id,
                guid => MuscleGroupId.From(guid));
                
        modelBuilder.Entity<ExerciseTargetedMuscle>()
            .HasOne(etm => etm.Exercise)
            .WithMany()
            .HasForeignKey(etm => etm.ExerciseId);
            
        modelBuilder.Entity<ExerciseTargetedMuscle>()
            .HasOne(etm => etm.MuscleGroup)
            .WithMany(mg => mg.Exercises)
            .HasForeignKey(etm => etm.MuscleGroupId);
            
        modelBuilder.Entity<ExerciseTargetedMuscle>()
            .HasOne(etm => etm.MuscleRole)
            .WithMany()
            .HasForeignKey(etm => etm.MuscleRoleId);
            
        // Exercise to Equipment (many-to-many)
        modelBuilder.Entity<ExerciseEquipment>()
            .HasKey(ee => new { ee.ExerciseId, ee.EquipmentId });
            
        modelBuilder.Entity<ExerciseEquipment>()
            .Property(ee => ee.ExerciseId)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        modelBuilder.Entity<ExerciseEquipment>()
            .Property(ee => ee.EquipmentId)
            .HasConversion(
                id => (Guid)id,
                guid => EquipmentId.From(guid));
                
        modelBuilder.Entity<ExerciseEquipment>()
            .HasOne(ee => ee.Exercise)
            .WithMany(e => e.ExerciseEquipment)
            .HasForeignKey(ee => ee.ExerciseId);
            
        modelBuilder.Entity<ExerciseEquipment>()
            .HasOne(ee => ee.Equipment)
            .WithMany(e => e.Exercises)
            .HasForeignKey(ee => ee.EquipmentId);
            
        // Exercise to Metric Type (many-to-many)
        modelBuilder.Entity<ExerciseMetricSupport>()
            .HasKey(ems => new { ems.ExerciseId, ems.MetricTypeId });
            
        modelBuilder.Entity<ExerciseMetricSupport>()
            .Property(ems => ems.ExerciseId)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        modelBuilder.Entity<ExerciseMetricSupport>()
            .Property(ems => ems.MetricTypeId)
            .HasConversion(
                id => (Guid)id,
                guid => MetricTypeId.From(guid));
                
        modelBuilder.Entity<ExerciseMetricSupport>()
            .HasOne(ems => ems.Exercise)
            .WithMany()
            .HasForeignKey(ems => ems.ExerciseId);
            
        modelBuilder.Entity<ExerciseMetricSupport>()
            .HasOne(ems => ems.MetricType)
            .WithMany(mt => mt.Exercises)
            .HasForeignKey(ems => ems.MetricTypeId);
            
        // Exercise to Muscle Group (many-to-many) - NEW
        modelBuilder.Entity<ExerciseMuscleGroup>()
            .HasKey(emg => new { emg.ExerciseId, emg.MuscleGroupId });
            
        modelBuilder.Entity<ExerciseMuscleGroup>()
            .Property(emg => emg.ExerciseId)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        modelBuilder.Entity<ExerciseMuscleGroup>()
            .Property(emg => emg.MuscleGroupId)
            .HasConversion(
                id => (Guid)id,
                guid => MuscleGroupId.From(guid));
                
        modelBuilder.Entity<ExerciseMuscleGroup>()
            .Property(emg => emg.MuscleRoleId)
            .HasConversion(
                id => (Guid)id,
                guid => MuscleRoleId.From(guid));
                
        modelBuilder.Entity<ExerciseMuscleGroup>()
            .HasOne(emg => emg.Exercise)
            .WithMany(e => e.ExerciseMuscleGroups)
            .HasForeignKey(emg => emg.ExerciseId);
            
        modelBuilder.Entity<ExerciseMuscleGroup>()
            .HasOne(emg => emg.MuscleGroup)
            .WithMany()
            .HasForeignKey(emg => emg.MuscleGroupId);
            
        modelBuilder.Entity<ExerciseMuscleGroup>()
            .HasOne(emg => emg.MuscleRole)
            .WithMany()
            .HasForeignKey(emg => emg.MuscleRoleId);
            
        // Exercise to BodyPart (many-to-many) - NEW
        modelBuilder.Entity<ExerciseBodyPart>()
            .HasKey(ebp => new { ebp.ExerciseId, ebp.BodyPartId });
            
        modelBuilder.Entity<ExerciseBodyPart>()
            .Property(ebp => ebp.ExerciseId)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        modelBuilder.Entity<ExerciseBodyPart>()
            .Property(ebp => ebp.BodyPartId)
            .HasConversion(
                id => (Guid)id,
                guid => BodyPartId.From(guid));
                
        modelBuilder.Entity<ExerciseBodyPart>()
            .HasOne(ebp => ebp.Exercise)
            .WithMany(e => e.ExerciseBodyParts)
            .HasForeignKey(ebp => ebp.ExerciseId);
            
        modelBuilder.Entity<ExerciseBodyPart>()
            .HasOne(ebp => ebp.BodyPart)
            .WithMany()
            .HasForeignKey(ebp => ebp.BodyPartId);
    }
    
    private static void ConfigureOneToManyRelationships(ModelBuilder modelBuilder)
    {
        // User to Claim (one-to-many)
        modelBuilder.Entity<Claim>()
            .HasOne(c => c.User)
            .WithMany(u => u.Claims)
            .HasForeignKey(c => c.UserId);

        // User to WorkoutLog (one-to-many)
        modelBuilder.Entity<WorkoutLog>()
            .HasOne(wl => wl.User)
            .WithMany(u => u.WorkoutLogs)
            .HasForeignKey(wl => wl.UserId);
            
        // WorkoutLog to WorkoutLogSet (one-to-many)
        modelBuilder.Entity<WorkoutLogSet>()
            .HasOne(wls => wls.WorkoutLog)
            .WithMany(wl => wl.Sets)
            .HasForeignKey(wls => wls.LogId);
            
        // Exercise to WorkoutLogSet (one-to-many)
        modelBuilder.Entity<WorkoutLogSet>()
            .HasOne(wls => wls.Exercise)
            .WithMany()
            .HasForeignKey(wls => wls.ExerciseId);
            
        // DifficultyLevel to Exercise (one-to-many)
        modelBuilder.Entity<Exercise>()
            .HasOne(e => e.Difficulty)
            .WithMany(dl => dl.Exercises)
            .HasForeignKey(e => e.DifficultyId);
            
        // BodyPart to MuscleGroup (one-to-many)
        modelBuilder.Entity<MuscleGroup>()
            .HasOne(mg => mg.BodyPart)
            .WithMany()
            .HasForeignKey(mg => mg.BodyPartId);
            
        // Configure decimal precision for WorkoutLogSet.WeightUsedKg
        modelBuilder.Entity<WorkoutLogSet>()
            .Property(wls => wls.WeightUsedKg)
            .HasPrecision(10, 2);
    }
    
    private static void SeedReferenceData(ModelBuilder modelBuilder)
    {
        // Seed DifficultyLevels
        modelBuilder.Entity<DifficultyLevel>().HasData(
            DifficultyLevel.Handler.Create(
                DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b")),
                "Beginner",
                "Suitable for those new to fitness",
                1,
                true),
            DifficultyLevel.Handler.Create(
                DifficultyLevelId.From(Guid.Parse("9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a")),
                "Intermediate",
                "Suitable for those with some fitness experience",
                2,
                true),
            DifficultyLevel.Handler.Create(
                DifficultyLevelId.From(Guid.Parse("3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c")),
                "Advanced",
                "Suitable for those with significant fitness experience",
                3,
                true)
        );
        
        // Seed KineticChainTypes
        modelBuilder.Entity<KineticChainType>().HasData(
            KineticChainType.Handler.Create(
                KineticChainTypeId.From(Guid.Parse("f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4")),
                "Compound",
                "Exercises that work multiple muscle groups",
                1,
                true),
            KineticChainType.Handler.Create(
                KineticChainTypeId.From(Guid.Parse("2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b")),
                "Isolation",
                "Exercises that work a single muscle group",
                2,
                true)
        );
        
        // Seed BodyParts
        modelBuilder.Entity<BodyPart>().HasData(
            BodyPart.Handler.Create(
                BodyPartId.From(Guid.Parse("7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c")),
                "Chest",
                null,
                1,
                true),
            BodyPart.Handler.Create(
                BodyPartId.From(Guid.Parse("b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a")),
                "Back",
                null,
                2,
                true),
            BodyPart.Handler.Create(
                BodyPartId.From(Guid.Parse("4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5")),
                "Legs",
                null,
                3,
                true),
            BodyPart.Handler.Create(
                BodyPartId.From(Guid.Parse("d7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a")),
                "Shoulders",
                null,
                4,
                true),
            BodyPart.Handler.Create(
                BodyPartId.From(Guid.Parse("9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1")),
                "Arms",
                null,
                5,
                true),
            BodyPart.Handler.Create(
                BodyPartId.From(Guid.Parse("3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c")),
                "Core",
                null,
                6,
                true)
        );
        
        // Seed MuscleRoles
        modelBuilder.Entity<MuscleRole>().HasData(
            MuscleRole.Handler.Create(
                MuscleRoleId.From(Guid.Parse("5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b")),
                "Primary",
                "The main muscle targeted by the exercise",
                1,
                true),
            MuscleRole.Handler.Create(
                MuscleRoleId.From(Guid.Parse("8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a")),
                "Secondary",
                "A muscle that assists in the exercise",
                2,
                true),
            MuscleRole.Handler.Create(
                MuscleRoleId.From(Guid.Parse("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d")),
                "Stabilizer",
                "A muscle that helps stabilize the body during the exercise",
                3,
                true)
        );
    }
}
