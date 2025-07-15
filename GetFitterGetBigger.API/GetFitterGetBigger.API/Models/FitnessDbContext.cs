using Microsoft.EntityFrameworkCore;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using System;

namespace GetFitterGetBigger.API.Models;

public class FitnessDbContext : DbContext
{
    // Core entities
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<CoachNote> CoachNotes => Set<CoachNote>();
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
    public DbSet<ExerciseType> ExerciseTypes => Set<ExerciseType>();
    public DbSet<ExerciseWeightType> ExerciseWeightTypes => Set<ExerciseWeightType>();
    public DbSet<WorkoutObjective> WorkoutObjectives => Set<WorkoutObjective>();
    public DbSet<WorkoutCategory> WorkoutCategories => Set<WorkoutCategory>();
    public DbSet<ExecutionProtocol> ExecutionProtocols => Set<ExecutionProtocol>();
    public DbSet<WorkoutMuscles> WorkoutMuscles => Set<WorkoutMuscles>();
    
    // Linking entities
    public DbSet<ExerciseMovementPattern> ExerciseMovementPatterns => Set<ExerciseMovementPattern>();
    public DbSet<ExerciseTargetedMuscle> ExerciseTargetedMuscles => Set<ExerciseTargetedMuscle>();
    public DbSet<ExerciseEquipment> ExerciseEquipment => Set<ExerciseEquipment>();
    public DbSet<ExerciseMetricSupport> ExerciseMetricSupport => Set<ExerciseMetricSupport>();
    public DbSet<ExerciseMuscleGroup> ExerciseMuscleGroups => Set<ExerciseMuscleGroup>();
    public DbSet<ExerciseBodyPart> ExerciseBodyParts => Set<ExerciseBodyPart>();
    public DbSet<ExerciseExerciseType> ExerciseExerciseTypes => Set<ExerciseExerciseType>();
    public DbSet<ExerciseLink> ExerciseLinks => Set<ExerciseLink>();
    
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
                
        // CoachNote ID
        modelBuilder.Entity<CoachNote>()
            .Property(cn => cn.Id)
            .HasConversion(
                id => (Guid)id,
                guid => CoachNoteId.From(guid));
                
        modelBuilder.Entity<CoachNote>()
            .Property(cn => cn.ExerciseId)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        // Movement Pattern ID
        modelBuilder.Entity<MovementPattern>()
            .HasKey(mp => mp.MovementPatternId);
            
        modelBuilder.Entity<MovementPattern>()
            .Property(mp => mp.MovementPatternId)
            .HasColumnName("Id")
            .HasConversion(
                id => (Guid)id,
                guid => MovementPatternId.From(guid));
                
        modelBuilder.Entity<MovementPattern>()
            .Ignore(mp => mp.Id);
                
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
            .HasKey(bp => bp.BodyPartId);
            
        modelBuilder.Entity<BodyPart>()
            .Property(bp => bp.BodyPartId)
            .HasColumnName("Id")
            .HasConversion(
                id => (Guid)id,
                guid => BodyPartId.From(guid));
                
        modelBuilder.Entity<BodyPart>()
            .Ignore(bp => bp.Id);
                
        modelBuilder.Entity<MuscleRole>()
            .Property(mr => mr.Id)
            .HasConversion(
                id => (Guid)id,
                guid => MuscleRoleId.From(guid));
                
