using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for BodyPart reference data
/// TEMPORARY: Using IEmptyEnabledReferenceDataRepository until all entities are migrated
/// </summary>
public interface IBodyPartRepository : IEmptyEnabledReferenceDataRepository<BodyPart, BodyPartId>
{
    // Add any BodyPart-specific repository methods here if needed
}
