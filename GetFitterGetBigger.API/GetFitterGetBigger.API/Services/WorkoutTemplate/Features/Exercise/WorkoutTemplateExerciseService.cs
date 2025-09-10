using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
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
    IReorderExerciseHandler reorderExerciseHandler,
    ICopyRoundHandler copyRoundHandler,
    IValidationHandler validationHandler,
    IEnhancedMethodsHandler enhancedMethodsHandler) : IWorkoutTemplateExerciseService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly IReorderExerciseHandler _reorderExerciseHandler = reorderExerciseHandler;
    private readonly ICopyRoundHandler _copyRoundHandler = copyRoundHandler;
    private readonly IValidationHandler _validationHandler = validationHandler;
    private readonly IEnhancedMethodsHandler _enhancedMethodsHandler = enhancedMethodsHandler;



    // Enhanced interface methods - Restored from LegacyMethodsHandler and adapted to DTO pattern
    public Task<ServiceResult<AddExerciseResultDto>> AddExerciseAsync(WorkoutTemplateId templateId, AddExerciseDto dto) 
        => _enhancedMethodsHandler.AddExerciseAsync(templateId, dto);
    
    public Task<ServiceResult<RemoveExerciseResultDto>> RemoveExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId) 
        => _enhancedMethodsHandler.RemoveExerciseAsync(templateId, exerciseId);
    
    public Task<ServiceResult<UpdateMetadataResultDto>> UpdateExerciseMetadataAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, string metadata) 
        => _enhancedMethodsHandler.UpdateExerciseMetadataAsync(templateId, exerciseId, metadata);
    
    public async Task<ServiceResult<ReorderResultDto>> ReorderExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, int newOrderInRound) 
        => await _reorderExerciseHandler.ReorderExerciseAsync(templateId, exerciseId, newOrderInRound);
    
    public async Task<ServiceResult<CopyRoundResultDto>> CopyRoundAsync(WorkoutTemplateId templateId, CopyRoundDto dto) 
        => await _copyRoundHandler.CopyRoundAsync(templateId, dto);
    
    public Task<ServiceResult<WorkoutTemplateExercisesDto>> GetTemplateExercisesAsync(WorkoutTemplateId templateId) 
        => _enhancedMethodsHandler.GetTemplateExercisesAsync(templateId);
    
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
}