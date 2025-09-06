namespace GetFitterGetBigger.Admin.Services;

/// <summary>
/// No-operation implementation of IPerformanceMonitoringService.
/// Used in development/testing environments where performance monitoring is not required.
/// This implementation performs no actual tracking, allowing the application to run
/// without overhead or external dependencies.
/// </summary>
public class NoOpPerformanceMonitoringService : IPerformanceMonitoringService
{
    /// <summary>
    /// No-op implementation - does nothing.
    /// </summary>
    public void StartOperation(string operationName)
    {
        // No-op: In production, this would start timing the operation
    }

    /// <summary>
    /// No-op implementation - does nothing.
    /// </summary>
    public void EndOperation(string operationName)
    {
        // No-op: In production, this would calculate and record elapsed time
    }

    /// <summary>
    /// No-op implementation - does nothing.
    /// </summary>
    public void TrackMetric(string metricName, double value, Dictionary<string, string>? properties = null)
    {
        // No-op: In production, this would send metric to telemetry service
    }

    /// <summary>
    /// No-op implementation - does nothing.
    /// </summary>
    public void TrackRenderCycle(string componentName, double renderTimeMs)
    {
        // No-op: In production, this would track component performance
    }

    /// <summary>
    /// No-op implementation - does nothing.
    /// </summary>
    public void TrackEvent(string eventName, string componentName, Dictionary<string, string>? properties = null)
    {
        // No-op: In production, this would track user interaction events
    }

    /// <summary>
    /// No-op implementation - does nothing.
    /// </summary>
    public void LogPerformanceWarning(string warningType, string message, double threshold, double actualValue)
    {
        // No-op: In production, this would log warnings to monitoring system
    }

    /// <summary>
    /// No-op implementation - always returns null.
    /// </summary>
    public double? GetAverageMetric(string metricName, int windowMinutes = 5)
    {
        // No-op: In production, this would calculate actual averages
        return null;
    }

    /// <summary>
    /// No-op implementation - does nothing.
    /// </summary>
    public void ClearMetrics()
    {
        // No-op: In production, this would clear stored metrics
    }
}