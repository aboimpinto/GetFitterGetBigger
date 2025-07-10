using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for ExerciseWeightType reference data
/// </summary>
public class ExerciseWeightTypeRepository : 
    ReferenceDataRepository<ExerciseWeightType, ExerciseWeightTypeId, FitnessDbContext>,
    IExerciseWeightTypeRepository
{
    /// <summary>
    /// Gets an exercise weight type by its code
    /// </summary>
    /// <param name="code">The code of the weight type (e.g., "BODYWEIGHT_ONLY", "WEIGHT_REQUIRED")</param>
    /// <returns>The exercise weight type if found, null otherwise</returns>
    public async Task<ExerciseWeightType?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return null;
        }
        
        return await Context.Set<ExerciseWeightType>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code && x.IsActive);
    }
}