        modelBuilder.Entity<ExerciseType>()
            .Property(et => et.Id)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseTypeId.From(guid));
                
        modelBuilder.Entity<ExerciseWeightType>()
            .Property(ewt => ewt.Id)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseWeightTypeId.From(guid));
                
        modelBuilder.Entity<WorkoutObjective>()
            .Property(wo => wo.Id)
            .HasConversion(
                id => (Guid)id,
                guid => WorkoutObjectiveId.From(guid));
                
        modelBuilder.Entity<WorkoutCategory>()
            .Property(wc => wc.Id)
            .HasConversion(
                id => (Guid)id,
                guid => WorkoutCategoryId.From(guid));
                
        modelBuilder.Entity<ExecutionProtocol>()
            .Property(ep => ep.Id)
            .HasConversion(
                id => (Guid)id,
                guid => ExecutionProtocolId.From(guid));
                
        modelBuilder.Entity<WorkoutMuscles>()
            .Property(wm => wm.Id)
            .HasConversion(
                id => (Guid)id,
                guid => WorkoutMusclesId.From(guid));
                
        modelBuilder.Entity<WorkoutMuscles>()
            .Property(wm => wm.MuscleGroupId)
            .HasConversion(
                id => (Guid)id,
                guid => MuscleGroupId.From(guid));
                
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
                
        modelBuilder.Entity<Exercise>()
            .Property(e => e.KineticChainId)
            .HasConversion(
                id => id.HasValue ? (Guid?)id.Value : null,
                guid => guid.HasValue ? KineticChainTypeId.From(guid.Value) : null);
                
        modelBuilder.Entity<Exercise>()
            .Property(e => e.ExerciseWeightTypeId)
            .HasConversion(
                id => id.HasValue ? (Guid?)id.Value : null,
                guid => guid.HasValue ? ExerciseWeightTypeId.From(guid.Value) : null);
                
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
            
        // Configure CoachNote entity constraints
        modelBuilder.Entity<CoachNote>()
            .Property(cn => cn.Text)
            .HasMaxLength(1000)
            .IsRequired();
            
        modelBuilder.Entity<CoachNote>()
            .HasIndex(cn => new { cn.ExerciseId, cn.Order })
            .HasDatabaseName("IX_CoachNote_ExerciseId_Order");
                
        modelBuilder.Entity<MuscleGroup>()
            .Property(mg => mg.BodyPartId)
            .HasConversion(
                id => (Guid)id,
                guid => BodyPartId.From(guid));
                
        // Configure MuscleGroup CRUD properties
        modelBuilder.Entity<MuscleGroup>()
            .Property(mg => mg.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        modelBuilder.Entity<MuscleGroup>()
            .HasIndex(mg => new { mg.Name, mg.IsActive })
            .HasDatabaseName("IX_MuscleGroup_Name_IsActive");
                
        modelBuilder.Entity<ExerciseTargetedMuscle>()
            .Property(etm => etm.MuscleRoleId)
            .HasConversion(
                id => (Guid)id,
                guid => MuscleRoleId.From(guid));
                
        // ExerciseLink ID
        modelBuilder.Entity<ExerciseLink>()
            .Property(el => el.Id)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseLinkId.From(guid));
                
        modelBuilder.Entity<ExerciseLink>()
            .Property(el => el.SourceExerciseId)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        modelBuilder.Entity<ExerciseLink>()
            .Property(el => el.TargetExerciseId)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        // Configure ExerciseLink constraints and indexes
        modelBuilder.Entity<ExerciseLink>()
            .HasIndex(el => new { el.SourceExerciseId, el.TargetExerciseId, el.LinkType })
            .IsUnique()
            .HasDatabaseName("IX_ExerciseLink_Source_Target_Type_Unique");
            
        modelBuilder.Entity<ExerciseLink>()
            .HasIndex(el => new { el.SourceExerciseId, el.LinkType })
            .HasDatabaseName("IX_ExerciseLink_SourceExerciseId_LinkType");
            
        modelBuilder.Entity<ExerciseLink>()
            .HasIndex(el => el.TargetExerciseId)
            .HasDatabaseName("IX_ExerciseLink_TargetExerciseId");
            
        modelBuilder.Entity<ExerciseLink>()
            .Property(el => el.LinkType)
            .HasMaxLength(10)
            .IsRequired();
            
        // Configure relationships
        modelBuilder.Entity<ExerciseLink>()
            .HasOne(el => el.SourceExercise)
            .WithMany()
            .HasForeignKey(el => el.SourceExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<ExerciseLink>()
            .HasOne(el => el.TargetExercise)
            .WithMany()
            .HasForeignKey(el => el.TargetExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Configure ExerciseWeightType properties and indexes
        modelBuilder.Entity<ExerciseWeightType>()
            .Property(ewt => ewt.Code)
            .HasMaxLength(50)
            .IsRequired();
            
        modelBuilder.Entity<ExerciseWeightType>()
            .Property(ewt => ewt.Value)
            .HasMaxLength(100)
            .IsRequired();
            
        modelBuilder.Entity<ExerciseWeightType>()
            .HasIndex(ewt => ewt.Code)
            .IsUnique()
            .HasDatabaseName("IX_ExerciseWeightType_Code");
            
        modelBuilder.Entity<ExerciseWeightType>()
            .HasIndex(ewt => ewt.Value)
            .HasDatabaseName("IX_ExerciseWeightType_Value");
            
        // Configure WorkoutObjective properties and indexes
        modelBuilder.Entity<WorkoutObjective>()
            .Property(wo => wo.Value)
            .HasMaxLength(100)
            .IsRequired();
            
        modelBuilder.Entity<WorkoutObjective>()
            .Property(wo => wo.Description)
            .HasMaxLength(500);
            
        modelBuilder.Entity<WorkoutObjective>()
            .HasIndex(wo => wo.Value)
            .HasDatabaseName("IX_WorkoutObjective_Value");
            
        // Configure WorkoutCategory properties and indexes
        modelBuilder.Entity<WorkoutCategory>()
            .Property(wc => wc.Value)
            .HasMaxLength(100)
            .IsRequired();
            
        modelBuilder.Entity<WorkoutCategory>()
            .Property(wc => wc.Description)
            .HasMaxLength(500);
            
        modelBuilder.Entity<WorkoutCategory>()
            .Property(wc => wc.Icon)
            .HasMaxLength(50)
            .IsRequired();
            
        modelBuilder.Entity<WorkoutCategory>()
            .Property(wc => wc.Color)
            .HasMaxLength(10)
            .IsRequired();
            
        modelBuilder.Entity<WorkoutCategory>()
            .Property(wc => wc.PrimaryMuscleGroups)
            .HasMaxLength(200);
            
        modelBuilder.Entity<WorkoutCategory>()
            .HasIndex(wc => wc.Value)
            .HasDatabaseName("IX_WorkoutCategory_Value");
            
        // Configure ExecutionProtocol properties and indexes
        modelBuilder.Entity<ExecutionProtocol>()
            .Property(ep => ep.Value)
            .HasMaxLength(100)
            .IsRequired();
            
        modelBuilder.Entity<ExecutionProtocol>()
            .Property(ep => ep.Description)
            .HasMaxLength(500);
            
        modelBuilder.Entity<ExecutionProtocol>()
            .Property(ep => ep.Code)
            .HasMaxLength(50)
            .IsRequired();
            
        modelBuilder.Entity<ExecutionProtocol>()
            .Property(ep => ep.RestPattern)
            .HasMaxLength(100);
            
        modelBuilder.Entity<ExecutionProtocol>()
            .Property(ep => ep.IntensityLevel)
            .HasMaxLength(50);
            
        modelBuilder.Entity<ExecutionProtocol>()
            .HasIndex(ep => ep.Code)
            .IsUnique()
            .HasDatabaseName("IX_ExecutionProtocol_Code");
            
        modelBuilder.Entity<ExecutionProtocol>()
            .HasIndex(ep => ep.Value)
            .HasDatabaseName("IX_ExecutionProtocol_Value");
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
            
        // Exercise to ExerciseType (many-to-many)
        modelBuilder.Entity<ExerciseExerciseType>()
            .HasKey(eet => new { eet.ExerciseId, eet.ExerciseTypeId });
            
        modelBuilder.Entity<ExerciseExerciseType>()
            .Property(eet => eet.ExerciseId)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        modelBuilder.Entity<ExerciseExerciseType>()
            .Property(eet => eet.ExerciseTypeId)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseTypeId.From(guid));
                
        modelBuilder.Entity<ExerciseExerciseType>()
            .HasOne(eet => eet.Exercise)
            .WithMany(e => e.ExerciseExerciseTypes)
            .HasForeignKey(eet => eet.ExerciseId);
            
        modelBuilder.Entity<ExerciseExerciseType>()
            .HasOne(eet => eet.ExerciseType)
            .WithMany()
            .HasForeignKey(eet => eet.ExerciseTypeId);
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
            
        // KineticChainType to Exercise (one-to-many)
        modelBuilder.Entity<Exercise>()
            .HasOne(e => e.KineticChain)
            .WithMany(kc => kc.Exercises)
            .HasForeignKey(e => e.KineticChainId)
            .IsRequired(false);
            
        // ExerciseWeightType to Exercise (one-to-many)
        modelBuilder.Entity<Exercise>()
            .HasOne(e => e.ExerciseWeightType)
            .WithMany(ewt => ewt.Exercises)
            .HasForeignKey(e => e.ExerciseWeightTypeId)
            .IsRequired(false);
            
        // Exercise to CoachNote (one-to-many)
        modelBuilder.Entity<CoachNote>()
            .HasOne(cn => cn.Exercise)
            .WithMany(e => e.CoachNotes)
            .HasForeignKey(cn => cn.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Add composite index for CoachNote
        modelBuilder.Entity<CoachNote>()
            .HasIndex(cn => new { cn.ExerciseId, cn.Order });
            
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
        
        // Seed ExerciseTypes
        modelBuilder.Entity<ExerciseType>().HasData(
            ExerciseType.Handler.Create(
                ExerciseTypeId.From(Guid.Parse("a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d")),
                "Warmup",
                "Exercises performed to prepare the body for more intense activity",
                1,
                true),
            ExerciseType.Handler.Create(
                ExerciseTypeId.From(Guid.Parse("b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e")),
                "Workout",
                "Main exercises that form the core of the training session",
                2,
                true),
            ExerciseType.Handler.Create(
                ExerciseTypeId.From(Guid.Parse("c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f")),
                "Cooldown",
                "Exercises performed to help the body recover after intense activity",
                3,
                true),
            ExerciseType.Handler.Create(
                ExerciseTypeId.From(Guid.Parse("d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a")),
                "Rest",
                "Periods of rest between exercises or sets",
                4,
                true)
        );
        
        // Seed ExerciseWeightTypes
        modelBuilder.Entity<ExerciseWeightType>().HasData(
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a")),
                "BODYWEIGHT_ONLY",
                "Bodyweight Only",
                "Exercises that cannot have external weight added",
                1,
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f")),
                "BODYWEIGHT_OPTIONAL",
                "Bodyweight Optional",
                "Exercises that can be performed with or without additional weight",
                2,
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a")),
                "WEIGHT_REQUIRED",
                "Weight Required",
                "Exercises that must have external weight specified",
                3,
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b")),
                "MACHINE_WEIGHT",
                "Machine Weight",
                "Exercises performed on machines with weight stacks",
                4,
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c")),
                "NO_WEIGHT",
                "No Weight",
                "Exercises that do not use weight as a metric",
                5,
                true)
        );
        
        // Seed WorkoutObjectives
        modelBuilder.Entity<WorkoutObjective>().HasData(
            WorkoutObjective.Handler.Create(
                WorkoutObjectiveId.From(Guid.Parse("10000001-1000-4000-8000-100000000001")),
                "Muscular Strength",
                "Build maximum strength through heavy loads and low repetitions",
                1,
                true),
            WorkoutObjective.Handler.Create(
                WorkoutObjectiveId.From(Guid.Parse("10000001-1000-4000-8000-100000000002")),
                "Muscular Hypertrophy",
                "Increase muscle size through moderate loads and volume",
                2,
                true),
            WorkoutObjective.Handler.Create(
                WorkoutObjectiveId.From(Guid.Parse("10000001-1000-4000-8000-100000000003")),
                "Muscular Endurance",
                "Improve ability to sustain effort over time",
                3,
                true),
            WorkoutObjective.Handler.Create(
                WorkoutObjectiveId.From(Guid.Parse("10000001-1000-4000-8000-100000000004")),
                "Power Development",
                "Develop explosive strength and speed",
                4,
                true)
        );
        
        // Seed WorkoutCategories
        modelBuilder.Entity<WorkoutCategory>().HasData(
            WorkoutCategory.Handler.Create(
                WorkoutCategoryId.From(Guid.Parse("20000002-2000-4000-8000-200000000001")),
                "Upper Body - Push",
                "Push exercises targeting chest, shoulders, and triceps",
                "💪",
                "#FF5722",
                "Chest,Shoulders,Triceps",
                1,
                true),
            WorkoutCategory.Handler.Create(
                WorkoutCategoryId.From(Guid.Parse("20000002-2000-4000-8000-200000000002")),
                "Upper Body - Pull",
                "Pull exercises targeting back and biceps",
                "🏋️",
                "#4CAF50",
                "Back,Biceps",
                2,
                true),
            WorkoutCategory.Handler.Create(
                WorkoutCategoryId.From(Guid.Parse("20000002-2000-4000-8000-200000000003")),
                "Lower Body",
                "Lower body exercises for legs and glutes",
                "🦵",
                "#2196F3",
                "Quadriceps,Hamstrings,Glutes,Calves",
                3,
                true),
            WorkoutCategory.Handler.Create(
                WorkoutCategoryId.From(Guid.Parse("20000002-2000-4000-8000-200000000004")),
                "Core",
                "Core stability and strength exercises",
                "🎯",
                "#9C27B0",
                "Abs,Obliques,Lower Back",
                4,
                true),
            WorkoutCategory.Handler.Create(
                WorkoutCategoryId.From(Guid.Parse("20000002-2000-4000-8000-200000000005")),
                "Full Body",
                "Compound exercises engaging multiple muscle groups",
                "🏃",
                "#FF9800",
                "Multiple",
                5,
                true)
        );
        
        // Seed ExecutionProtocols
        modelBuilder.Entity<ExecutionProtocol>().HasData(
            ExecutionProtocol.Handler.Create(
                ExecutionProtocolId.From(Guid.Parse("30000003-3000-4000-8000-300000000001")),
                "Standard",
                "Standard protocol with balanced rep and time components",
                "STANDARD",
                true,
                true,
                "60-90 seconds between sets",
                "Moderate to High",
                1,
                true),
            ExecutionProtocol.Handler.Create(
                ExecutionProtocolId.From(Guid.Parse("30000003-3000-4000-8000-300000000002")),
                "Superset",
                "Perform exercises back-to-back without rest",
                "SUPERSET",
                false,
                true,
                "Rest after completing both exercises",
                "High",
                2,
                true),
            ExecutionProtocol.Handler.Create(
                ExecutionProtocolId.From(Guid.Parse("30000003-3000-4000-8000-300000000003")),
                "Drop Set",
                "Reduce weight after reaching failure",
                "DROP_SET",
                false,
                true,
                "Minimal rest between drops",
                "Very High",
                3,
                true),
            ExecutionProtocol.Handler.Create(
                ExecutionProtocolId.From(Guid.Parse("30000003-3000-4000-8000-300000000004")),
                "AMRAP",
                "As Many Reps As Possible in given time",
                "AMRAP",
                true,
                false,
                "Fixed rest periods",
                "High",
                4,
                true)
        );
    }
}
