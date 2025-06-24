using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for MuscleRole reference data
/// </summary>
public interface IMuscleRoleRepository : IReferenceDataRepository<MuscleRole, MuscleRoleId>
{
    // Add any MuscleRole-specific repository methods here if needed
}
