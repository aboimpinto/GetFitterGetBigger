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
    private readonly IBodyPartRepository _bodyPartRepository;

    public BodyPartService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IBodyPartRepository bodyPartRepository)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _bodyPartRepository = bodyPartRepository;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(BodyPartId id)
    {
        using var uow = _unitOfWorkProvider.CreateReadOnly();
        var bodyPart = await _bodyPartRepository.GetByIdAsync(id);
        return bodyPart != null;
    }
}