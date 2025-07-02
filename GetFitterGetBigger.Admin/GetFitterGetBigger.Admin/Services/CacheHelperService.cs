using Microsoft.Extensions.Caching.Memory;

namespace GetFitterGetBigger.Admin.Services
{
    public interface ICacheHelperService
    {
        void ClearAllCaches();
        void ClearReferenceDataCaches();
    }

    public class CacheHelperService : ICacheHelperService
    {
        private readonly IMemoryCache _cache;

        // List of all cache keys used in the application
        private readonly string[] _referenceDataCacheKeys = new[]
        {
            // ReferenceDataService keys
            "RefData_BodyParts",
            "RefData_DifficultyLevels",
            "RefData_Equipment",
            "RefData_KineticChainTypes",
            "RefData_MetricTypes",
            "RefData_MovementPatterns",
            "RefData_MuscleGroups",
            "RefData_MuscleRoles",
            "RefData_ExerciseTypes",
            
            // Legacy keys that might still exist
            "BodyParts",
            "DifficultyLevels",
            "Equipment",
            "KineticChainTypes",
            "MetricTypes",
            "MovementPatterns",
            "MuscleGroups",
            "MuscleRoles",
            "ExerciseTypes",
            
            // Service-specific keys
            "MuscleGroupsDto_Full",
            "EquipmentDto_Full"
        };

        public CacheHelperService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void ClearAllCaches()
        {
            // Clear all known cache keys
            ClearReferenceDataCaches();
        }

        public void ClearReferenceDataCaches()
        {
            foreach (var key in _referenceDataCacheKeys)
            {
                _cache.Remove(key);
            }
        }
    }
}