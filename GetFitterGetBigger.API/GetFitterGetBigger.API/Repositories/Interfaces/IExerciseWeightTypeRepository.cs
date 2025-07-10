using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for ExerciseWeightType reference data
/// </summary>
public interface IExerciseWeightTypeRepository : IReferenceDataRepository<ExerciseWeightType, ExerciseWeightTypeId>
{
    /// <summary>
    /// Gets an exercise weight type by its code
    /// </summary>
    /// <param name="code">The code of the weight type (e.g., "BODYWEIGHT_ONLY", "WEIGHT_REQUIRED")</param>
    /// <returns>The exercise weight type if found, null otherwise</returns>
    Task<ExerciseWeightType?> GetByCodeAsync(string code);
}