namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Centralized error messages for Authentication-related operations
/// </summary>
public static class AuthenticationErrorMessages
{
    /// <summary>
    /// Validation error messages
    /// </summary>
    public static class Validation
    {
        public static string EmailCannotBeEmpty => "Email cannot be empty";
        public static string RequestCannotBeNull => "Request cannot be null";
        public static string InvalidEmailFormat => "Invalid email format";
    }
    
    /// <summary>
    /// Entity operation error messages
    /// </summary>
    public static class Operations
    {
        public static string FailedToCreateUser => "Failed to create user";
        public static string FailedToCreateClaim => "Failed to create user claim";
        public static string FailedToRetrieveUser => "Failed to retrieve created user";
        public static string FailedToGenerateToken => "Failed to generate authentication token";
        public static string FailedToLoadUser => "Failed to load user";
    }
}