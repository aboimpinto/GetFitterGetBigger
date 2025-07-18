using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for exercise type operations
/// </summary>
public class ExerciseTypeService : IExerciseTypeService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;

    public ExerciseTypeService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(ExerciseTypeId id)
    {
        using var uow = _unitOfWorkProvider.CreateReadOnly();
        var repository = uow.GetRepository<IExerciseTypeRepository>();
        var exerciseType = await repository.GetByIdAsync(id);
        return exerciseType != null && exerciseType.IsActive;
    }

    /// <inheritdoc/>
    public async Task<bool> AllExistAsync(IEnumerable<string> ids)
    {
        using var uow = _unitOfWorkProvider.CreateReadOnly();
        var repository = uow.GetRepository<IExerciseTypeRepository>();
        
        foreach (var idString in ids)
        {
            if (ExerciseTypeId.TryParse(idString, out var id))
            {
                var exerciseType = await repository.GetByIdAsync(id);
                if (exerciseType == null || !exerciseType.IsActive)
                {
                    return false;
                }
            }
            else
            {
                // Invalid ID format
                return false;
            }
        }
        
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<bool> AnyIsRestTypeAsync(IEnumerable<string> ids)
    {
        using var uow = _unitOfWorkProvider.CreateReadOnly();
        var repository = uow.GetRepository<IExerciseTypeRepository>();
        
        foreach (var idString in ids)
        {
            if (ExerciseTypeId.TryParse(idString, out var id))
            {
                var exerciseType = await repository.GetByIdAsync(id);
                if (exerciseType != null && exerciseType.Value.ToLowerInvariant() == "rest")
                {
                    return true;
                }
            }
        }
        
        return false;
    }
}