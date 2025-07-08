using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for body part operations
/// </summary>
public interface IBodyPartService
{
    /// <summary>
    /// Checks if a body part exists
    /// </summary>
    /// <param name="id">The body part ID to check</param>
    /// <returns>True if the body part exists, false otherwise</returns>
    Task<bool> ExistsAsync(BodyPartId id);
}