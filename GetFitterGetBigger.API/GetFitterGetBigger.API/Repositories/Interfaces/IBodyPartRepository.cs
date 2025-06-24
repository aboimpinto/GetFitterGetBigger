using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for BodyPart reference data
/// </summary>
public interface IBodyPartRepository : IReferenceDataRepository<BodyPart, BodyPartId>
{
    // Add any BodyPart-specific repository methods here if needed
}
