using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Implementations;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for MuscleRole reference data
/// </summary>
public class MuscleRoleRepository : 
    EmptyEnabledReferenceDataRepository<MuscleRole, MuscleRoleId, FitnessDbContext>,
    IMuscleRoleRepository
{
    // Add any MuscleRole-specific repository methods here if needed
}
