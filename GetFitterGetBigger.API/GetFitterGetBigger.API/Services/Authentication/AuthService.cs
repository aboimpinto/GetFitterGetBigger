using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Authentication.DataServices;
using GetFitterGetBigger.API.Services.Commands.Authentication;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;

namespace GetFitterGetBigger.API.Services.Authentication;

/// <summary>
/// Service implementation for user authentication operations
/// </summary>
public class AuthService(
    IJwtService jwtService,
    IUserQueryDataService userQueryDataService,
    IUserCommandDataService userCommandDataService,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly IJwtService _jwtService = jwtService;
    private readonly IUserQueryDataService _userQueryDataService = userQueryDataService;
    private readonly IUserCommandDataService _userCommandDataService = userCommandDataService;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<ServiceResult<AuthenticationResponse>> AuthenticateAsync(AuthenticationCommand command)
    {
        return await ServiceValidate.For<AuthenticationResponse>()
            .EnsureNotNull(command, AuthenticationErrorMessages.Validation.RequestCannotBeNull)
            .ThenEnsureNotWhiteSpace(command.Email, AuthenticationErrorMessages.Validation.EmailCannotBeEmpty)
            .ThenEnsureEmailIsValid(command.Email, AuthenticationErrorMessages.Validation.InvalidEmailFormat)
            .MatchAsync(
                whenValid: async () => await ProcessAuthenticationAsync(command!)
            );
    }

    private async Task<ServiceResult<AuthenticationResponse>> ProcessAuthenticationAsync(AuthenticationCommand command)
    {
        return await ServiceValidate.For<AuthenticationResponse>()
            .WithServiceResultAsync(() => _userQueryDataService.GetByEmailAsync(command.Email))
            .ThenMatchDataAsync<AuthenticationResponse, UserDto>(
                whenEmpty: async () => await HandleNewUserAsync(command.Email),
                whenNotEmpty: userData => Task.FromResult(GenerateAuthToken(userData))
            );
    }

    private async Task<ServiceResult<AuthenticationResponse>> HandleNewUserAsync(string email)
    {
        // Create new user with Free-Tier claim
        var createUserResult = await _userCommandDataService.CreateUserWithClaimAsync(email, "Free-Tier");
        
        return createUserResult.IsSuccess
            ? GenerateAuthToken(createUserResult.Data)
            : ServiceResult<AuthenticationResponse>.Failure(
                AuthenticationResponse.Empty,
                createUserResult.StructuredErrors);
    }

    private ServiceResult<AuthenticationResponse> GenerateAuthToken(UserDto userDto)
    {
        var token = _jwtService.GenerateToken(userDto);
        var claims = userDto.Claims ?? [];
        var response = new AuthenticationResponse(token, claims);
        return ServiceResult<AuthenticationResponse>.Success(response);
    }
}