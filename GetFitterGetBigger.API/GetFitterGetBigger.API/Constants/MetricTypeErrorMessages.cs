namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Error messages specific to MetricType operations
/// </summary>
public static class MetricTypeErrorMessages
{
    // Validation errors
    public const string InvalidIdFormat = "Invalid metric type ID format";
    public const string ValueCannotBeEmpty = "Metric type value cannot be empty";
    public const string ValueCannotBeEmptyEntity = "Metric type value cannot be empty";
    public const string DisplayOrderMustBeNonNegative = "Display order must be non-negative";
    
    // Not found errors
    public const string NotFound = "Metric type";
}