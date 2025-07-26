using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Services.Strategies;
using Microsoft.Extensions.Caching.Memory;

namespace GetFitterGetBigger.Admin.Services
{
    /// <summary>
    /// Reference data service using the Strategy Pattern.
    /// Provides a single generic method for all reference data types.
    /// </summary>
    public class ReferenceDataService : IGenericReferenceDataService
    {
        private readonly Dictionary<Type, IReferenceTableStrategy> _strategies;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ReferenceDataService> _logger;

        public ReferenceDataService(
            IEnumerable<IReferenceTableStrategy> strategies,
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<ReferenceDataService> logger)
        {
            _strategies = strategies.ToDictionary(s => s.EntityType);
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            
            _logger.LogInformation("ReferenceDataService initialized with {StrategyCount} strategies", 
                _strategies.Count);
        }

        /// <summary>
        /// THE ONLY METHOD! Gets reference data for any table type.
        /// </summary>
        public async Task<IEnumerable<ReferenceDataDto>> GetReferenceDataAsync<T>() 
            where T : IReferenceTableEntity
        {
            var entityType = typeof(T);
            IEnumerable<ReferenceDataDto> result = Enumerable.Empty<ReferenceDataDto>();
            
            if (!_strategies.TryGetValue(entityType, out var strategy))
            {
                _logger.LogError("No strategy registered for type {TypeName}", entityType.Name);
                return result;
            }
            
            // Check cache first
            if (_cache.TryGetValue(strategy.CacheKey, out IEnumerable<ReferenceDataDto>? cachedData))
            {
                _logger.LogInformation("Cache HIT for {CacheKey}", strategy.CacheKey);
                result = cachedData!; // TryGetValue guarantees non-null when returning true
            }
            else
            {
                // Fetch from API
                result = await FetchFromApiAsync(strategy);
            }
            
            return result; // Single exit point
        }
        
        /// <summary>
        /// Clears the cache for the specified reference table type.
        /// Uses the strategy pattern to determine the correct cache key.
        /// </summary>
        public void ClearCache<T>() where T : IReferenceTableEntity
        {
            var entityType = typeof(T);
            
            if (_strategies.TryGetValue(entityType, out var strategy))
            {
                _cache.Remove(strategy.CacheKey);
                _logger.LogInformation("Cleared cache for {TypeName} with key {CacheKey}", 
                    entityType.Name, strategy.CacheKey);
            }
            else
            {
                _logger.LogWarning("No strategy found for type {TypeName} when attempting to clear cache", 
                    entityType.Name);
            }
        }
        
        private async Task<IEnumerable<ReferenceDataDto>> FetchFromApiAsync(IReferenceTableStrategy strategy)
        {
            IEnumerable<ReferenceDataDto> result = Enumerable.Empty<ReferenceDataDto>();
            
            try
            {
                _logger.LogInformation("Cache MISS for {CacheKey}, fetching from {Endpoint}", 
                    strategy.CacheKey, strategy.Endpoint);
                
                var requestUrl = strategy.Endpoint.StartsWith("/") 
                    ? strategy.Endpoint.Substring(1) 
                    : strategy.Endpoint;
                
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    result = await strategy.TransformDataAsync(jsonContent);
                    
                    // Cache the result
                    _cache.Set(strategy.CacheKey, result, strategy.CacheDuration);
                    _logger.LogInformation("Cached {Count} items for {CacheKey}", 
                        result.Count(), strategy.CacheKey);
                }
                else
                {
                    _logger.LogError("API returned {StatusCode} for {Endpoint}", 
                        response.StatusCode, strategy.Endpoint);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data for {CacheKey}", strategy.CacheKey);
            }
            
            return result;
        }
    }
}