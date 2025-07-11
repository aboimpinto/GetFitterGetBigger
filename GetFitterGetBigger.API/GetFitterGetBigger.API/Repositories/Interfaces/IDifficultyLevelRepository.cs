using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for DifficultyLevel reference data
/// </summary>
public interface IDifficultyLevelRepository : IReferenceDataRepository<DifficultyLevel, DifficultyLevelId>
{
    // Add any DifficultyLevel-specific repository methods here if needed
}
