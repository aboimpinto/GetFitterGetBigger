namespace GetFitterGetBigger.API.Services.Commands.Authentication;

/// <summary>
/// Command for user authentication
/// </summary>
public record AuthenticationCommand
{
    /// <summary>
    /// The user's email address
    /// </summary>
    public required string Email { get; init; }
}