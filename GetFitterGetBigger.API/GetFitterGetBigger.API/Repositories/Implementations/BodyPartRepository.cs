using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for BodyPart reference data
/// TEMPORARY: Using EmptyEnabledReferenceDataRepository until all entities are migrated
/// </summary>
public class BodyPartRepository : 
    EmptyEnabledReferenceDataRepository<BodyPart, BodyPartId, FitnessDbContext>,
    IBodyPartRepository
{
    // Add any BodyPart-specific repository methods here if needed
}
