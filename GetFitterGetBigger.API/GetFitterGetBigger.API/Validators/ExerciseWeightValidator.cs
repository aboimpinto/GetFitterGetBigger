using System;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Validators;

/// <summary>
/// Validates exercise weight based on the exercise's weight type
/// </summary>
public class ExerciseWeightValidator : IExerciseWeightValidator
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    
    public ExerciseWeightValidator(IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }
    
    /// <summary>
    /// Validates whether the provided weight is valid for the given exercise
    /// </summary>
    public async Task<WeightValidationResult> ValidateWeightAsync(ExerciseId exerciseId, decimal? weightKg)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var exerciseRepository = unitOfWork.GetRepository<IExerciseRepository>();
        
        var exercise = await exerciseRepository.GetByIdAsync(exerciseId);
        if (exercise.IsEmpty)
        {
            return WeightValidationResult.Failure($"Exercise with ID {exerciseId} not found");
        }
        
        if (exercise.ExerciseWeightTypeId == null)
        {
            // If no weight type is specified, allow any weight (legacy support)
            return WeightValidationResult.Success();
        }
        
        return await ValidateWeightByTypeAsync(exercise.ExerciseWeightTypeId.Value, weightKg);
    }
    
    /// <summary>
    /// Validates whether the provided weight is valid for the given exercise weight type
    /// </summary>
    public async Task<WeightValidationResult> ValidateWeightByTypeAsync(ExerciseWeightTypeId exerciseWeightTypeId, decimal? weightKg)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var weightTypeRepository = unitOfWork.GetRepository<IExerciseWeightTypeRepository>();
        
        var weightType = await weightTypeRepository.GetByIdAsync(exerciseWeightTypeId);
        if (weightType == null || weightType.IsEmpty)
        {
            return WeightValidationResult.Failure($"Exercise weight type with ID {exerciseWeightTypeId} not found");
        }
        
        return ValidateWeightByCode(weightType.Code, weightKg);
    }
    
    private WeightValidationResult ValidateWeightByCode(string weightTypeCode, decimal? weightKg)
    {
        return weightTypeCode switch
        {
            "BODYWEIGHT_ONLY" => ValidateBodyweightOnly(weightKg),
            "BODYWEIGHT_OPTIONAL" => ValidateBodyweightOptional(weightKg),
            "WEIGHT_REQUIRED" => ValidateWeightRequired(weightKg),
            "MACHINE_WEIGHT" => ValidateMachineWeight(weightKg),
            "NO_WEIGHT" => ValidateNoWeight(weightKg),
            _ => WeightValidationResult.Failure($"Unknown weight type code: {weightTypeCode}")
        };
    }
    
    private WeightValidationResult ValidateBodyweightOnly(decimal? weightKg)
    {
        if (weightKg.HasValue && weightKg.Value != 0)
        {
            return WeightValidationResult.Failure("Bodyweight-only exercises cannot have external weight specified. Weight must be null or 0.");
        }
        return WeightValidationResult.Success();
    }
    
    private WeightValidationResult ValidateBodyweightOptional(decimal? weightKg)
    {
        if (weightKg.HasValue && weightKg.Value < 0)
        {
            return WeightValidationResult.Failure("Weight cannot be negative.");
        }
        return WeightValidationResult.Success();
    }
    
    private WeightValidationResult ValidateWeightRequired(decimal? weightKg)
    {
        if (!weightKg.HasValue || weightKg.Value <= 0)
        {
            return WeightValidationResult.Failure("This exercise requires external weight to be specified. Weight must be greater than 0.");
        }
        return WeightValidationResult.Success();
    }
    
    private WeightValidationResult ValidateMachineWeight(decimal? weightKg)
    {
        if (!weightKg.HasValue || weightKg.Value <= 0)
        {
            return WeightValidationResult.Failure("Machine exercises require weight to be specified. Weight must be greater than 0.");
        }
        return WeightValidationResult.Success();
    }
    
    private WeightValidationResult ValidateNoWeight(decimal? weightKg)
    {
        if (weightKg.HasValue && weightKg.Value != 0)
        {
            return WeightValidationResult.Failure("This exercise type does not use weight. Weight must be null or 0.");
        }
        return WeightValidationResult.Success();
    }
}