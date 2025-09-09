using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Extensions;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;

public class ReorderExerciseHandler : IReorderExerciseHandler
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<ReorderExerciseHandler> _logger;

    public ReorderExerciseHandler(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<ReorderExerciseHandler> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    public async Task<ServiceResult<ReorderResultDto>> ReorderExerciseAsync(
        WorkoutTemplateId templateId,
        WorkoutTemplateExerciseId exerciseId,
        int newOrderInRound)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();

        var exercise = await repository.GetByIdWithDetailsAsync(exerciseId);
        if (exercise == null || exercise.IsEmpty)
        {
            return ServiceResult<ReorderResultDto>.Failure(
                ReorderResultDto.Empty,
                ServiceError.NotFound("Template exercise not found"));
        }

        // Get all exercises in the same zone
        var allExercises = await repository.GetByWorkoutTemplateAsync(templateId);
        var exercisesInZone = allExercises
            .Where(e => e.Zone == exercise.Zone)
            .OrderBy(e => e.SequenceOrder)
            .ToList();

        // Validate new order is within bounds
        if (newOrderInRound < 1 || newOrderInRound > exercisesInZone.Count)
        {
            return ServiceResult<ReorderResultDto>.Failure(
                ReorderResultDto.Empty,
                ServiceError.ValidationFailed("Invalid order position"));
        }

        // Reorder exercises
        var affectedExercises = new List<WorkoutTemplateExercise>();
        var currentOrder = exercise.SequenceOrder;
        var currentIndex = exercisesInZone.FindIndex(e => e.Id == exerciseId);

        if (newOrderInRound < currentOrder)
        {
            // Moving up - shift others down
            for (int i = newOrderInRound - 1; i < currentIndex; i++)
            {
                var other = exercisesInZone[i];
                var updated = WorkoutTemplateExercise.Handler.UpdateSequenceOrder(
                    other, 
                    other.SequenceOrder + 1);
                if (updated.IsSuccess)
                {
                    await repository.UpdateAsync(updated.Value);
                    affectedExercises.Add(updated.Value);
                }
            }
        }
        else if (newOrderInRound > currentOrder)
        {
            // Moving down - shift others up
            for (int i = currentIndex + 1; i < newOrderInRound; i++)
            {
                var other = exercisesInZone[i];
                var updated = WorkoutTemplateExercise.Handler.UpdateSequenceOrder(
                    other, 
                    other.SequenceOrder - 1);
                if (updated.IsSuccess)
                {
                    await repository.UpdateAsync(updated.Value);
                    affectedExercises.Add(updated.Value);
                }
            }
        }

        // Update the target exercise
        var targetUpdated = WorkoutTemplateExercise.Handler.UpdateSequenceOrder(
            exercise, 
            newOrderInRound);
        if (targetUpdated.IsSuccess)
        {
            await repository.UpdateAsync(targetUpdated.Value);
            affectedExercises.Add(targetUpdated.Value);
        }

        await unitOfWork.CommitAsync();

        var affectedExerciseDtos = affectedExercises.Select(e => e.ToDto()).ToList();
        var resultDto = new ReorderResultDto(
            affectedExerciseDtos,
            $"Exercise reordered to position {newOrderInRound}");

        return ServiceResult<ReorderResultDto>.Success(resultDto);
    }

    public async Task ReorderAfterRemovalAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateId templateId,
        List<WorkoutTemplateExercise> removedExercises)
    {
        foreach (var removed in removedExercises)
        {
            // Get all exercises in the template
            var allExercises = await repository.GetByWorkoutTemplateAsync(templateId);
            
            // Get remaining exercises in the same zone after the removed one
            var remainingInZone = allExercises
                .Where(e => e.Zone == removed.Zone && e.SequenceOrder > removed.SequenceOrder)
                .OrderBy(e => e.SequenceOrder)
                .ToList();

            // Reorder exercises after the removed one
            foreach (var exercise in remainingInZone)
            {
                var updated = WorkoutTemplateExercise.Handler.UpdateSequenceOrder(
                    exercise, 
                    exercise.SequenceOrder - 1);
                if (updated.IsSuccess)
                {
                    await repository.UpdateAsync(updated.Value);
                }
            }
        }
    }
}