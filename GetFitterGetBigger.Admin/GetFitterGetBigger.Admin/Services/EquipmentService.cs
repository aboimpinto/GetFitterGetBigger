using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GetFitterGetBigger.Admin.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ICacheHelperService _cacheHelper;
        private readonly ILogger<EquipmentService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string CacheKey = "EquipmentDto_Full";

        public EquipmentService(
            HttpClient httpClient, 
            IMemoryCache cache, 
            ICacheHelperService cacheHelper,
            ILogger<EquipmentService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _cacheHelper = cacheHelper;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<IEnumerable<EquipmentDto>> GetEquipmentAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out IEnumerable<EquipmentDto>? cachedData))
            {
                var requestUrl = "api/ReferenceTables/Equipment";
                _logger.LogInformation("Fetching equipment from: {Url}", requestUrl);
                
                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to get equipment: {response.StatusCode}");
                }

                // Try to deserialize the response - the API might return either format
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Equipment API response: {Response}", responseContent);
                
                try
                {
                    // First try as EquipmentDto array (new format)
                    _logger.LogDebug("Attempting to deserialize as EquipmentDto array");
                    cachedData = JsonSerializer.Deserialize<IEnumerable<EquipmentDto>>(responseContent, _jsonOptions);
                    _logger.LogInformation("Successfully deserialized {Count} equipment items as EquipmentDto", cachedData?.Count() ?? 0);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize as EquipmentDto, trying ReferenceDataDto format");
                    
                    // If that fails, try as ReferenceDataDto array (old format)
                    var referenceData = JsonSerializer.Deserialize<IEnumerable<ReferenceDataDto>>(responseContent, _jsonOptions);
                    
                    if (referenceData != null)
                    {
                        cachedData = referenceData.Select(rd => new EquipmentDto
                        {
                            Id = rd.Id,
                            Name = rd.Value, // Map 'value' to 'name'
                            IsActive = true, // Reference data is always active
                            CreatedAt = DateTime.UtcNow, // Default since not provided by API
                            UpdatedAt = null
                        }).ToList();
                        _logger.LogInformation("Successfully converted {Count} ReferenceDataDto items to EquipmentDto", cachedData.Count());
                    }
                }

                if (cachedData != null)
                {
                    _cache.Set(CacheKey, cachedData, TimeSpan.FromHours(24));
                }
            }

            return cachedData ?? Enumerable.Empty<EquipmentDto>();
        }

        public async Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentDto dto)
        {
            var requestUrl = "api/ReferenceTables/Equipment";

            // Log the request details
            Console.WriteLine($"[CREATE EQUIPMENT] URL: {requestUrl}");
            Console.WriteLine($"[CREATE EQUIPMENT] DTO: {JsonSerializer.Serialize(dto, _jsonOptions)}");

            var response = await _httpClient.PostAsJsonAsync(requestUrl, dto, _jsonOptions);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new InvalidOperationException("Equipment with this name already exists");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[CREATE EQUIPMENT] Error Response: {errorContent}");
                throw new HttpRequestException($"Failed to create equipment: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[CREATE EQUIPMENT] Success Response: {responseContent}");

            // Try to deserialize as ReferenceDataDto first since that's what the API returns
            EquipmentDto created;
            try
            {
                var referenceData = JsonSerializer.Deserialize<ReferenceDataDto>(responseContent, _jsonOptions);
                created = new EquipmentDto
                {
                    Id = referenceData!.Id,
                    Name = referenceData.Value,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };
            }
            catch
            {
                // If that fails, try as EquipmentDto
                created = JsonSerializer.Deserialize<EquipmentDto>(responseContent, _jsonOptions)!;
            }

            // Invalidate caches - both our own and ReferenceDataService's
            _cache.Remove(CacheKey);
            _cache.Remove("RefData_Equipment");
            _cache.Remove("Equipment"); // Legacy key
            Console.WriteLine($"[EquipmentService] Cache invalidated: {CacheKey}, RefData_Equipment, Equipment");
            Console.WriteLine($"[EquipmentService] Cache invalidated: {CacheKey}, RefData_Equipment, Equipment");

            return created ?? throw new InvalidOperationException("Failed to deserialize created equipment");
        }

        public async Task<EquipmentDto> UpdateEquipmentAsync(string id, UpdateEquipmentDto dto)
        {
            var requestUrl = $"api/ReferenceTables/Equipment/{id}";

            // Log the request details
            Console.WriteLine($"[UPDATE EQUIPMENT] URL: {requestUrl}");
            Console.WriteLine($"[UPDATE EQUIPMENT] ID: {id}");
            Console.WriteLine($"[UPDATE EQUIPMENT] DTO: {JsonSerializer.Serialize(dto, _jsonOptions)}");

            var response = await _httpClient.PutAsJsonAsync(requestUrl, dto, _jsonOptions);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Equipment not found");
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new InvalidOperationException("Equipment with this name already exists");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[UPDATE EQUIPMENT] Error Response: {errorContent}");
                throw new HttpRequestException($"Failed to update equipment: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[UPDATE EQUIPMENT] Success Response: {responseContent}");

            // Try to deserialize as ReferenceDataDto first since that's what the API returns
            EquipmentDto updated;
            try
            {
                var referenceData = JsonSerializer.Deserialize<ReferenceDataDto>(responseContent, _jsonOptions);
                updated = new EquipmentDto
                {
                    Id = referenceData!.Id,
                    Name = referenceData.Value,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            catch
            {
                // If that fails, try as EquipmentDto
                updated = JsonSerializer.Deserialize<EquipmentDto>(responseContent, _jsonOptions)!;
            }

            // Invalidate caches - both our own and ReferenceDataService's
            _cache.Remove(CacheKey);
            _cache.Remove("RefData_Equipment");
            _cache.Remove("Equipment"); // Legacy key
            Console.WriteLine($"[EquipmentService] Cache invalidated: {CacheKey}, RefData_Equipment, Equipment");

            return updated ?? throw new InvalidOperationException("Failed to deserialize updated equipment");
        }

        public async Task DeleteEquipmentAsync(string id)
        {
            var requestUrl = $"api/ReferenceTables/Equipment/{id}";

            // Log the request details
            Console.WriteLine($"[DELETE EQUIPMENT] URL: {requestUrl}");
            Console.WriteLine($"[DELETE EQUIPMENT] ID: {id}");

            var response = await _httpClient.DeleteAsync(requestUrl);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Equipment not found");
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new InvalidOperationException("Cannot delete equipment that is in use by exercises");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DELETE EQUIPMENT] Error Response: {errorContent}");
                throw new HttpRequestException($"Failed to delete equipment: {response.StatusCode}");
            }

            Console.WriteLine($"[DELETE EQUIPMENT] Success: Equipment {id} deleted");

            // Invalidate caches - both our own and ReferenceDataService's
            _cache.Remove(CacheKey);
            _cache.Remove("RefData_Equipment");
            _cache.Remove("Equipment"); // Legacy key
            Console.WriteLine($"[EquipmentService] Cache invalidated: {CacheKey}, RefData_Equipment, Equipment");
        }
    }
}