using Microsoft.EntityFrameworkCore;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

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
    public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>();
    public DbSet<WorkoutLogSet> WorkoutLogSets => Set<WorkoutLogSet>();
    
    // Linking entities
    public DbSet<ExerciseMovementPattern> ExerciseMovementPatterns => Set<ExerciseMovementPattern>();
    public DbSet<ExerciseTargetedMuscle> ExerciseTargetedMuscles => Set<ExerciseTargetedMuscle>();
    public DbSet<ExerciseEquipment> ExerciseEquipment => Set<ExerciseEquipment>();
    public DbSet<ExerciseMetricSupport> ExerciseMetricSupport => Set<ExerciseMetricSupport>();
    
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
    }
    
    private static void ConfigureManyToManyRelationships(ModelBuilder modelBuilder)
    {
        // Exercise to Movement Pattern (many-to-many)
        modelBuilder.Entity<ExerciseMovementPattern>()
            .HasKey(emp => new { emp.ExerciseId, emp.PatternId });
            
        modelBuilder.Entity<ExerciseMovementPattern>()
            .Property(emp => emp.ExerciseId)
            .HasConversion(
                id => (Guid)id,
                guid => ExerciseId.From(guid));
                
        modelBuilder.Entity<ExerciseMovementPattern>()
            .Property(emp => emp.PatternId)
            .HasConversion(
                id => (Guid)id,
                guid => MovementPatternId.From(guid));
                
        modelBuilder.Entity<ExerciseMovementPattern>()
            .HasOne(emp => emp.Exercise)
            .WithMany(e => e.MovementPatterns)
            .HasForeignKey(emp => emp.ExerciseId);
            
        modelBuilder.Entity<ExerciseMovementPattern>()
            .HasOne(emp => emp.Pattern)
            .WithMany(mp => mp.Exercises)
            .HasForeignKey(emp => emp.PatternId);
            
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
            .WithMany(e => e.TargetedMuscles)
            .HasForeignKey(etm => etm.ExerciseId);
            
        modelBuilder.Entity<ExerciseTargetedMuscle>()
            .HasOne(etm => etm.MuscleGroup)
            .WithMany(mg => mg.Exercises)
            .HasForeignKey(etm => etm.MuscleGroupId);
            
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
            .WithMany(e => e.Equipment)
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
            .WithMany(e => e.SupportedMetrics)
            .HasForeignKey(ems => ems.ExerciseId);
            
        modelBuilder.Entity<ExerciseMetricSupport>()
            .HasOne(ems => ems.MetricType)
            .WithMany(mt => mt.Exercises)
            .HasForeignKey(ems => ems.MetricTypeId);
    }
    
    private static void ConfigureOneToManyRelationships(ModelBuilder modelBuilder)
    {
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
            .WithMany(e => e.WorkoutLogSets)
            .HasForeignKey(wls => wls.ExerciseId);
            
        // Configure decimal precision for WorkoutLogSet.WeightUsedKg
        modelBuilder.Entity<WorkoutLogSet>()
            .Property(wls => wls.WeightUsedKg)
            .HasPrecision(10, 2);
    }
}
