using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for MovementPattern reference data
/// TEMPORARY: Using IEmptyEnabledReferenceDataRepository until all entities are migrated
/// </summary>
public interface IMovementPatternRepository : IEmptyEnabledReferenceDataRepository<MovementPattern, MovementPatternId>
{
    // Add any MovementPattern-specific repository methods here if needed
}
