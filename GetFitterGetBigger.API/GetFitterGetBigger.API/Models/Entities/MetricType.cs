using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record MetricType
{
    public MetricTypeId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    
    // Navigation properties
    public ICollection<ExerciseMetricSupport> Exercises { get; init; } = new List<ExerciseMetricSupport>();
    
    private MetricType() { }
    
    public static class Handler
    {
        public static MetricType CreateNew(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be empty", nameof(name));
            }
            
            return new()
            {
                Id = MetricTypeId.New(),
                Name = name
            };
        }
        
        public static MetricType Create(MetricTypeId id, string name) =>
            new()
            {
                Id = id,
                Name = name
            };
    }
}
