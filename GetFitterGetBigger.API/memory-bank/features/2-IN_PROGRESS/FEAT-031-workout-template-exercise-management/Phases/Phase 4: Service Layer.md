## Phase 4: Service Layer - Estimated: 6h 0m

### Task 4.1: Create WorkoutTemplateExerciseService interface
`[Complete]` (Est: 1h, Actual: 1h 15m) - Completed: 2025-09-08 17:30

**CRITICAL**: Before implementing service methods, review `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md` Section 1 about UnitOfWork usage.

**Implementation:**
- Create `/GetFitterGetBigger.API/Services/WorkoutTemplate/Features/Exercise/IWorkoutTemplateExerciseService.cs`
- Follow service patterns from ExerciseLink implementation:

```csharp
public interface IWorkoutTemplateExerciseService
{
    // Core exercise management
    Task<ServiceResult<AddExerciseResultDto>> AddExerciseAsync(WorkoutTemplateId templateId, AddExerciseDto dto);
    Task<ServiceResult<RemoveExerciseResultDto>> RemoveExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId);
    Task<ServiceResult<UpdateMetadataResultDto>> UpdateExerciseMetadataAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, string metadata);
    Task<ServiceResult<ReorderResultDto>> ReorderExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, int newOrderInRound);
    
    // Round management
    Task<ServiceResult<CopyRoundResultDto>> CopyRoundAsync(WorkoutTemplateId templateId, CopyRoundDto dto);
    
    // Query operations  
    Task<ServiceResult<WorkoutTemplateExercisesDto>> GetTemplateExercisesAsync(WorkoutTemplateId templateId);
    Task<ServiceResult<WorkoutTemplateExerciseDto>> GetExerciseByIdAsync(WorkoutTemplateExerciseId exerciseId);
    
    // Validation
    Task<ServiceResult<BooleanResultDto>> ValidateExerciseMetadataAsync(ExerciseId exerciseId, ExecutionProtocolId protocolId, string metadata);
}
```

**DTOs Required:**
```csharp
public record AddExerciseDto(
    ExerciseId ExerciseId,
    string Phase,
    int RoundNumber,
    string Metadata);

public record AddExerciseResultDto(
    List<WorkoutTemplateExerciseDto> AddedExercises,
    string Message);

public record RemoveExerciseResultDto(
    List<WorkoutTemplateExerciseDto> RemovedExercises,
    string Message);

public record CopyRoundDto(
    string SourcePhase,
    int SourceRoundNumber,
    string TargetPhase,
    int TargetRoundNumber);

public record WorkoutTemplateExercisesDto(
    WorkoutTemplateId TemplateId,
    string TemplateName,
    ExecutionProtocolDto ExecutionProtocol,
    WorkoutPhaseDto Warmup,
    WorkoutPhaseDto Workout,
    WorkoutPhaseDto Cooldown);

public record WorkoutPhaseDto(
    List<RoundDto> Rounds);

public record RoundDto(
    int RoundNumber,
    List<WorkoutTemplateExerciseDto> Exercises);
```

**Unit Tests:**
- Test interface contract definition
- Test DTO structure and validation
- Test ServiceResult<T> return types

**Critical Patterns:**
- Follow interface patterns from `/GetFitterGetBigger.API/Services/Exercise/Features/Links/IExerciseLinkService.cs`
- All methods return ServiceResult<T>
- Use specialized ID types throughout
- Support auto-linking operations

### Task 4.2: Implement core service methods with auto-linking logic
`[Complete]` (Est: 3h, Actual: 45m) - Completed: 2025-09-08 19:15

**CRITICAL**: Read `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md` before starting. Use ReadOnlyUnitOfWork for validation, WritableUnitOfWork ONLY for modifications.

**Implementation:**
- Create `/GetFitterGetBigger.API/Services/WorkoutTemplate/Features/Exercise/WorkoutTemplateExerciseService.cs`
- Follow patterns from ExerciseLinkService with proper UnitOfWork usage:

