using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for DifficultyLevel reference data
/// TEMPORARY: Extends EmptyEnabledReferenceDataRepository until all entities are migrated
/// </summary>
public class DifficultyLevelRepository : 
    EmptyEnabledReferenceDataRepository<DifficultyLevel, DifficultyLevelId, FitnessDbContext>,
    IDifficultyLevelRepository
{
    // Add any DifficultyLevel-specific repository methods here if needed
}
