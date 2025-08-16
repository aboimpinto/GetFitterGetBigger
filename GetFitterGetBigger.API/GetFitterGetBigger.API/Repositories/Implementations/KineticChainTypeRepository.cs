using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for KineticChainType reference data
/// </summary>
public class KineticChainTypeRepository : 
    ReferenceDataRepository<KineticChainType, KineticChainTypeId, FitnessDbContext>,
    IKineticChainTypeRepository
{
    // Add any KineticChainType-specific repository methods here if needed
}
