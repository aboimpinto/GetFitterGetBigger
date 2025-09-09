using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Extensions;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;

public class CopyRoundHandler : ICopyRoundHandler
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<CopyRoundHandler> _logger;

    public CopyRoundHandler(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<CopyRoundHandler> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    public async Task<ServiceResult<CopyRoundResultDto>> CopyRoundAsync(
        WorkoutTemplateId templateId,
        CopyRoundDto dto)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();

        // Map phase to zone
        var sourceZone = dto.SourcePhase.ToLower() switch
        {
            "warmup" => WorkoutZone.Warmup,
            "workout" => WorkoutZone.Main,
            "cooldown" => WorkoutZone.Cooldown,
            _ => WorkoutZone.Main
        };

        var targetZone = dto.TargetPhase.ToLower() switch
        {
            "warmup" => WorkoutZone.Warmup,
            "workout" => WorkoutZone.Main,
            "cooldown" => WorkoutZone.Cooldown,
            _ => WorkoutZone.Main
        };

        // Get all exercises in the template
        var allExercises = await repository.GetByWorkoutTemplateAsync(templateId);
        
        // Get source exercises (in the specified zone)
        var sourceExercises = allExercises
            .Where(e => e.Zone == sourceZone)
            .OrderBy(e => e.SequenceOrder)
            .ToList();

        if (!sourceExercises.Any())
        {
            return ServiceResult<CopyRoundResultDto>.Failure(
                CopyRoundResultDto.Empty,
                ServiceError.NotFound($"No exercises found in {dto.SourcePhase} phase"));
        }

        // Get max sequence order in target zone
        var maxSequenceInTarget = allExercises
            .Where(e => e.Zone == targetZone)
            .Select(e => e.SequenceOrder)
            .DefaultIfEmpty(0)
            .Max();

        var copiedExercises = new List<WorkoutTemplateExercise>();
        var setConfigRepo = unitOfWork.GetRepository<ISetConfigurationRepository>();

        // Copy each exercise with new GUIDs
        foreach (var sourceExercise in sourceExercises)
        {
            maxSequenceInTarget++;
            var newExercise = WorkoutTemplateExercise.Handler.CreateNew(
                templateId,
                sourceExercise.ExerciseId,
                targetZone,
                maxSequenceInTarget,
                sourceExercise.Notes);

            if (newExercise.IsSuccess)
            {
                await repository.AddAsync(newExercise.Value);
                copiedExercises.Add(newExercise.Value);

                // Copy set configurations if they exist
                if (sourceExercise.Configurations.Any())
                {
                    foreach (var config in sourceExercise.Configurations)
                    {
                        var newConfig = GetFitterGetBigger.API.Models.Entities.SetConfiguration.Handler.CreateNew(
                            newExercise.Value.Id,
                            config.SetNumber,
                            config.TargetReps,
                            config.TargetWeight,
                            config.TargetTimeSeconds,
                            config.RestSeconds);

                        if (newConfig.IsSuccess)
                        {
                            await setConfigRepo.AddAsync(newConfig.Value);
                        }
                    }
                }
            }
        }

        await unitOfWork.CommitAsync();

        var copiedExerciseDtos = copiedExercises.Select(e => e.ToDto()).ToList();
        var resultDto = new CopyRoundResultDto(
            copiedExerciseDtos,
            $"Successfully copied {copiedExercises.Count} exercises to {dto.TargetPhase}");

        return ServiceResult<CopyRoundResultDto>.Success(resultDto);
    }
}