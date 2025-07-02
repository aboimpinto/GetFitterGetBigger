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
        private readonly IConfiguration _configuration;
        private readonly ICacheHelperService _cacheHelper;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string CacheKey = "EquipmentDto_Full";

        public EquipmentService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration, ICacheHelperService cacheHelper)
        {
            _httpClient = httpClient;
            _cache = cache;
            _configuration = configuration;
            _cacheHelper = cacheHelper;
            _apiBaseUrl = _configuration["ApiBaseUrl"] ?? string.Empty;
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
                var requestUrl = $"{_apiBaseUrl}/api/ReferenceTables/Equipment";
                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to get equipment: {response.StatusCode}");
                }

                // The API returns ReferenceDataDto, we need to convert to EquipmentDto
                var referenceData = await response.Content.ReadFromJsonAsync<IEnumerable<ReferenceDataDto>>(_jsonOptions);

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

                    _cache.Set(CacheKey, cachedData, TimeSpan.FromHours(24));
                }
            }

            return cachedData ?? Enumerable.Empty<EquipmentDto>();
        }

        public async Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentDto dto)
        {
            var requestUrl = $"{_apiBaseUrl}/api/ReferenceTables/Equipment";

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
            var requestUrl = $"{_apiBaseUrl}/api/ReferenceTables/Equipment/{id}";

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
            var requestUrl = $"{_apiBaseUrl}/api/ReferenceTables/Equipment/{id}";

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