```csharp
public class WorkoutTemplateExerciseService : IWorkoutTemplateExerciseService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IExerciseLinkQueryDataService _exerciseLinkDataService;
    private readonly ILogger<WorkoutTemplateExerciseService> _logger;

    public async Task<ServiceResult<AddExerciseResultDto>> AddExerciseAsync(WorkoutTemplateId templateId, AddExerciseDto dto)
    {
        return await ServiceValidate.Build<AddExerciseResultDto>()
            .EnsureNotEmpty(templateId, WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound)
            .EnsureNotEmpty(dto.ExerciseId, WorkoutTemplateExerciseErrorMessages.InvalidExerciseId)
            .EnsureNotEmpty(dto.Phase, WorkoutTemplateExerciseErrorMessages.InvalidZone)
            .Ensure(() => IsValidPhase(dto.Phase), WorkoutTemplateExerciseErrorMessages.InvalidZoneWarmupMainCooldown)
            .EnsureMinValue(dto.RoundNumber, 1, "Round number must be at least 1")
            .EnsureNotEmpty(dto.Metadata, "Metadata is required for exercise configuration")
            .EnsureAsync(
                async () => await IsTemplateInDraftStateAsync(templateId),
                WorkoutTemplateExerciseErrorMessages.CanOnlyAddExercisesToDraftTemplates)
            .EnsureAsync(
                async () => await IsExerciseActiveAsync(dto.ExerciseId),
                WorkoutTemplateExerciseErrorMessages.ExerciseNotFound)
            .MatchAsync(
                whenValid: async () => await ProcessAddExerciseWithAutoLinkingAsync(templateId, dto),
                whenInvalid: errors => ServiceResult<AddExerciseResultDto>.Failure(
                    AddExerciseResultDto.Empty,
                    errors.FirstOrDefault()));
    }

    private async Task<ServiceResult<AddExerciseResultDto>> ProcessAddExerciseWithAutoLinkingAsync(WorkoutTemplateId templateId, AddExerciseDto dto)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        var addedExercises = new List<WorkoutTemplateExercise>();

        // Calculate order in round
        var orderInRound = await repository.GetMaxOrderInRoundAsync(templateId, dto.Phase, dto.RoundNumber) + 1;

        // Create main exercise
        var mainExercise = WorkoutTemplateExercise.Handler.CreateNew(
            templateId,
            dto.ExerciseId,
            dto.Phase,
            dto.RoundNumber,
            orderInRound,
            dto.Metadata);

        if (!mainExercise.IsSuccess)
        {
            return ServiceResult<AddExerciseResultDto>.Failure(
                AddExerciseResultDto.Empty,
                ServiceError.ValidationFailed(string.Join(", ", mainExercise.Errors)));
        }

        await repository.AddAsync(mainExercise.Value);
        addedExercises.Add(mainExercise.Value);

        // Auto-link if this is a workout exercise
        if (dto.Phase == "Workout")
        {
            var linkedExercises = await AddAutoLinkedExercisesAsync(repository, templateId, dto.ExerciseId);
            addedExercises.AddRange(linkedExercises);
        }

        await unitOfWork.CommitAsync();

        var resultDto = new AddExerciseResultDto(
            addedExercises.Select(e => MapToDto(e)).ToList(),
            $"Successfully added {addedExercises.Count} exercise(s)");

        return ServiceResult<AddExerciseResultDto>.Success(resultDto);
    }

    private async Task<List<WorkoutTemplateExercise>> AddAutoLinkedExercisesAsync(
        IWorkoutTemplateExerciseRepository repository, 
        WorkoutTemplateId templateId, 
        ExerciseId workoutExerciseId)
    {
        var addedExercises = new List<WorkoutTemplateExercise>();

        // Use ReadOnlyUnitOfWork for querying ExerciseLinks
        using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
        
        // Query linked exercises (warmup/cooldown)
        // Note: GetLinkedExercisesAsync doesn't exist, use GetBySourceExerciseAsync for each type
        var warmupLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(workoutExerciseId, ExerciseLinkType.WARMUP.ToString());
        var cooldownLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(workoutExerciseId, ExerciseLinkType.COOLDOWN.ToString());
        
        // Combine results
        var allLinks = new List<ExerciseLinkDto>();
        if (warmupLinks.IsSuccess) allLinks.AddRange(warmupLinks.Data);
        if (cooldownLinks.IsSuccess) allLinks.AddRange(cooldownLinks.Data);
        
        var linkedExercises = ServiceResult<List<ExerciseLinkDto>>.Success(allLinks);
        
        if (!linkedExercises.IsSuccess || !linkedExercises.Data.Any())
            return addedExercises;

        foreach (var linkedExercise in linkedExercises.Data)
        {
            var targetPhase = linkedExercise.LinkType == ExerciseLinkType.WARMUP ? "Warmup" : "Cooldown";
            
            // Check if already exists in template
            var alreadyExists = await repository.ExistsInPhaseAsync(templateId, targetPhase, linkedExercise.TargetExerciseId);
            
            if (!alreadyExists)
            {
                // Get max round number for target phase, default to 1
                var maxRound = await repository.GetMaxRoundNumberAsync(templateId, targetPhase);
                var targetRound = Math.Max(1, maxRound);
                
                var orderInPhase = await repository.GetMaxOrderInRoundAsync(templateId, targetPhase, targetRound) + 1;

                var autoLinkedExercise = WorkoutTemplateExercise.Handler.CreateNew(
                    templateId,
                    linkedExercise.TargetExerciseId,
                    targetPhase,
                    targetRound,
                    orderInPhase,
                    "{}"); // Empty metadata - PT must configure

                if (autoLinkedExercise.IsSuccess)
                {
                    await repository.AddAsync(autoLinkedExercise.Value);
                    addedExercises.Add(autoLinkedExercise.Value);
                }
            }
        }

        return addedExercises;
    }

    // Critical: Use ReadOnlyUnitOfWork for validation queries
    private async Task<bool> IsTemplateInDraftStateAsync(WorkoutTemplateId templateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var templateRepo = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var template = await templateRepo.GetByIdAsync(templateId);
        return !template.IsEmpty && template.WorkoutState?.Value == "Draft";
    }

    private async Task<bool> IsExerciseActiveAsync(ExerciseId exerciseId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var exerciseRepo = unitOfWork.GetRepository<IExerciseRepository>();
        
        var exercise = await exerciseRepo.GetByIdAsync(exerciseId);
        return !exercise.IsEmpty && exercise.IsActive;
    }
}
```

