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
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string CacheKey = "Equipment";

        public EquipmentService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _configuration = configuration;
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
            var response = await _httpClient.PostAsJsonAsync(requestUrl, dto, _jsonOptions);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new InvalidOperationException("Equipment with this name already exists");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to create equipment: {response.StatusCode}");
            }

            var created = await response.Content.ReadFromJsonAsync<EquipmentDto>(_jsonOptions);
            
            // Invalidate cache
            _cache.Remove(CacheKey);
            
            return created ?? throw new InvalidOperationException("Failed to deserialize created equipment");
        }

        public async Task<EquipmentDto> UpdateEquipmentAsync(string id, UpdateEquipmentDto dto)
        {
            var requestUrl = $"{_apiBaseUrl}/api/ReferenceTables/Equipment/{id}";
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
                throw new HttpRequestException($"Failed to update equipment: {response.StatusCode}");
            }

            var updated = await response.Content.ReadFromJsonAsync<EquipmentDto>(_jsonOptions);
            
            // Invalidate cache
            _cache.Remove(CacheKey);
            
            return updated ?? throw new InvalidOperationException("Failed to deserialize updated equipment");
        }

        public async Task DeleteEquipmentAsync(string id)
        {
            var requestUrl = $"{_apiBaseUrl}/api/ReferenceTables/Equipment/{id}";
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
                throw new HttpRequestException($"Failed to delete equipment: {response.StatusCode}");
            }

            // Invalidate cache
            _cache.Remove(CacheKey);
        }
    }
}