using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for DifficultyLevel reference data
/// TEMPORARY: Extends IEmptyEnabledReferenceDataRepository until all entities are migrated
/// </summary>
public interface IDifficultyLevelRepository : IEmptyEnabledReferenceDataRepository<DifficultyLevel, DifficultyLevelId>
{
    // Add any DifficultyLevel-specific repository methods here if needed
}
