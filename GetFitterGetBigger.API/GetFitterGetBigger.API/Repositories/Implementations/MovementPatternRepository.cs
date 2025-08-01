using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for MovementPattern reference data
/// TEMPORARY: Using EmptyEnabledReferenceDataRepository until all entities are migrated
/// </summary>
public class MovementPatternRepository : 
    EmptyEnabledReferenceDataRepository<MovementPattern, MovementPatternId, FitnessDbContext>,
    IMovementPatternRepository
{
    // Add any MovementPattern-specific repository methods here if needed
}