**Unit Tests:**
- Test AddExerciseAsync with all validation scenarios
- Test auto-linking logic for workout exercises
- Test proper UnitOfWork usage (ReadOnly vs Writable)
- Test error handling and ServiceResult patterns
- Test ServiceValidate chain execution

**Critical Patterns:**
- **CRITICAL**: Use ReadOnlyUnitOfWork for ALL validation queries
- Only use WritableUnitOfWork for actual data modifications
- Follow ServiceValidate pattern from `/GetFitterGetBigger.API/Services/Exercise/Features/Links/ExerciseLinkService.cs`
- Use ServiceResult<T> pattern consistently
- Check `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md` Section 1

### Task 4.3: Implement remove exercise with orphan cleanup
`[Pending]` (Est: 1h 30m)

**Implementation:**
- Implement RemoveExerciseAsync with orphan detection logic:

```csharp
public async Task<ServiceResult<RemoveExerciseResultDto>> RemoveExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId)
{
    return await ServiceValidate.Build<RemoveExerciseResultDto>()
        .EnsureNotEmpty(templateId, WorkoutTemplateExerciseErrorMessages.InvalidTemplateId)
        .EnsureNotEmpty(exerciseId, WorkoutTemplateExerciseErrorMessages.InvalidExerciseId)
        .EnsureAsync(
            async () => await IsTemplateInDraftStateAsync(templateId),
            WorkoutTemplateExerciseErrorMessages.TemplateNotInDraftState)
        .EnsureAsync(
            async () => await DoesExerciseExistInTemplateAsync(exerciseId, templateId),
            WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound)
        .MatchAsync(
            whenValid: async () => await ProcessRemoveExerciseWithCleanupAsync(templateId, exerciseId));
}

private async Task<ServiceResult<RemoveExerciseResultDto>> ProcessRemoveExerciseWithCleanupAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
    var removedExercises = new List<WorkoutTemplateExercise>();

    // Get the exercise being removed
    var exerciseToRemove = await repository.GetByIdAsync(exerciseId);
    removedExercises.Add(exerciseToRemove);

    // If it's a workout exercise, find orphaned warmup/cooldown exercises
    if (exerciseToRemove.Phase == "Workout")
    {
        var orphanedExercises = await FindOrphanedExercisesAsync(repository, templateId, exerciseToRemove.ExerciseId);
        removedExercises.AddRange(orphanedExercises);
    }

    // Remove all exercises
    var exerciseIds = removedExercises.Select(e => e.Id).ToList();
    await repository.DeleteRangeAsync(exerciseIds);

    // Reorder remaining exercises in affected rounds
    await ReorderRemainingExercisesAsync(repository, templateId, removedExercises);

    await unitOfWork.CommitAsync();

    var resultDto = new RemoveExerciseResultDto(
        removedExercises.Select(e => MapToDto(e)).ToList(),
        $"Successfully removed {removedExercises.Count} exercise(s)");

    return ServiceResult<RemoveExerciseResultDto>.Success(resultDto);
}

private async Task<List<WorkoutTemplateExercise>> FindOrphanedExercisesAsync(
    IWorkoutTemplateExerciseRepository repository, 
    WorkoutTemplateId templateId, 
    ExerciseId removedWorkoutExerciseId)
{
    var orphanedExercises = new List<WorkoutTemplateExercise>();

    // Use ReadOnlyUnitOfWork for querying ExerciseLinks
    using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
    
    // Get all linked exercises for the removed workout exercise
    var warmupLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(removedWorkoutExerciseId, ExerciseLinkType.WARMUP.ToString());
    var cooldownLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(removedWorkoutExerciseId, ExerciseLinkType.COOLDOWN.ToString());
    
    // Combine results
    var allLinks = new List<ExerciseLinkDto>();
    if (warmupLinks.IsSuccess) allLinks.AddRange(warmupLinks.Data);
    if (cooldownLinks.IsSuccess) allLinks.AddRange(cooldownLinks.Data);
    
    var linkedExercises = ServiceResult<List<ExerciseLinkDto>>.Success(allLinks);
    
    if (!linkedExercises.IsSuccess) return orphanedExercises;

    foreach (var linkedExercise in linkedExercises.Data)
    {
        // Check if this warmup/cooldown exercise is used by any OTHER workout exercise
        var otherWorkoutExercises = await repository.GetWorkoutExercisesAsync(templateId);
        var stillNeeded = false;

        foreach (var otherWorkout in otherWorkoutExercises.Where(e => e.ExerciseId != removedWorkoutExerciseId))
        {
            // Check if other workout exercises link to this warmup/cooldown
            // Use the specific link type to check for dependencies
            var otherLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(
                otherWorkout.ExerciseId, 
                linkedExercise.LinkType.ToString());
            
            if (otherLinks.IsSuccess && otherLinks.Data.Any(l => l.TargetExerciseId == linkedExercise.TargetExerciseId))
            {
                stillNeeded = true;
                break;
            }
        }

        if (!stillNeeded)
        {
            // Find this exercise in the template and mark for removal
            var targetPhase = linkedExercise.LinkType == ExerciseLinkType.WARMUP ? "Warmup" : "Cooldown";
            var exercisesInPhase = await repository.GetByTemplateAndPhaseAsync(templateId, targetPhase);
            var orphan = exercisesInPhase.FirstOrDefault(e => e.ExerciseId == linkedExercise.TargetExerciseId);
            
            if (orphan != null)
            {
                orphanedExercises.Add(orphan);
            }
        }
    }

    return orphanedExercises;
}
```

