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
        public static string UserIdCannotBeEmpty => "User ID cannot be empty";
        public static string ClaimTypeCannotBeEmpty => "Claim type cannot be empty";
    }
}