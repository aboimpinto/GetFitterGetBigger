using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using System.Text.Json;

namespace GetFitterGetBigger.Admin.Services.Strategies
{
    public abstract class BaseReferenceTableStrategy<T> : IReferenceTableStrategy 
        where T : IReferenceTableEntity
    {
        private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromHours(24);
        
        public Type EntityType => typeof(T);
        public abstract string Endpoint { get; }
        public abstract string CacheKey { get; }
        public virtual TimeSpan CacheDuration => DefaultCacheDuration;
        
        protected static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        /// <summary>
        /// Transforms JSON response into reference data DTOs.
        /// </summary>
        /// <param name="jsonResponse">The JSON response from the API.</param>
        /// <returns>A task containing the transformed reference data.</returns>
        /// <remarks>
        /// This method is synchronous but returns Task for interface compatibility.
        /// The async signature allows derived classes to implement truly asynchronous transformations
        /// if needed (e.g., additional API calls, database lookups).
        /// </remarks>
        public virtual Task<IEnumerable<ReferenceDataDto>> TransformDataAsync(string jsonResponse)
        {
            // Default implementation for standard reference data
            var flexibleData = JsonSerializer.Deserialize<IEnumerable<FlexibleReferenceDataDto>>(
                jsonResponse, 
                JsonOptions);
                
            var result = flexibleData?.Select(fd => fd.ToReferenceDataDto()) 
                ?? Enumerable.Empty<ReferenceDataDto>();
                
            return Task.FromResult(result);
        }
    }
}