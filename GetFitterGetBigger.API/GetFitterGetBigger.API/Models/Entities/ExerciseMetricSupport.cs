using System;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExerciseMetricSupport
{
    public ExerciseId ExerciseId { get; init; }
    public MetricTypeId MetricTypeId { get; init; }
    
    // Navigation properties
    public Exercise Exercise { get; init; } = null!;
    public MetricType MetricType { get; init; } = null!;
    
    private ExerciseMetricSupport() { }
    
    public static class Handler
    {
        public static ExerciseMetricSupport Create(ExerciseId exerciseId, MetricTypeId metricTypeId) =>
            new()
            {
                ExerciseId = exerciseId,
                MetricTypeId = metricTypeId
            };
    }
}
