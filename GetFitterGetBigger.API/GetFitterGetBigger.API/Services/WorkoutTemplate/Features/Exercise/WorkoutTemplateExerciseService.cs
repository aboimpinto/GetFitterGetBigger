using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Extensions;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;

/// <summary>
/// Service implementation for managing exercises within workout templates
/// </summary>
public class WorkoutTemplateExerciseService : IWorkoutTemplateExerciseService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IAutoLinkingHandler _autoLinkingHandler;
    private readonly IReorderExerciseHandler _reorderExerciseHandler;
    private readonly ICopyRoundHandler _copyRoundHandler;
    private readonly IValidationHandler _validationHandler;
    private readonly ILogger<WorkoutTemplateExerciseService> _logger;

    public WorkoutTemplateExerciseService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IAutoLinkingHandler autoLinkingHandler,
        IReorderExerciseHandler reorderExerciseHandler,
        ICopyRoundHandler copyRoundHandler,
        IValidationHandler validationHandler,
        ILogger<WorkoutTemplateExerciseService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _autoLinkingHandler = autoLinkingHandler;
        _reorderExerciseHandler = reorderExerciseHandler;
        _copyRoundHandler = copyRoundHandler;
        _validationHandler = validationHandler;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WorkoutTemplateExerciseListDto>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId)
    {
        return await ServiceValidate.For<WorkoutTemplateExerciseListDto>()
            .EnsureNotEmpty(workoutTemplateId, WorkoutTemplateExerciseErrorMessages.InvalidWorkoutTemplateId)
            .MatchAsync(
                whenValid: async () => await LoadExercisesByTemplateAsync(workoutTemplateId)
            );
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseListDto>> LoadExercisesByTemplateAsync(WorkoutTemplateId workoutTemplateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exercises = (await repository.GetByWorkoutTemplateAsync(workoutTemplateId)).ToList();

        var dto = new WorkoutTemplateExerciseListDto
        {
            WorkoutTemplateId = workoutTemplateId.ToString(),
            WarmupExercises = exercises
                .Where(e => e.Zone == WorkoutZone.Warmup)
                .OrderBy(e => e.SequenceOrder)
                .Select(e => e.ToDto())
                .ToList(),
            MainExercises = exercises
                .Where(e => e.Zone == WorkoutZone.Main)
                .OrderBy(e => e.SequenceOrder)
                .Select(e => e.ToDto())
                .ToList(),
            CooldownExercises = exercises
                .Where(e => e.Zone == WorkoutZone.Cooldown)
                .OrderBy(e => e.SequenceOrder)
                .Select(e => e.ToDto())
                .ToList(),
            TotalEstimatedDurationMinutes = CalculateEstimatedDuration(exercises)
        };

        return ServiceResult<WorkoutTemplateExerciseListDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> GetByIdAsync(WorkoutTemplateExerciseId exerciseId)
    {
        return await ServiceValidate.For<WorkoutTemplateExerciseDto>()
            .EnsureNotEmpty(exerciseId, WorkoutTemplateExerciseErrorMessages.InvalidExerciseId)
            .MatchAsync(
                whenValid: async () => await LoadExerciseByIdAsync(exerciseId)
            );
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> LoadExerciseByIdAsync(WorkoutTemplateExerciseId exerciseId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exercise = await repository.GetByIdWithDetailsAsync(exerciseId);

        return exercise.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateExerciseDto>.Failure(WorkoutTemplateExerciseDto.Empty, ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound)),
            false => ServiceResult<WorkoutTemplateExerciseDto>.Success(exercise.ToDto())
        };
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> AddExerciseAsync(AddExerciseToTemplateCommand command)
    {
        return await ServiceValidate.Build<WorkoutTemplateExerciseDto>()
            .EnsureNotEmpty(command.WorkoutTemplateId, WorkoutTemplateExerciseErrorMessages.InvalidWorkoutTemplateId)
            .EnsureNotEmpty(command.ExerciseId, WorkoutTemplateExerciseErrorMessages.InvalidExerciseId)
            .EnsureNotWhiteSpace(command.Zone, WorkoutTemplateExerciseErrorMessages.InvalidZone)
            .EnsureNotEmpty(command.UserId, WorkoutTemplateExerciseErrorMessages.InvalidUserId)
            .EnsureAsync(async () => await IsValidWorkoutZoneAsync(command.Zone), 
                string.Format(WorkoutTemplateExerciseErrorMessages.InvalidZoneWarmupMainCooldown, command.Zone))
            .EnsureAsync(async () => await DoesTemplateExistAsync(command.WorkoutTemplateId), 
                WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound)
            .EnsureAsync(async () => await IsTemplateInDraftStateAsync(command.WorkoutTemplateId), 
                WorkoutTemplateExerciseErrorMessages.CanOnlyAddExercisesToDraftTemplates)
            .EnsureAsync(async () => await IsExerciseActiveAsync(command.ExerciseId), 
                WorkoutTemplateExerciseErrorMessages.ExerciseNotFound)
            .MatchAsync(
                whenValid: async () => await ExecuteAddExerciseWithChainingAsync(command),
                whenInvalid: (IReadOnlyList<ServiceError> errors) => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                    WorkoutTemplateExerciseDto.Empty,
                    errors.First())
            );
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ExecuteAddExerciseWithChainingAsync(AddExerciseToTemplateCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var exerciseTemplateRepo = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        // Calculate sequence order
        var zone = Enum.Parse<WorkoutZone>(command.Zone);
        var sequenceOrder = command.SequenceOrder ?? await exerciseTemplateRepo.GetMaxSequenceOrderAsync(command.WorkoutTemplateId, zone) + 1;
        
        // Create the workout template exercise entity
        var createResult = WorkoutTemplateExercise.Handler.CreateNew(
            command.WorkoutTemplateId, 
            command.ExerciseId, 
            zone, 
            sequenceOrder, 
            command.Notes);
            
        // Use the chained validation pattern similar to WorkoutTemplateCommandDataService
        return await ServiceValidate.Build<WorkoutTemplateExerciseDto>()
            .Ensure(
                () => createResult.IsSuccess,
                ServiceError.ValidationFailed(createResult.IsSuccess ? string.Empty : string.Join(", ", createResult.Errors)))
            .MatchAsync(
                whenValid: async () =>
                {
                    var workoutTemplateExercise = createResult.Value;
                    
                    // Add to repository
                    await exerciseTemplateRepo.AddAsync(workoutTemplateExercise);
                    
                    // Handle auto-linking if main exercise
                    if (zone == WorkoutZone.Main)
                    {
                        await _autoLinkingHandler.AddAutoLinkedExercisesAsync(
                            exerciseTemplateRepo, 
                            workoutTemplateExercise.WorkoutTemplateId, 
                            workoutTemplateExercise.ExerciseId);
                    }
                    
                    // Commit transaction
                    await unitOfWork.CommitAsync();
                    
                    // Reload with details
                    var created = await exerciseTemplateRepo.GetByIdWithDetailsAsync(workoutTemplateExercise.Id);
                    
                    return ServiceResult<WorkoutTemplateExerciseDto>.Success(created.ToDto());
                },
                whenInvalid: (IReadOnlyList<ServiceError> errors) => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                    WorkoutTemplateExerciseDto.Empty, 
                    errors.First())
            );
    }

    /// <summary>
    /// Validates if the zone string can be parsed as a WorkoutZone enum
    /// </summary>
    private static async Task<bool> IsValidWorkoutZoneAsync(string zone)
    {
        await Task.CompletedTask; // Make async for consistency
        return Enum.TryParse<WorkoutZone>(zone, out _);
    }

    /// <summary>
    /// Checks if the workout template exists using the validation handler
    /// </summary>
    private async Task<bool> DoesTemplateExistAsync(WorkoutTemplateId templateId)
    {
        return await _validationHandler.DoesTemplateExistAsync(templateId);
    }

    /// <summary>
    /// Checks if the workout template is in draft state using the validation handler
    /// </summary>
    private async Task<bool> IsTemplateInDraftStateAsync(WorkoutTemplateId templateId)
    {
        return await _validationHandler.IsTemplateInDraftStateAsync(templateId);
    }

    /// <summary>
    /// Checks if the exercise is active using the validation handler
    /// </summary>
    private async Task<bool> IsExerciseActiveAsync(ExerciseId exerciseId)
    {
        return await _validationHandler.IsExerciseActiveAsync(exerciseId);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> UpdateExerciseAsync(UpdateTemplateExerciseCommand command)
    {
        return await ServiceValidate.For<WorkoutTemplateExerciseDto>()
            .EnsureNotNull(command, WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters)
            .ThenEnsureNotEmpty(command.WorkoutTemplateExerciseId, WorkoutTemplateExerciseErrorMessages.InvalidExerciseId)
            .ThenEnsureNotEmpty(command.UserId, WorkoutTemplateExerciseErrorMessages.InvalidUserId)
            .MatchAsync(
                whenValid: async () => await ExecuteUpdateExerciseAsync(command)
            );
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ExecuteUpdateExerciseAsync(UpdateTemplateExerciseCommand command)
    {
        return await ServiceValidate.BuildTransactional<WorkoutTemplateExerciseDto>(_unitOfWorkProvider)
            .ThenLoadAsync<WorkoutTemplateExercise, IWorkoutTemplateExerciseRepository>(
                async repo => await repo.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId))
            .ThenEnsureNotEmptyAsync(
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound))
            .ThenEnsureAsyncChained(
                async entity => await _validationHandler.IsTemplateInDraftStateAsync(entity.WorkoutTemplateId),
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CanOnlyUpdateExercisesInDraftTemplates))
            .ThenTransformChained(
                entity => WorkoutTemplateExercise.Handler.UpdateNotes(entity, command.Notes),
                "Update exercise notes")
            .ThenPerformAsyncChained<WorkoutTemplateExercise, WorkoutTemplateExerciseDto, IWorkoutTemplateExerciseRepository>(
                async (repo, entity) => await repo.UpdateAsync(entity),
                "Save updated exercise")
            .ThenReloadAsyncChained<WorkoutTemplateExercise, WorkoutTemplateExerciseDto, IWorkoutTemplateExerciseRepository, WorkoutTemplateExercise>(
                async (repo, entity) => await repo.GetByIdWithDetailsAsync(entity.Id))
            .ThenCommitAsyncChained(entity => entity.ToDto());
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BooleanResultDto>> RemoveExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId)
    {
        return await ServiceValidate.BuildTransactional<BooleanResultDto>(_unitOfWorkProvider)
            .EnsureNotEmpty(workoutTemplateExerciseId, WorkoutTemplateExerciseErrorMessages.InvalidExerciseId)
            .ThenLoadAsync<WorkoutTemplateExercise, IWorkoutTemplateExerciseRepository>(
                async repo => await repo.GetByIdWithDetailsAsync(workoutTemplateExerciseId))
            .ThenEnsureNotEmptyAsync(
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound))
            .ThenEnsureAsyncChained(
                async entity => await _validationHandler.IsTemplateInDraftStateAsync(entity.WorkoutTemplateId),
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CanOnlyRemoveExercisesFromDraftTemplates))
            .ThenPerformIfZone(
                WorkoutZone.Main,
                async (repo, entity) => await HandleOrphanedExercisesAsync(repo, entity),
                "Handle orphaned exercises for main zone")
            .ThenPerformAsyncChained<WorkoutTemplateExercise, BooleanResultDto, IWorkoutTemplateExerciseRepository>(
                async (repo, _) => await repo.DeleteAsync(workoutTemplateExerciseId),
                "Remove exercise")
            .ThenCommitAsyncChained(_ => BooleanResultDto.Create(true));
    }
    
    private async Task HandleOrphanedExercisesAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateExercise entity)
    {
        var orphanedExercises = await _autoLinkingHandler.FindOrphanedExercisesAsync(
            repository, 
            entity.WorkoutTemplateId, 
            entity.ExerciseId);
        
        foreach (var orphaned in orphanedExercises)
            await repository.DeleteAsync(orphaned.Id);

        var allRemovedExercises = new List<WorkoutTemplateExercise> { entity };
        allRemovedExercises.AddRange(orphanedExercises);
        
        await _reorderExerciseHandler.ReorderAfterRemovalAsync(
            repository, 
            entity.WorkoutTemplateId, 
            allRemovedExercises);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BooleanResultDto>> ReorderExercisesAsync(ReorderTemplateExercisesCommand command)
    {
        return await (await ServiceValidate.BuildTransactional<BooleanResultDto>(_unitOfWorkProvider)
            .EnsureNotEmpty(command.WorkoutTemplateId, WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters)
            .EnsureNotEmpty(command.UserId, WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters)
            .EnsureNotWhiteSpace(command.Zone, WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters)
            .Ensure(
                () => command.ExerciseIds != null && command.ExerciseIds.Count > 0,
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters))
            .EnsureValidZone(command.Zone, WorkoutTemplateExerciseErrorMessages.InvalidZone)
            .EnsureAsync(
                async () => await _validationHandler.IsTemplateInDraftStateAsync(command.WorkoutTemplateId),
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CanOnlyReorderExercisesInDraftTemplates)))
            .ThenExecuteAsync<IWorkoutTemplateExerciseRepository>(async repo =>
            {
                var zone = Enum.Parse<WorkoutZone>(command.Zone);
                await repo.ReorderExercisesAsync(
                    command.WorkoutTemplateId, 
                    zone, 
                    command.ExerciseIds.ToDictionary(id => id, id => command.ExerciseIds.IndexOf(id) + 1));
                return BooleanResultDto.Create(true);
            });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> ChangeExerciseZoneAsync(ChangeExerciseZoneCommand command)
    {
        return await ServiceValidate.BuildTransactional<WorkoutTemplateExerciseDto>(_unitOfWorkProvider)
            .EnsureNotEmpty(command.WorkoutTemplateExerciseId, WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters)
            .EnsureNotEmpty(command.UserId, WorkoutTemplateExerciseErrorMessages.InvalidUserId)
            .EnsureNotWhiteSpace(command.NewZone, WorkoutTemplateExerciseErrorMessages.InvalidZone)
            .EnsureValidZone(command.NewZone, WorkoutTemplateExerciseErrorMessages.InvalidZone)
            .ThenLoadAsync<WorkoutTemplateExercise, IWorkoutTemplateExerciseRepository>(
                async repo => await repo.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId))
            .ThenEnsureNotEmptyAsync(
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound))
            .ThenEnsureAsyncChained(
                async entity => await _validationHandler.IsTemplateInDraftStateAsync(entity.WorkoutTemplateId),
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CanOnlyChangeZonesInDraftTemplates))
            .ThenTransformWorkoutTemplateExerciseAsync(
                async (repo, entity) =>
                {
                    var newZone = Enum.Parse<WorkoutZone>(command.NewZone);
                    var newSequenceOrder = command.NewSequenceOrder ?? 
                        await repo.GetMaxSequenceOrderAsync(entity.WorkoutTemplateId, newZone) + 1;
                    
                    return WorkoutTemplateExercise.Handler.ChangeZone(entity, newZone, newSequenceOrder);
                },
                "Change exercise zone")
            .ThenReloadAsyncChained<WorkoutTemplateExercise, WorkoutTemplateExerciseDto, IWorkoutTemplateExerciseRepository, WorkoutTemplateExercise>(
                async (repo, entity) => await repo.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId))
            .ThenCommitAsyncChained(entity => entity.ToDto());
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> DuplicateExercisesAsync(DuplicateTemplateExercisesCommand command)
    {
        return await ServiceValidate.BuildTransactional<FitnessDbContext, int>(_unitOfWorkProvider)
            // External validations - no context needed
            .EnsureNotEmpty(command.SourceTemplateId, WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters)
            .EnsureNotEmpty(command.TargetTemplateId, WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters) 
            .EnsureNotEmpty(command.UserId, WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters)
            // External async validations using existing validation handler
            .EnsureAsync(
                async () => await _validationHandler.DoesTemplateExistAsync(command.SourceTemplateId),
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.SourceTemplateNotFound))
            .EnsureExistsAsyncChained(
                async () => await _validationHandler.DoesTemplateExistAsync(command.TargetTemplateId),
                WorkoutTemplateExerciseErrorMessages.TargetTemplateNotFound)
            .EnsureAsyncChained(
                async () => await _validationHandler.IsTemplateInDraftStateAsync(command.TargetTemplateId),
                WorkoutTemplateExerciseErrorMessages.CanOnlyDuplicateExercisesToDraftTemplates)
            .EnsureAsyncChained(
                async () => await _validationHandler.TemplateHasExercisesAsync(command.SourceTemplateId),
                WorkoutTemplateExerciseErrorMessages.SourceTemplateHasNoExercisesToDuplicate)
            // Step 1: Create writable repository for exercises
            .ThenCreateWritableRepositoryChained<int, IWorkoutTemplateExerciseRepository>()
            // Step 2: Load source exercises into context
            .ThenLoadAsyncChained("SourceExercises", 
                async context => 
                {
                    var repository = context.GetRepository<IWorkoutTemplateExerciseRepository>(isReadOnly: false);
                    return (await repository.GetByWorkoutTemplateAsync(command.SourceTemplateId)).ToList();
                })
            // Step 3: Duplicate the exercises and store count
            .ThenLoadAsyncChained("DuplicatedCount",
                async context =>
                {
                    var sourceExercises = context.Get<List<WorkoutTemplateExercise>>("SourceExercises");
                    var exerciseRepository = context.GetRepository<IWorkoutTemplateExerciseRepository>(isReadOnly: false);
                    return await exerciseRepository.DuplicateExercisesOnlyAsync(sourceExercises, command.TargetTemplateId);
                })
            // Step 4: Conditionally create set configuration repository (only if needed)
            .ThenExecuteIfChained(
                () => command.IncludeSetConfigurations,
                currentBuilder => currentBuilder.ThenCreateWritableRepository<FitnessDbContext, int, ISetConfigurationRepository>())
            // Step 5: Conditionally duplicate set configurations (only if requested AND exercises were duplicated)
            .ThenPerformIfAsyncChained(
                context => command.IncludeSetConfigurations && context.Get<int>("DuplicatedCount") > 0,
                async context =>
                {
                    var sourceExercises = context.Get<List<WorkoutTemplateExercise>>("SourceExercises");
                    var setConfigurationRepository = context.GetRepository<ISetConfigurationRepository>(isReadOnly: false);
                    var exerciseRepository = context.GetRepository<IWorkoutTemplateExerciseRepository>(isReadOnly: false);
                    
                    await setConfigurationRepository.DuplicateConfigurationsForExercisesAsync(
                        sourceExercises, 
                        command.TargetTemplateId,
                        exerciseRepository);
                })
            // Step 6: Return the duplicated count
            .ThenExecuteAsyncChained(context => Task.FromResult(context.Get<int>("DuplicatedCount")));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<List<ExerciseDto>>> GetExerciseSuggestionsAsync(WorkoutTemplateId workoutTemplateId, string zone, int maxSuggestions = 5)
    {
        if (workoutTemplateId.IsEmpty || string.IsNullOrWhiteSpace(zone))
            return ServiceResult<List<ExerciseDto>>.Failure([], ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.InvalidTemplateIdOrZone));

        // TODO: Implement exercise suggestion logic
        _logger.LogInformation("Exercise suggestions requested for template {TemplateId} zone {Zone}", workoutTemplateId, zone);
        return await Task.FromResult(ServiceResult<List<ExerciseDto>>.Success([]));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BooleanResultDto>> ValidateExercisesAsync(WorkoutTemplateId workoutTemplateId, List<ExerciseId> exerciseIds)
    {
        return await ServiceValidate.Build<BooleanResultDto>()
            .Ensure(
                () => !workoutTemplateId.IsEmpty,
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.InvalidTemplateIdOrExerciseList))
            .Ensure(
                () => exerciseIds != null && exerciseIds.Count > 0,
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.InvalidTemplateIdOrExerciseList))
            .EnsureAsync(
                async () => await _validationHandler.DoesTemplateExistAsync(workoutTemplateId),
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound))
            .EnsureAsync(
                async () => 
                {
                    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                    var exerciseRepo = unitOfWork.GetRepository<IExerciseRepository>();
                    return await exerciseRepo.AreAllExercisesValidAsync(exerciseIds);
                },
                ServiceError.NotFound("One or more exercises not found"))
            .MatchAsync(
                whenValid: async () => await Task.FromResult(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true))),
                whenInvalid: (IReadOnlyList<ServiceError> errors) => ServiceResult<BooleanResultDto>.Failure(BooleanResultDto.Empty, errors.First())
            );
    }

    private static int CalculateEstimatedDuration(List<WorkoutTemplateExercise> exercises)
    {
        // Simple estimation: 
        // - Warmup exercises: 1 minute each
        // - Main exercises: 3 minutes per set (including rest)
        // - Cooldown exercises: 1 minute each
        var warmupMinutes = exercises.Count(e => e.Zone == WorkoutZone.Warmup);
        var mainMinutes = exercises
            .Where(e => e.Zone == WorkoutZone.Main)
            .Sum(e => e.Configurations.Count * 3);
        var cooldownMinutes = exercises.Count(e => e.Zone == WorkoutZone.Cooldown);

        return warmupMinutes + mainMinutes + cooldownMinutes;
    }

    // Enhanced interface methods - Phase/Round support (Task 4.2+)
    public async Task<ServiceResult<AddExerciseResultDto>> AddExerciseAsync(WorkoutTemplateId templateId, AddExerciseDto dto) => await Task.FromResult(ServiceResult<AddExerciseResultDto>.Failure(AddExerciseResultDto.Empty, ServiceError.ValidationFailed("Enhanced AddExerciseAsync will be implemented in Task 4.2")));
    public async Task<ServiceResult<RemoveExerciseResultDto>> RemoveExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId) => await Task.FromResult(ServiceResult<RemoveExerciseResultDto>.Failure(RemoveExerciseResultDto.Empty, ServiceError.ValidationFailed("Enhanced RemoveExerciseAsync will be implemented in Task 4.2")));
    public async Task<ServiceResult<UpdateMetadataResultDto>> UpdateExerciseMetadataAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, string metadata) => await Task.FromResult(ServiceResult<UpdateMetadataResultDto>.Failure(UpdateMetadataResultDto.Empty, ServiceError.ValidationFailed("UpdateExerciseMetadataAsync will be implemented in Task 4.2")));
    public async Task<ServiceResult<ReorderResultDto>> ReorderExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, int newOrderInRound) => await _reorderExerciseHandler.ReorderExerciseAsync(templateId, exerciseId, newOrderInRound);
    public async Task<ServiceResult<CopyRoundResultDto>> CopyRoundAsync(WorkoutTemplateId templateId, CopyRoundDto dto) => await _copyRoundHandler.CopyRoundAsync(templateId, dto);
    public async Task<ServiceResult<WorkoutTemplateExercisesDto>> GetTemplateExercisesAsync(WorkoutTemplateId templateId) => await Task.FromResult(ServiceResult<WorkoutTemplateExercisesDto>.Failure(WorkoutTemplateExercisesDto.Empty, ServiceError.ValidationFailed("GetTemplateExercisesAsync will be implemented in Task 4.2")));
    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> GetExerciseByIdAsync(WorkoutTemplateExerciseId exerciseId) => await GetByIdAsync(exerciseId);
    public async Task<ServiceResult<BooleanResultDto>> ValidateExerciseMetadataAsync(ExerciseId exerciseId, ExecutionProtocolId protocolId, string metadata) => await Task.FromResult(ServiceResult<BooleanResultDto>.Failure(BooleanResultDto.Empty, ServiceError.ValidationFailed("ValidateExerciseMetadataAsync will be implemented in Task 4.2")));
}