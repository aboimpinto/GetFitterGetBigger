using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;

namespace GetFitterGetBigger.Admin.Services
{
    /// <summary>
    /// Modern generic interface for reference data access.
    /// Replaces the need for specific methods per reference table.
    /// </summary>
    public interface IGenericReferenceDataService
    {
        /// <summary>
        /// Gets reference data for the specified type.
        /// </summary>
        /// <typeparam name="T">The reference table type (e.g., BodyParts, Equipment)</typeparam>
        /// <returns>Collection of reference data items</returns>
        Task<IEnumerable<ReferenceDataDto>> GetReferenceDataAsync<T>() 
            where T : IReferenceTableEntity;
            
        /// <summary>
        /// Clears the cache for the specified reference table type.
        /// </summary>
        /// <typeparam name="T">The reference table type to clear cache for</typeparam>
        void ClearCache<T>() where T : IReferenceTableEntity;
    }
}