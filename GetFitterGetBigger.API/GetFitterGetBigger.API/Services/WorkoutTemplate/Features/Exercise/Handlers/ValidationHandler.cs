using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;

public class ValidationHandler : IValidationHandler
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<ValidationHandler> _logger;

    public ValidationHandler(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<ValidationHandler> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    public async Task<bool> IsTemplateInDraftStateAsync(WorkoutTemplateId templateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var template = await repository.GetByIdAsync(templateId);
        return template != null && !template.IsEmpty && template.WorkoutState?.Value == "Draft";
    }

    public async Task<bool> IsExerciseActiveAsync(ExerciseId exerciseId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseRepository>();
        
        var exercise = await repository.GetByIdAsync(exerciseId);
        return exercise != null && !exercise.IsEmpty && exercise.IsActive;
    }

    public async Task<bool> DoesTemplateExistAsync(WorkoutTemplateId templateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var template = await repository.GetByIdAsync(templateId);
        return template != null && !template.IsEmpty;
    }

    public async Task<bool> DoesExerciseExistInTemplateAsync(
        WorkoutTemplateExerciseId exerciseId,
        WorkoutTemplateId templateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exercise = await repository.GetByIdWithDetailsAsync(exerciseId);
        return exercise != null && !exercise.IsEmpty && exercise.WorkoutTemplateId == templateId;
    }

    public async Task<bool> DoesRoundExistAsync(
        WorkoutTemplateId templateId,
        string phase,
        int roundNumber)
    {
        // Map phase to WorkoutZone
        var zone = phase.ToLower() switch
        {
            "warmup" => WorkoutZone.Warmup,
            "workout" => WorkoutZone.Main,
            "cooldown" => WorkoutZone.Cooldown,
            _ => WorkoutZone.Main
        };

        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exercises = await repository.GetByWorkoutTemplateAsync(templateId);
        // For now, we're not tracking round numbers, just zones
        // This would need to be expanded if we implement round support
        return exercises?.Any(e => e.Zone == zone) ?? false;
    }

    public async Task<bool> TemplateHasExercisesAsync(WorkoutTemplateId templateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exercises = await repository.GetByWorkoutTemplateAsync(templateId);
        return exercises?.Any() ?? false;
    }
}