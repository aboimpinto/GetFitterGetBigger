using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for BodyPart reference data
/// </summary>
public class BodyPartRepository : 
    ReferenceDataRepository<BodyPart, BodyPartId, FitnessDbContext>,
    IBodyPartRepository
{
    // Add any BodyPart-specific repository methods here if needed
}
