using System;
using System.Linq;
using System.Threading.Tasks;
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
                whenNotEmpty: userData => Task.FromResult(GenerateAuthTokenAsync(userData))
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
                        ? GenerateAuthTokenAsync(userResult.Data)
                        : ServiceResult<AuthenticationResponse>.Failure(
                            AuthenticationResponse.Empty,
                            userResult.Errors);
                });
    }

    private async Task<ServiceResult<UserDto>> LoadUserByEmailAsync(string email)
    {
        try
        {
            using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
            var readOnlyUserRepo = readOnlyUow.GetRepository<IUserRepository>();
            var user = await readOnlyUserRepo.GetUserByEmailAsync(email);
            
            // Handle null here - this is the only place we deal with entity nulls
            var userDto = user?.ToDto() ?? UserDto.Empty;
            return ServiceResult<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user by email: {Email}", email);
            return ServiceResult<UserDto>.Failure(
                UserDto.Empty,
                ServiceError.InternalError(AuthenticationErrorMessages.Operations.FailedToLoadUser));
        }
    }

    private async Task<ServiceResult<bool>> CreateNewUserAccountAsync(string email)
    {
        try
        {
            using var unitOfWork = _unitOfWorkProvider.CreateWritable();
            var userRepository = unitOfWork.GetRepository<IUserRepository>();

            // Double-check in case of race condition
            var existingUser = await userRepository.GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                // User was created by another thread/request between the read-only check and now
                return ServiceResult<bool>.Success(true);
            }

            // User truly doesn't exist, create new user
            var user = new User { Id = UserId.New(), Email = email };
            await userRepository.AddUserAsync(user);

            // Use ClaimService with the same UnitOfWork for transactional consistency
            await _claimService.CreateUserClaimAsync(user.Id, "Free-Tier", unitOfWork);
            
            await unitOfWork.CommitAsync();

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new user for email: {Email}", email);
            return ServiceResult<bool>.Failure(
                false,
                ServiceError.InternalError(AuthenticationErrorMessages.Operations.FailedToCreateUser));
        }
    }

    private ServiceResult<AuthenticationResponse> GenerateAuthTokenAsync(UserDto userDto)
    {
        try
        {
            // Validate user has claims
            if (userDto.Claims == null || !userDto.Claims.Any())
            {
                _logger.LogError("User has no valid claims: {UserId}", userDto.Id);
                return ServiceResult<AuthenticationResponse>.Failure(
                    AuthenticationResponse.Empty,
                    ServiceError.InternalError(AuthenticationErrorMessages.Operations.FailedToGenerateToken));
            }
            
            // We need to convert UserDto back to User for JWT generation
            // This is acceptable since it's internal to the service
            var user = new User 
            { 
                Id = UserId.ParseOrEmpty(userDto.Id), 
                Email = userDto.Email,
                Claims = userDto.Claims.Select(c => new Claim 
                { 
                    Id = ClaimId.ParseOrEmpty(c.Id), 
                    ClaimType = c.ClaimType, 
                    ExpirationDate = c.ExpirationDate, 
                    Resource = c.Resource 
                }).ToList()
            };
            
            var token = _jwtService.GenerateToken(user);
            var response = new AuthenticationResponse(token, userDto.Claims);
            return ServiceResult<AuthenticationResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating authentication response for user: {UserId}", userDto.Id);
            return ServiceResult<AuthenticationResponse>.Failure(
                AuthenticationResponse.Empty, 
                ServiceError.InternalError(AuthenticationErrorMessages.Operations.FailedToGenerateToken));
        }
    }
}