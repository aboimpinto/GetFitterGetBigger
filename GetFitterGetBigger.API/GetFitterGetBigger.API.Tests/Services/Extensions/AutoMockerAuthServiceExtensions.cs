using System.Collections.Generic;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Authentication.DataServices;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services.Extensions;

/// <summary>
/// AutoMocker extensions for AuthService testing
/// Provides fluent setup and verification methods for authentication-related dependencies
/// </summary>
public static class AutoMockerAuthServiceExtensions
{
    #region Setup Methods

    /// <summary>
    /// Sets up user query data service to return empty user (user not found)
    /// </summary>
    public static AutoMocker SetupUserNotFound(this AutoMocker mocker, string email)
    {
        mocker.GetMock<IUserQueryDataService>()
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(ServiceResult<UserDto>.Success(UserDto.Empty));
        return mocker;
    }

    /// <summary>
    /// Sets up user query data service to return existing user
    /// </summary>
    public static AutoMocker SetupExistingUser(this AutoMocker mocker, string email, UserDto existingUser)
    {
        mocker.GetMock<IUserQueryDataService>()
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(ServiceResult<UserDto>.Success(existingUser));
        return mocker;
    }

    /// <summary>
    /// Sets up user command data service to successfully create user with claim
    /// </summary>
    public static AutoMocker SetupCreateUserWithClaimSuccess(this AutoMocker mocker, string email, string claimType, UserDto createdUser)
    {
        mocker.GetMock<IUserCommandDataService>()
            .Setup(x => x.CreateUserWithClaimAsync(email, claimType))
            .ReturnsAsync(ServiceResult<UserDto>.Success(createdUser));
        return mocker;
    }

    /// <summary>
    /// Sets up user command data service to fail creating user with claim
    /// </summary>
    public static AutoMocker SetupCreateUserWithClaimFailure(this AutoMocker mocker, string email, string claimType, List<ServiceError> errors)
    {
        mocker.GetMock<IUserCommandDataService>()
            .Setup(x => x.CreateUserWithClaimAsync(email, claimType))
            .ReturnsAsync(ServiceResult<UserDto>.Failure(UserDto.Empty, errors));
        return mocker;
    }

    /// <summary>
    /// Sets up JWT service to generate token for user
    /// </summary>
    public static AutoMocker SetupJwtTokenGeneration(this AutoMocker mocker, UserDto user, string token)
    {
        mocker.GetMock<IJwtService>()
            .Setup(x => x.GenerateToken(user))
            .Returns(token);
        return mocker;
    }

    /// <summary>
    /// Sets up JWT service to throw exception during token generation
    /// </summary>
    public static AutoMocker SetupJwtTokenGenerationThrows(this AutoMocker mocker, UserDto user, Exception exception)
    {
        mocker.GetMock<IJwtService>()
            .Setup(x => x.GenerateToken(user))
            .Throws(exception);
        return mocker;
    }

    /// <summary>
    /// Sets up user query data service to throw exception
    /// </summary>
    public static AutoMocker SetupUserQueryThrows(this AutoMocker mocker, string email, Exception exception)
    {
        mocker.GetMock<IUserQueryDataService>()
            .Setup(x => x.GetByEmailAsync(email))
            .ThrowsAsync(exception);
        return mocker;
    }

    #endregion

    #region Verification Methods - Positive Cases

    /// <summary>
    /// Verifies user query data service was called once with specific email
    /// </summary>
    public static AutoMocker VerifyUserQueryCalledOnceWith(this AutoMocker mocker, string email)
    {
        mocker.GetMock<IUserQueryDataService>()
            .Verify(x => x.GetByEmailAsync(email), Times.Once);
        return mocker;
    }

    /// <summary>
    /// Verifies user command data service was called once to create user with claim
    /// </summary>
    public static AutoMocker VerifyCreateUserWithClaimCalledOnceWith(this AutoMocker mocker, string email, string claimType)
    {
        mocker.GetMock<IUserCommandDataService>()
            .Verify(x => x.CreateUserWithClaimAsync(email, claimType), Times.Once);
        return mocker;
    }

    /// <summary>
    /// Verifies JWT service was called once to generate token for user
    /// </summary>
    public static AutoMocker VerifyJwtTokenGenerationCalledOnceWith(this AutoMocker mocker, UserDto user)
    {
        mocker.GetMock<IJwtService>()
            .Verify(x => x.GenerateToken(user), Times.Once);
        return mocker;
    }

    #endregion

    #region Verification Methods - Negative Cases (Explicit Intent)

    /// <summary>
    /// Verifies user query data service was never called (validation failure scenario)
    /// </summary>
    public static AutoMocker VerifyUserQueryNeverCalledForValidationFailure(this AutoMocker mocker)
    {
        mocker.GetMock<IUserQueryDataService>()
            .Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        return mocker;
    }

    /// <summary>
    /// Verifies user command data service was never called (existing user scenario)
    /// </summary>
    public static AutoMocker VerifyCreateUserWithClaimNeverCalledForExistingUser(this AutoMocker mocker)
    {
        mocker.GetMock<IUserCommandDataService>()
            .Verify(x => x.CreateUserWithClaimAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        return mocker;
    }

    /// <summary>
    /// Verifies user command data service was never called (validation failure scenario)
    /// </summary>
    public static AutoMocker VerifyCreateUserWithClaimNeverCalledForValidationFailure(this AutoMocker mocker)
    {
        mocker.GetMock<IUserCommandDataService>()
            .Verify(x => x.CreateUserWithClaimAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        return mocker;
    }

    #endregion
}