**Unit Tests:**
- Test RemoveExerciseAsync with various scenarios
- Test orphan detection logic
- Test that non-orphaned exercises are preserved
- Test reordering after removal
- Test ServiceValidate chain for removal

**Critical Patterns:**
- Use ReadOnlyUnitOfWork for orphan detection queries
- Use WritableUnitOfWork only for actual deletions
- Follow complex business logic patterns from ExerciseLinkService
- Proper error handling with ServiceResult<T>

### Task 4.4: Implement round management and reordering
`[Pending]` (Est: 30m)

**Implementation:**
- Implement CopyRoundAsync and ReorderExerciseAsync methods following the same patterns
- Include proper validation and ServiceResult<T> return types
- Use appropriate UnitOfWork patterns

**Unit Tests:**
- Test round copying with new GUIDs
- Test exercise reordering within rounds
- Test validation scenarios for round operations

## CHECKPOINT: Phase 4 Complete - Service Layer
`[Complete]` - Completed: 2025-09-09 with advanced validation patterns

**Requirements for Completion:**
- Build: ✅ 0 errors, 0 warnings
- Tests: ✅ All service methods tested + existing tests passing
- Patterns: ✅ ServiceValidate and ServiceResult<T> used consistently
- UnitOfWork: ✅ Proper ReadOnly vs Writable usage verified

**Implementation Summary:**
- **IWorkoutTemplateExerciseService**: Complete interface with all required methods
- **Advanced Validation Patterns**: TransactionalServiceValidationBuilder with chained extensions
- **WorkoutTemplateExerciseValidationExtensions**: Domain-specific validation logic
- **WorkoutTemplateExerciseChainedExtensions**: Fluent exercise operation extensions
- **Enhanced Service Architecture**: Improved separation of concerns with comprehensive validation chains
- **ServiceValidationExtensions**: Additional helper methods for validation patterns

**Test Requirements:**
- Service Tests: All methods with comprehensive scenario coverage
- Validation Tests: Complex business rule validation chains
- UnitOfWork Tests: Proper ReadOnly vs Writable usage
- Integration Tests: End-to-end service functionality

**Code Review Status:**
- Status: [To be added after review]
- Report: [To be added after review]

**Git Commits (Phase 4):**
- 2d291226: feat(feat-031): implement comprehensive transactional validation pattern and service architecture refactoring
- 7ba4d325: feat(feat-031): complete Phase 4 service layer with advanced validation patterns and chained extensions

**Phase 4 Achievements:**
- Implemented advanced transactional validation builder pattern
- Created comprehensive domain-specific validation extensions
- Enhanced service architecture with improved separation of concerns
- Added fluent chained extensions for exercise operations
- Established robust validation chains for business rule enforcement
