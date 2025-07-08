using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for body part operations
/// </summary>
public class BodyPartService : IBodyPartService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;

    public BodyPartService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(BodyPartId id)
    {
        using var uow = _unitOfWorkProvider.CreateReadOnly();
        var repository = uow.GetRepository<IBodyPartRepository>();
        var bodyPart = await repository.GetByIdAsync(id);
        return bodyPart != null && bodyPart.IsActive;
    }
}