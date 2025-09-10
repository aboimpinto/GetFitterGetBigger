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
public class WorkoutTemplateExerciseService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IAutoLinkingHandler autoLinkingHandler,
    IReorderExerciseHandler reorderExerciseHandler,
    ICopyRoundHandler copyRoundHandler,
    IValidationHandler validationHandler,
    ILogger<WorkoutTemplateExerciseService> logger) : IWorkoutTemplateExerciseService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly IAutoLinkingHandler _autoLinkingHandler = autoLinkingHandler;
    private readonly IReorderExerciseHandler _reorderExerciseHandler = reorderExerciseHandler;
    private readonly ICopyRoundHandler _copyRoundHandler = copyRoundHandler;
    private readonly IValidationHandler _validationHandler = validationHandler;
    private readonly ILogger<WorkoutTemplateExerciseService> _logger = logger;







    /// <inheritdoc />
    public async Task<ServiceResult<ExerciseListDto>> GetExerciseSuggestionsAsync(WorkoutTemplateId workoutTemplateId, string zone, int maxSuggestions = 5)
    {
        return await ServiceValidate.For<ExerciseListDto>()
            .EnsureNotEmpty(workoutTemplateId, WorkoutTemplateExerciseErrorMessages.InvalidTemplateIdOrZone)
            .EnsureNotWhiteSpace(zone, WorkoutTemplateExerciseErrorMessages.InvalidTemplateIdOrZone)
            .MatchAsync(
                whenValid: async () =>
                {
                    return await Task.FromResult(ServiceResult<ExerciseListDto>.Success(ExerciseListDto.Empty));
                }
            );
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BooleanResultDto>> ValidateExercisesAsync(WorkoutTemplateId workoutTemplateId, List<ExerciseId> exerciseIds)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(workoutTemplateId, WorkoutTemplateExerciseErrorMessages.InvalidTemplateIdOrExerciseList)
            .Ensure(
                () => exerciseIds != null && exerciseIds.Count > 0,
                WorkoutTemplateExerciseErrorMessages.InvalidTemplateIdOrExerciseList)
            .EnsureAsync(
                async () => await _validationHandler.DoesTemplateExistAsync(workoutTemplateId),
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound))
            .EnsureAsync(
                async () => await AreAllExercisesValidAsync(exerciseIds),
                ServiceError.NotFound("One or more exercises not found"))
            .MatchAsync(
                whenValid: async () => await Task.FromResult(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)))
            );
    }
    
    /// <summary>
    /// Checks if all provided exercise IDs are valid
    /// </summary>
    private async Task<bool> AreAllExercisesValidAsync(List<ExerciseId> exerciseIds)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var exerciseRepository = unitOfWork.GetRepository<IExerciseRepository>();
        return await exerciseRepository.AreAllExercisesValidAsync(exerciseIds);
    }


    // Enhanced interface methods - Phase/Round support (To be implemented in Phase 5)
    public Task<ServiceResult<AddExerciseResultDto>> AddExerciseAsync(WorkoutTemplateId templateId, AddExerciseDto dto) 
        => throw new NotImplementedException("Will be implemented in Phase 5");
    
    public Task<ServiceResult<RemoveExerciseResultDto>> RemoveExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId) 
        => throw new NotImplementedException("Will be implemented in Phase 5");
    
    public Task<ServiceResult<UpdateMetadataResultDto>> UpdateExerciseMetadataAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, string metadata) 
        => throw new NotImplementedException("Will be implemented in Phase 5");
    
    public async Task<ServiceResult<ReorderResultDto>> ReorderExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, int newOrderInRound) 
        => await _reorderExerciseHandler.ReorderExerciseAsync(templateId, exerciseId, newOrderInRound);
    
    public async Task<ServiceResult<CopyRoundResultDto>> CopyRoundAsync(WorkoutTemplateId templateId, CopyRoundDto dto) 
        => await _copyRoundHandler.CopyRoundAsync(templateId, dto);
    
    public Task<ServiceResult<WorkoutTemplateExercisesDto>> GetTemplateExercisesAsync(WorkoutTemplateId templateId) 
        => throw new NotImplementedException("Will be implemented in Phase 5");
    
    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> GetExerciseByIdAsync(WorkoutTemplateExerciseId exerciseId)
    {
        return await ServiceValidate.For<WorkoutTemplateExerciseDto>()
            .EnsureNotEmpty(exerciseId, WorkoutTemplateExerciseErrorMessages.InvalidExerciseId)
            .MatchAsync(
                whenValid: async () => await LoadExerciseByIdDirectAsync(exerciseId)
            );
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> LoadExerciseByIdDirectAsync(WorkoutTemplateExerciseId exerciseId)
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
    
    public Task<ServiceResult<BooleanResultDto>> ValidateExerciseMetadataAsync(ExerciseId exerciseId, ExecutionProtocolId protocolId, string metadata) 
        => throw new NotImplementedException("Will be implemented in Phase 5");
}