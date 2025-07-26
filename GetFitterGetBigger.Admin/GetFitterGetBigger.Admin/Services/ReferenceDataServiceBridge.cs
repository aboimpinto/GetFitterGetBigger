using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using Microsoft.Extensions.Caching.Memory;

namespace GetFitterGetBigger.Admin.Services
{
    /// <summary>
    /// Bridge implementation that provides backward compatibility.
    /// Implements old interface using the new generic service.
    /// </summary>
    public class ReferenceDataServiceBridge : IReferenceDataService
    {
        private readonly IGenericReferenceDataService _genericService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ReferenceDataServiceBridge> _logger;

        public ReferenceDataServiceBridge(
            IGenericReferenceDataService genericService,
            IMemoryCache cache,
            ILogger<ReferenceDataServiceBridge> logger)
        {
            _genericService = genericService;
            _cache = cache;
            _logger = logger;
        }

        // Bridge methods - just delegate to generic service
        public Task<IEnumerable<ReferenceDataDto>> GetBodyPartsAsync() 
            => _genericService.GetReferenceDataAsync<BodyParts>();

        public Task<IEnumerable<ReferenceDataDto>> GetDifficultyLevelsAsync() 
            => _genericService.GetReferenceDataAsync<DifficultyLevels>();

        public Task<IEnumerable<ReferenceDataDto>> GetEquipmentAsync() 
            => _genericService.GetReferenceDataAsync<Equipment>();

        public Task<IEnumerable<ReferenceDataDto>> GetKineticChainTypesAsync() 
            => _genericService.GetReferenceDataAsync<KineticChainTypes>();

        public Task<IEnumerable<ReferenceDataDto>> GetMetricTypesAsync() 
            => _genericService.GetReferenceDataAsync<MetricTypes>();

        public Task<IEnumerable<ReferenceDataDto>> GetMovementPatternsAsync() 
            => _genericService.GetReferenceDataAsync<MovementPatterns>();

        public Task<IEnumerable<ReferenceDataDto>> GetMuscleGroupsAsync() 
            => _genericService.GetReferenceDataAsync<MuscleGroups>();

        public Task<IEnumerable<ReferenceDataDto>> GetMuscleRolesAsync() 
            => _genericService.GetReferenceDataAsync<MuscleRoles>();

        public async Task<IEnumerable<ExerciseTypeDto>> GetExerciseTypesAsync()
        {
            var referenceData = await _genericService.GetReferenceDataAsync<ExerciseTypes>();
            
            // Convert ReferenceDataDto to ExerciseTypeDto
            return referenceData.Select(rd => new ExerciseTypeDto
            {
                Id = rd.Id,
                Value = rd.Value,
                Description = rd.Description ?? string.Empty
            });
        }

        // Cache management
        public void ClearEquipmentCache()
        {
            _cache.Remove("RefData_Equipment");
            _logger.LogInformation("Cleared cache: RefData_Equipment");
        }

        public void ClearMuscleGroupsCache()
        {
            _cache.Remove("RefData_MuscleGroups");
            _cache.Remove("MuscleGroupsDto_Full");
            _cache.Remove("MuscleGroups");
            _logger.LogInformation("Cleared muscle groups related caches");
        }
    }
}