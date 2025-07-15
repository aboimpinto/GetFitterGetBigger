using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for MuscleRole reference data
/// TEMPORARY: Using IEmptyEnabledReferenceDataRepository until all entities are migrated
/// </summary>
public interface IMuscleRoleRepository : IEmptyEnabledReferenceDataRepository<MuscleRole, MuscleRoleId>
{
    // Add any MuscleRole-specific repository methods here if needed
}
