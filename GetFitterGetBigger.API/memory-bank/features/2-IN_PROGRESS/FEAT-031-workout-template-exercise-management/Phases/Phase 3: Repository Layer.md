## Phase 3: Repository Layer - Estimated: 3h 0m

### Task 3.1: Create IWorkoutTemplateExerciseRepository interface
`[Pending]` (Est: 1h)

**Implementation:**
- Create `/GetFitterGetBigger.API/Repositories/Interfaces/IWorkoutTemplateExerciseRepository.cs`
- Follow repository patterns from ExerciseLink implementation:

```csharp
public interface IWorkoutTemplateExerciseRepository
{
    // CRUD Operations
    Task<WorkoutTemplateExercise> GetByIdAsync(WorkoutTemplateExerciseId id);
    Task<List<WorkoutTemplateExercise>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId);
    Task<List<WorkoutTemplateExercise>> GetByTemplateAndPhaseAsync(WorkoutTemplateId workoutTemplateId, string phase);
    Task<List<WorkoutTemplateExercise>> GetByTemplatePhaseAndRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber);
    
    // Auto-linking support queries
    Task<List<WorkoutTemplateExercise>> GetWorkoutExercisesAsync(WorkoutTemplateId workoutTemplateId);
    Task<bool> ExistsInTemplateAsync(WorkoutTemplateId workoutTemplateId, ExerciseId exerciseId);
    Task<bool> ExistsInPhaseAsync(WorkoutTemplateId workoutTemplateId, string phase, ExerciseId exerciseId);
    
    // Order management
    Task<int> GetMaxOrderInRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber);
    Task ReorderExercisesInRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber, Dictionary<WorkoutTemplateExerciseId, int> newOrders);
    
    // Round management
    Task<List<WorkoutTemplateExercise>> GetRoundExercisesAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber);
    Task<int> GetMaxRoundNumberAsync(WorkoutTemplateId workoutTemplateId, string phase);
    
    // Modification operations
    Task AddAsync(WorkoutTemplateExercise exercise);
    Task AddRangeAsync(List<WorkoutTemplateExercise> exercises);
    Task UpdateAsync(WorkoutTemplateExercise exercise);
    Task DeleteAsync(WorkoutTemplateExerciseId id);
    Task DeleteRangeAsync(List<WorkoutTemplateExerciseId> ids);
}
```

**Unit Tests:**
- Test interface contract and method signatures
- Test specialized ID type usage
- Test parameter validation requirements

**Critical Patterns:**
- Follow interface patterns from `/GetFitterGetBigger.API/Repositories/Interfaces/IExerciseLinkRepository.cs`
- Use specialized ID types (WorkoutTemplateExerciseId, WorkoutTemplateId, ExerciseId)
- Support both individual and batch operations
- Include auto-linking support queries

### Task 3.2: Implement WorkoutTemplateExerciseRepository
`[Pending]` (Est: 2h)

**Implementation:**
- Create `/GetFitterGetBigger.API/Repositories/Implementations/WorkoutTemplateExerciseRepository.cs`
- Extend from RepositoryBase pattern:

```csharp
public class WorkoutTemplateExerciseRepository : RepositoryBase<WorkoutTemplateExercise, WorkoutTemplateExerciseId>, IWorkoutTemplateExerciseRepository
{
    public WorkoutTemplateExerciseRepository(FitnessDbContext context) : base(context) { }

    public async Task<List<WorkoutTemplateExercise>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId)
    {
        return await Context.WorkoutTemplateExercises
            .Include(wte => wte.Exercise)
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId)
            .OrderBy(wte => wte.Phase)
            .ThenBy(wte => wte.RoundNumber)
            .ThenBy(wte => wte.OrderInRound)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<WorkoutTemplateExercise>> GetByTemplateAndPhaseAsync(WorkoutTemplateId workoutTemplateId, string phase)
    {
        return await Context.WorkoutTemplateExercises
            .Include(wte => wte.Exercise)
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && wte.Phase == phase)
            .OrderBy(wte => wte.RoundNumber)
            .ThenBy(wte => wte.OrderInRound)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<bool> ExistsInTemplateAsync(WorkoutTemplateId workoutTemplateId, ExerciseId exerciseId)
    {
        return await Context.WorkoutTemplateExercises
            .AnyAsync(wte => wte.WorkoutTemplateId == workoutTemplateId && 
                           wte.ExerciseId == exerciseId);
    }
    
    public async Task<int> GetMaxOrderInRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber)
    {
        var maxOrder = await Context.WorkoutTemplateExercises
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && 
                         wte.Phase == phase && 
                         wte.RoundNumber == roundNumber)
            .Select(wte => wte.OrderInRound)
            .DefaultIfEmpty(0)
            .MaxAsync();
            
        return maxOrder;
    }
    
    public async Task ReorderExercisesInRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber, Dictionary<WorkoutTemplateExerciseId, int> newOrders)
    {
        var exercises = await Context.WorkoutTemplateExercises
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && 
                         wte.Phase == phase && 
                         wte.RoundNumber == roundNumber)
            .ToListAsync();
        
        foreach (var exercise in exercises)
        {
            if (newOrders.TryGetValue(exercise.Id, out var newOrder))
            {
                // Use record 'with' syntax to update
                var updatedExercise = exercise with 
                { 
                    OrderInRound = newOrder,
                    UpdatedAt = DateTime.UtcNow
                };
                Context.WorkoutTemplateExercises.Update(updatedExercise);
            }
        }
    }
    
    // Additional methods following same patterns...
}
```

**Unit Tests:**
- Test all repository methods with mock data
- Test Include() navigation property loading
- Test ordering by Phase → Round → Order
- Test efficient queries with AsNoTracking()
- Test batch operations
- Test error scenarios (not found, invalid parameters)

**Critical Patterns:**
- Follow repository patterns from `/GetFitterGetBigger.API/Repositories/Implementations/ExerciseLinkRepository.cs`
- Use AsNoTracking() for query performance
- Use Include() for navigation properties following NavigationLoadingPattern
- Use record 'with' syntax for updates
- Extend from RepositoryBase<T, TId> pattern

## CHECKPOINT: Phase 3 Complete - Repository Layer
`[Pending]`

**Requirements for Completion:**
- Build: ✅ 0 errors, 0 warnings
- Tests: ✅ All repository methods tested + existing tests passing
- Performance: ✅ Query optimization with AsNoTracking() and indexes
- Git Commit Hash: [MANDATORY - To be added after commit]

**Implementation Summary:**
- **IWorkoutTemplateExerciseRepository**: Complete interface with auto-linking support
- **WorkoutTemplateExerciseRepository**: Full implementation with EF Core patterns
- **Query Optimization**: AsNoTracking() and Include() for performance
- **Batch Operations**: Support for bulk add/update/delete operations

**Test Requirements:**
- Repository Tests: All CRUD methods with comprehensive scenarios
- Performance Tests: Query execution time validation
- Integration Tests: Database operations with real DbContext
- Mock Tests: Unit tests with in-memory database

**Code Review**: Follow `/memory-bank/DevelopmentGuidelines/Templates/FeatureCheckpointTemplate.md`
- **Status**: [To be filled after review]
- **Review Path**: `/memory-bank/features/2-IN_PROGRESS/FEAT-031-workout-template-exercise-management/code-reviews/Phase_3_Repository_Layer/`

**Git Commit**: [MANDATORY - Phase cannot be marked complete without this]