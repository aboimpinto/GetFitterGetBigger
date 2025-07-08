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
    private readonly IExerciseTypeRepository _exerciseTypeRepository;

    public ExerciseTypeService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IExerciseTypeRepository exerciseTypeRepository)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _exerciseTypeRepository = exerciseTypeRepository;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(ExerciseTypeId id)
    {
        using var uow = _unitOfWorkProvider.CreateReadOnly();
        var exerciseType = await _exerciseTypeRepository.GetByIdAsync(id);
        return exerciseType != null;
    }

    /// <inheritdoc/>
    public async Task<bool> AllExistAsync(IEnumerable<ExerciseTypeId> ids)
    {
        using var uow = _unitOfWorkProvider.CreateReadOnly();
        
        foreach (var id in ids)
        {
            var exerciseType = await _exerciseTypeRepository.GetByIdAsync(id);
            if (exerciseType == null)
            {
                return false;
            }
        }
        
        return true;
    }
}