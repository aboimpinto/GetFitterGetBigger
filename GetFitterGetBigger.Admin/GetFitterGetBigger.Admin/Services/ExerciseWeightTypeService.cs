using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;
using System.Text.Json;

namespace GetFitterGetBigger.Admin.Services
{
    public class ExerciseWeightTypeService : IExerciseWeightTypeService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly JsonSerializerOptions _jsonOptions;

        public ExerciseWeightTypeService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<IEnumerable<ExerciseWeightTypeDto>> GetWeightTypesAsync()
        {
            const string cacheKey = "exercise_weight_types";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<ExerciseWeightTypeDto>? cachedWeightTypes))
            {
                return cachedWeightTypes ?? Enumerable.Empty<ExerciseWeightTypeDto>();
            }

            try
            {
                var response = await _httpClient.GetAsync("api/ReferenceTables/ExerciseWeightTypes");

                if (response.IsSuccessStatusCode)
                {
                    var referenceData = await response.Content.ReadFromJsonAsync<IEnumerable<ReferenceDataDto>>(_jsonOptions)
                        ?? Enumerable.Empty<ReferenceDataDto>();

                    // Map ReferenceDataDto to ExerciseWeightTypeDto
                    var weightTypes = MapToExerciseWeightTypes(referenceData);

                    // Cache for 30 minutes since this is reference data
                    _cache.Set(cacheKey, weightTypes, TimeSpan.FromMinutes(30));

                    return weightTypes;
                }

                response.EnsureSuccessStatusCode();
                return Enumerable.Empty<ExerciseWeightTypeDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Failed to retrieve exercise weight types: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving exercise weight types: {ex.Message}", ex);
            }
        }

        public async Task<ExerciseWeightTypeDto?> GetWeightTypeByIdAsync(Guid id)
        {
            try
            {
                // Since the API uses string IDs, we need to convert the GUID to the expected format
                var stringId = $"exerciseweighttype-{id}";
                var response = await _httpClient.GetAsync($"api/ReferenceTables/ExerciseWeightTypes/{stringId}");

                if (response.IsSuccessStatusCode)
                {
                    var referenceData = await response.Content.ReadFromJsonAsync<ReferenceDataDto>(_jsonOptions);
                    if (referenceData != null)
                    {
                        return MapToExerciseWeightType(referenceData);
                    }
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();
                return null;
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Failed to retrieve exercise weight type with ID {id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving exercise weight type with ID {id}: {ex.Message}", ex);
            }
        }

        private IEnumerable<ExerciseWeightTypeDto> MapToExerciseWeightTypes(IEnumerable<ReferenceDataDto> referenceData)
        {
            var weightTypes = new List<ExerciseWeightTypeDto>();
            var displayOrder = 1;

            foreach (var item in referenceData)
            {
                var weightType = MapToExerciseWeightType(item, displayOrder++);
                if (weightType != null)
                {
                    weightTypes.Add(weightType);
                }
            }

            return weightTypes;
        }

        private ExerciseWeightTypeDto? MapToExerciseWeightType(ReferenceDataDto referenceData, int displayOrder = 1)
        {
            // Extract the GUID from the ID string (format: "exerciseweighttype-{guid}")
            if (!TryExtractGuid(referenceData.Id, out var guid))
            {
                return null;
            }

            // Derive the code from the value
            var code = DeriveCodeFromValue(referenceData.Value);

            return new ExerciseWeightTypeDto
            {
                Id = guid,
                Code = code,
                Name = referenceData.Value,
                Description = referenceData.Description,
                IsActive = true,
                DisplayOrder = displayOrder
            };
        }

        private bool TryExtractGuid(string id, out Guid guid)
        {
            guid = Guid.Empty;
            if (string.IsNullOrEmpty(id))
                return false;

            // Expected format: "exerciseweighttype-{guid}"
            var parts = id.Split('-');
            if (parts.Length >= 6) // exerciseweighttype-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a
            {
                var guidString = string.Join("-", parts.Skip(1));
                return Guid.TryParse(guidString, out guid);
            }

            return false;
        }

        private string DeriveCodeFromValue(string value)
        {
            // Map the display values to their expected codes
            return value switch
            {
                "Bodyweight Only" => "BODYWEIGHT_ONLY",
                "Bodyweight Optional" => "BODYWEIGHT_OPTIONAL",
                "Weight Required" => "WEIGHT_REQUIRED",
                "Machine Weight" => "MACHINE_WEIGHT",
                "No Weight" => "NO_WEIGHT",
                _ => value.ToUpperInvariant().Replace(" ", "_")
            };
        }
    }
}