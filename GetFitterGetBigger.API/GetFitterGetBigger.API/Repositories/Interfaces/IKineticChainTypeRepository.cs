using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for KineticChainType reference data
/// </summary>
public interface IKineticChainTypeRepository : IEmptyEnabledReferenceDataRepository<KineticChainType, KineticChainTypeId>
{
    // Add any KineticChainType-specific repository methods here if needed
}
