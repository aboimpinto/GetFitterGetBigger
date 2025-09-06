namespace GetFitterGetBigger.Admin.Services;

/// <summary>
/// Service interface for performance monitoring and telemetry collection.
/// Provides methods to track component performance, user interactions, and system metrics.
/// </summary>
public interface IPerformanceMonitoringService
{
    /// <summary>
    /// Starts tracking a named operation for performance measurement.
    /// </summary>
    /// <param name="operationName">The unique name of the operation to track</param>
    void StartOperation(string operationName);

    /// <summary>
    /// Ends tracking of a named operation and records the elapsed time.
    /// </summary>
    /// <param name="operationName">The name of the operation to stop tracking</param>
    void EndOperation(string operationName);

    /// <summary>
    /// Tracks a custom metric with optional properties.
    /// </summary>
    /// <param name="metricName">The name of the metric</param>
    /// <param name="value">The numeric value of the metric</param>
    /// <param name="properties">Optional dictionary of additional properties</param>
    void TrackMetric(string metricName, double value, Dictionary<string, string>? properties = null);

    /// <summary>
    /// Tracks the render cycle time of a Blazor component.
    /// </summary>
    /// <param name="componentName">The name of the component</param>
    /// <param name="renderTimeMs">The render time in milliseconds</param>
    void TrackRenderCycle(string componentName, double renderTimeMs);

    /// <summary>
    /// Tracks user interaction events with components.
    /// </summary>
    /// <param name="eventName">The name of the event (e.g., "button_click", "context_switch")</param>
    /// <param name="componentName">The component where the event occurred</param>
    /// <param name="properties">Optional additional properties about the event</param>
    void TrackEvent(string eventName, string componentName, Dictionary<string, string>? properties = null);

    /// <summary>
    /// Records a performance warning when thresholds are exceeded.
    /// </summary>
    /// <param name="warningType">The type of performance warning</param>
    /// <param name="message">Descriptive message about the warning</param>
    /// <param name="threshold">The threshold that was exceeded</param>
    /// <param name="actualValue">The actual value that exceeded the threshold</param>
    void LogPerformanceWarning(string warningType, string message, double threshold, double actualValue);

    /// <summary>
    /// Gets the average metric value for a given metric name within a time window.
    /// </summary>
    /// <param name="metricName">The name of the metric</param>
    /// <param name="windowMinutes">The time window in minutes (default: 5)</param>
    /// <returns>The average value or null if no data exists</returns>
    double? GetAverageMetric(string metricName, int windowMinutes = 5);

    /// <summary>
    /// Clears all collected metrics and resets tracking.
    /// </summary>
    void ClearMetrics();
}