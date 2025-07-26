using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Services;

namespace GetFitterGetBigger.Admin.Tests.TestHelpers
{
    /// <summary>
    /// Mock implementation of IGenericReferenceDataService for testing.
    /// Provides default empty data for all reference table types.
    /// </summary>
    public class MockGenericReferenceDataService : IGenericReferenceDataService
    {
        private readonly Dictionary<Type, IEnumerable<ReferenceDataDto>> _data = new();
        private readonly HashSet<Type> _clearedCaches = new();
        
        public Action<Type>? OnCacheCleared { get; set; }
        
        public MockGenericReferenceDataService()
        {
            // Initialize with empty data by default
            _data[typeof(DifficultyLevels)] = new List<ReferenceDataDto>();
            _data[typeof(MuscleGroups)] = new List<ReferenceDataDto>();
            _data[typeof(MuscleRoles)] = new List<ReferenceDataDto>();
            _data[typeof(Equipment)] = new List<ReferenceDataDto>();
            _data[typeof(BodyParts)] = new List<ReferenceDataDto>();
            _data[typeof(MovementPatterns)] = new List<ReferenceDataDto>();
            _data[typeof(ExerciseTypes)] = new List<ReferenceDataDto>();
            _data[typeof(KineticChainTypes)] = new List<ReferenceDataDto>();
            _data[typeof(MetricTypes)] = new List<ReferenceDataDto>();
        }
        
        public Task<IEnumerable<ReferenceDataDto>> GetReferenceDataAsync<T>() 
            where T : IReferenceTableEntity
        {
            var type = typeof(T);
            IEnumerable<ReferenceDataDto> result = new List<ReferenceDataDto>();
            
            if (_data.TryGetValue(type, out var data))
            {
                result = data;
            }
            
            return Task.FromResult(result);
        }
        
        public void ClearCache<T>() where T : IReferenceTableEntity
        {
            var type = typeof(T);
            _clearedCaches.Add(type);
            OnCacheCleared?.Invoke(type);
        }
        
        public bool WasCacheCleared<T>() where T : IReferenceTableEntity
        {
            return _clearedCaches.Contains(typeof(T));
        }
        
        public void SetData<T>(IEnumerable<ReferenceDataDto> data) where T : IReferenceTableEntity
        {
            _data[typeof(T)] = data;
        }
    }
}