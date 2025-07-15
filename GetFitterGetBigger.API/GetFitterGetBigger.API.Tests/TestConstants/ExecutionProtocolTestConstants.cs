namespace GetFitterGetBigger.API.Tests.TestConstants;

/// <summary>
/// Test constants for ExecutionProtocol tests to avoid magic strings
/// </summary>
public static class ExecutionProtocolTestConstants
{
    // Test names/values
    public const string StandardValue = "Standard";
    public const string SupersetValue = "Superset";
    public const string InactiveValue = "Inactive";
    public const string NonExistentValue = "NonExistent";
    public const string InactiveProtocolValue = "InactiveProtocol";
    
    // Test descriptions
    public const string StandardDescription = "Standard protocol";
    public const string SupersetDescription = "Superset protocol";
    public const string InactiveDescription = "Inactive protocol";
    
    // Test codes
    public const string StandardCode = "STANDARD";
    public const string SupersetCode = "SUPERSET";
    public const string InactiveCode = "INACTIVE";
    public const string NonExistentCode = "NONEXISTENT";
    
    // Test IDs
    public const string TestExecutionProtocolId = "executionprotocol-123";
    public const string ValidExecutionProtocolId = "executionprotocol-12345678-1234-1234-1234-123456789012";
    public const string NonExistentExecutionProtocolId = "executionprotocol-00000000-0000-0000-0000-000000000999";
    public const string InvalidFormatId = "invalid-format";
    
    // Rest patterns
    public const string StandardRestPattern = "60-90 seconds";
    public const string RestAfterBoth = "Rest after both";
    
    // Intensity levels
    public const string ModerateIntensity = "Moderate";
    public const string HighIntensity = "High";
    
    // Display orders
    public const int StandardDisplayOrder = 1;
    public const int SupersetDisplayOrder = 2;
    
    // Cache keys
    public const string AllExecutionProtocolsCacheKey = "ExecutionProtocol:all";
    
    // Partial error message for not found errors
    public const string NotFoundPartialMessage = "not found";
    
    // Empty string for clarity
    public const string EmptyString = "";
}