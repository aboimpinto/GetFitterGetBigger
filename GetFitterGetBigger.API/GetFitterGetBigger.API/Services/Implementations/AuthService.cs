using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Mappers;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.Authentication;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for user authentication operations
/// </summary>
public class AuthService(
    IJwtService jwtService,
    IUnitOfWorkProvider<Models.FitnessDbContext> unitOfWorkProvider,
    IClaimService claimService,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly IJwtService _jwtService = jwtService;
    private readonly IUnitOfWorkProvider<Models.FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly IClaimService _claimService = claimService;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<ServiceResult<AuthenticationResponse>> AuthenticateAsync(AuthenticationCommand command)
    {
        return await ServiceValidate.For<AuthenticationResponse>()
            .EnsureNotNull(command, AuthenticationErrorMessages.Validation.RequestCannotBeNull)
            .EnsureNotWhiteSpace(command?.Email, AuthenticationErrorMessages.Validation.EmailCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await ProcessAuthenticationAsync(command!)
            );
    }

    private async Task<ServiceResult<AuthenticationResponse>> ProcessAuthenticationAsync(AuthenticationCommand command)
    {
        return await ServiceValidate.For<AuthenticationResponse>()
            .WithServiceResultAsync(() => LoadUserByEmailAsync(command.Email))
            .ThenMatchDataAsync<AuthenticationResponse, UserDto>(
                whenEmpty: async () => await HandleNewUserAsync(command.Email),
                whenNotEmpty: userData => Task.FromResult(GenerateAuthToken(userData))
            );
    }

    private async Task<ServiceResult<AuthenticationResponse>> HandleNewUserAsync(string email)
    {
        return await ServiceValidate.For<AuthenticationResponse>()
            .EnsureServiceResultAsync(() => CreateNewUserAccountAsync(email))
            .ThenMatchAsync(
                whenValid: async () =>
                {
                    var userResult = await LoadUserByEmailAsync(email);
                    return userResult.IsSuccess 
                        ? GenerateAuthToken(userResult.Data)
                        : ServiceResult<AuthenticationResponse>.Failure(
                            AuthenticationResponse.Empty,
                            userResult.Errors);
                });
    }

    private async Task<ServiceResult<UserDto>> LoadUserByEmailAsync(string email)
    {
        using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
        var readOnlyUserRepo = readOnlyUow.GetRepository<IUserRepository>();
        var user = await readOnlyUserRepo.GetUserByEmailAsync(email);
        
        // Handle null here - this is the only place we deal with entity nulls
        var userDto = user?.ToDto() ?? UserDto.Empty;
        return ServiceResult<UserDto>.Success(userDto);
    }

    private async Task<ServiceResult<bool>> CreateNewUserAccountAsync(string email)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var userRepository = unitOfWork.GetRepository<IUserRepository>();

        // Create new user - we already validated it doesn't exist in LoadUserByEmailAsync
        var user = new User { Id = UserId.New(), Email = email };
        await userRepository.AddUserAsync(user);

        // Use ClaimService with the same UnitOfWork for transactional consistency
        await _claimService.CreateUserClaimAsync(user.Id, "Free-Tier", unitOfWork);
        
        await unitOfWork.CommitAsync();

        return ServiceResult<bool>.Success(true);
    }

    private ServiceResult<AuthenticationResponse> GenerateAuthToken(UserDto userDto)
    {
        var token = _jwtService.GenerateToken(userDto);
        // Handle null claims gracefully - convert to empty list
        var claims = userDto.Claims ?? [];
        var response = new AuthenticationResponse(token, claims);
        return ServiceResult<AuthenticationResponse>.Success(response);
    }
}