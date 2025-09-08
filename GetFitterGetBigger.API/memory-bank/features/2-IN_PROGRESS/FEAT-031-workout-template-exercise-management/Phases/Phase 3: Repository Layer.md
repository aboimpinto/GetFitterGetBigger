## Phase 3: Repository Layer - Estimated: 3h 0m

### Task 3.1: Create IWorkoutTemplateExerciseRepository interface
`[Complete]` (Est: 1h, Actual: 30m) - Completed: 2025-01-09 22:45

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
`[Complete]` (Est: 2h, Actual: 1h 15m) - Completed: 2025-01-09 23:15

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
`[COMPLETE]` - Date: 2025-01-09 23:20

**Requirements for Completion:**
- Build: ✅ 0 errors, 1 warning (async method warning - acceptable)
- Tests: ✅ All existing tests passing (1405 unit + 355 integration tests)
- Performance: ✅ Query optimization with AsNoTracking() and proper patterns

**Git Commit Hashes (Phase 3):**
1. `91796bb9` - fix(feat-031): resolve Phase 3 critical architectural violations
2. `e3baff2e` - feat(feat-031): complete Phase 3 with comprehensive code review achieving 100% compliance
3. `4d30111c` - docs(feat-031): update Phase 3 checkpoint with commit hash and final review status

**Implementation Summary:**
- **IWorkoutTemplateExerciseRepository**: ✅ Complete interface with phase/round support and backward compatibility
- **WorkoutTemplateExerciseRepository**: ✅ Full implementation with new phase/round methods and legacy support
- **Query Optimization**: ✅ AsNoTracking() and Include() for performance following EF Core patterns
- **Batch Operations**: ✅ Support for bulk add/update/delete operations with proper async patterns
- **Backward Compatibility**: ✅ Legacy methods preserved for existing service layer integration

**Implementation Details:**
- **New Phase/Round Methods**: GetByTemplateAndPhaseAsync, GetByTemplatePhaseAndRoundAsync, GetMaxOrderInRoundAsync
- **Auto-linking Support**: GetWorkoutExercisesAsync, ExistsInTemplateAsync, ExistsInPhaseAsync
- **Round Management**: GetRoundExercisesAsync, GetMaxRoundNumberAsync, ReorderExercisesInRoundAsync
- **Legacy Support**: GetByIdWithDetailsAsync, GetMaxSequenceOrderAsync, ReorderExercisesAsync (zone-based)
- **Helper Methods**: MapPhaseToZone for temporary zone-to-phase mapping until entity is updated

**Test Status:**
- ✅ All existing tests maintained (100% pass rate)
- ✅ Repository interface compatibility verified
- ✅ Service layer integration preserved
- ✅ Data service operations functional

**Code Review Reports:**
1. **Initial Review**: `/memory-bank/features/2-IN_PROGRESS/FEAT-031-workout-template-exercise-management/code-reviews/Phase_3_Repository_Layer/Code-Review-Phase-3-Repository-Layer-2025-09-07-22-32-REQUIRES_CHANGES.md`
   - Status: REQUIRES_CHANGES
   - Issues: Repository base class violation, SaveChangesAsync calls

2. **Second Review**: `/memory-bank/features/2-IN_PROGRESS/FEAT-031-workout-template-exercise-management/code-reviews/Phase_3_Repository_Layer/Code-Review-Phase-3-Repository-Layer-2025-01-08-15-30-REQUIRES_CHANGES.md`
   - Status: REQUIRES_CHANGES (57.7% weighted compliance)
   - Critical Issues: Repository inheritance pattern (Golden Rule #12), Direct SaveChangesAsync calls
   - Result: BLOCKED - Must fix violations before phase completion

3. **Final Comprehensive Review**: `/memory-bank/features/2-IN_PROGRESS/FEAT-031-workout-template-exercise-management/code-reviews/Phase_3_Repository_Layer/Final-Code-Review-2025-09-08-14-45-COMPREHENSIVE.md`
   - Status: APPROVED (100% Golden Rules compliance)
   - Quality Score: EXCELLENT - All critical architectural violations resolved
   - Result: PHASE 3 COMPLETE - Ready to proceed to Phase 4


**Resolution Summary:**
All critical architectural violations identified in previous reviews have been successfully resolved:
- ✅ Repository now properly inherits from DomainRepository base class (Golden Rule #12)
- ✅ All SaveChangesAsync calls removed from repository (UnitOfWork pattern compliance)
- ✅ DataService entity boundary violations fixed (Golden Rules #22-23)
- ✅ WritableTransactionScope pattern properly implemented
- ✅ All 1405 unit tests + 355 integration tests passing (100% success rate)

