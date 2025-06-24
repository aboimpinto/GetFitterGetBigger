using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for DifficultyLevel reference data
/// </summary>
public class DifficultyLevelRepository : 
    ReferenceDataRepository<DifficultyLevel, DifficultyLevelId, FitnessDbContext>,
    IDifficultyLevelRepository
{
    // Add any DifficultyLevel-specific repository methods here if needed
}
