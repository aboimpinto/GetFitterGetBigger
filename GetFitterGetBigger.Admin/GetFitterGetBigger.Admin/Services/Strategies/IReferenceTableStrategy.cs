using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services.Strategies
{
    public interface IReferenceTableStrategy
    {
        Type EntityType { get; }
        string Endpoint { get; }
        string CacheKey { get; }
        TimeSpan CacheDuration { get; }
        
        Task<IEnumerable<ReferenceDataDto>> TransformDataAsync(string jsonResponse);
    }
}