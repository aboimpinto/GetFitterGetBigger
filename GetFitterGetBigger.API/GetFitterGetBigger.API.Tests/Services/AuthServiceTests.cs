using System;
using System.Collections.Generic;
using FluentAssertions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Authentication;
using GetFitterGetBigger.API.Services.Commands.Authentication;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for AuthService using AutoMocker pattern
/// Tests authentication scenarios including new user creation, existing user login,
/// validation failures, and error handling
/// </summary>
public class AuthServiceTests
{

    [Fact]
    public async Task AuthenticateAsync_WithNewUserEmail_CreatesUserAndReturnsFreeTierToken()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<AuthService>();

        const string newUserEmail = "newuser@example.com";
        const string expectedToken = "jwt-token";
        const string freeTierClaimType = "Free-Tier";
        const string claimId = "claim1";
        
        var command = new AuthenticationCommand { Email = newUserEmail };
        var userId = UserId.New();
        var claims = new List<ClaimInfo>
        {
            new ClaimInfo(claimId, freeTierClaimType, null, null)
        };
        var createdUserDto = new UserDto(
            userId.ToString(),
            newUserEmail,
            claims);

        automocker
            .SetupUserNotFound(newUserEmail)
            .SetupCreateUserWithClaimSuccess(newUserEmail, freeTierClaimType, createdUserDto)
            .SetupJwtTokenGeneration(createdUserDto, expectedToken);

        // Act
        var result = await testee.AuthenticateAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Token.Should().Be(expectedToken);
        result.Data.Claims.Should().ContainSingle();
        result.Data.Claims[0].ClaimType.Should().Be(freeTierClaimType);

        automocker
            .VerifyUserQueryCalledOnceWith(newUserEmail)
            .VerifyCreateUserWithClaimCalledOnceWith(newUserEmail, freeTierClaimType);
    }

    [Fact]
    public async Task AuthenticateAsync_WithExistingUserEmail_ReturnsTokenWithExistingClaims()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<AuthService>();

        const string existingUserEmail = "existing@example.com";
        const string expectedToken = "jwt-token";
        const string freeTierClaimType = "Free-Tier";
        const string premiumTierClaimType = "Premium-Tier";
        const string freeTierClaimId = "claim1";
        const string premiumTierClaimId = "claim2";
        const string premiumFeatures = "premium-features";
        
        var command = new AuthenticationCommand { Email = existingUserEmail };
        var userId = UserId.New();

        var existingClaims = new List<ClaimInfo>
        {
            new ClaimInfo(freeTierClaimId, freeTierClaimType, null, null),
            new ClaimInfo(premiumTierClaimId, premiumTierClaimType, DateTime.UtcNow.AddDays(30), premiumFeatures)
        };

        var existingUserDto = new UserDto(
            userId.ToString(),
            existingUserEmail,
            existingClaims);

        automocker
            .SetupExistingUser(existingUserEmail, existingUserDto)
            .SetupJwtTokenGeneration(existingUserDto, expectedToken);

        // Act
        var result = await testee.AuthenticateAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Token.Should().Be(expectedToken);
        result.Data.Claims.Should().HaveCount(2);
        result.Data.Claims.Should().Contain(c => c.ClaimType == freeTierClaimType);
        result.Data.Claims.Should().Contain(c => c.ClaimType == premiumTierClaimType);

        automocker
            .VerifyUserQueryCalledOnceWith(existingUserEmail)
            .VerifyCreateUserWithClaimNeverCalledForExistingUser();
    }

    [Fact]
    public async Task AuthenticateAsync_WithUserHavingOnlyActiveClaims_ReturnsTokenWithActiveClaimsOnly()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<AuthService>();

        const string userEmail = "user@example.com";
        const string jwtToken = "token";
        const string activeClaimType = "Active-Claim";
        const string noExpiryClaimType = "No-Expiry-Claim";
        const string activeClaimId = "claim1";
        const string noExpiryClaimId = "claim2";
        
        var command = new AuthenticationCommand { Email = userEmail };
        var userId = UserId.New();

        // Note: DataService already filters expired claims, so we only get active ones
        var activeClaims = new List<ClaimInfo>
        {
            new ClaimInfo(activeClaimId, activeClaimType, DateTime.UtcNow.AddDays(30), null),
            new ClaimInfo(noExpiryClaimId, noExpiryClaimType, null, null)
        };

        var existingUserDto = new UserDto(
            userId.ToString(),
            userEmail,
            activeClaims);

        automocker
            .SetupExistingUser(userEmail, existingUserDto)
            .SetupJwtTokenGeneration(existingUserDto, jwtToken);

        // Act
        var result = await testee.AuthenticateAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Claims.Should().HaveCount(2);
        result.Data.Claims.Should().Contain(c => c.ClaimType == activeClaimType);
        result.Data.Claims.Should().Contain(c => c.ClaimType == noExpiryClaimType);
        result.Data.Claims.Should().NotContain(c => c.ClaimType == "Expired-Claim",
            because: "DataService filters expired claims before returning user");
    }

    [Fact]
    public async Task AuthenticateAsync_WithNewUserAndValidEmail_CallsAllRequiredServicesInCorrectOrder()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<AuthService>();

        const string testEmail = "test@example.com";
        const string freeTierClaimType = "Free-Tier";
        const string jwtToken = "token";
        const string claimId = "claim1";
        
        var command = new AuthenticationCommand { Email = testEmail };
        var userId = UserId.New();
        var claims = new List<ClaimInfo> { new ClaimInfo(claimId, freeTierClaimType, null, null) };
        var createdUserDto = new UserDto(userId.ToString(), testEmail, claims);

        automocker
            .SetupUserNotFound(testEmail)
            .SetupCreateUserWithClaimSuccess(testEmail, freeTierClaimType, createdUserDto)
            .SetupJwtTokenGeneration(createdUserDto, jwtToken);

        // Act
        var result = await testee.AuthenticateAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        automocker
            .VerifyUserQueryCalledOnceWith(testEmail)
            .VerifyCreateUserWithClaimCalledOnceWith(testEmail, freeTierClaimType)
            .VerifyJwtTokenGenerationCalledOnceWith(createdUserDto);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task AuthenticateAsync_WithEmptyOrWhitespaceEmail_ReturnsValidationFailureWithoutCallingServices(string invalidEmail)
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<AuthService>();
        
        var command = new AuthenticationCommand { Email = invalidEmail };

        // Act
        var result = await testee.AuthenticateAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        // Verify no data service calls were made for invalid input
        automocker
            .VerifyUserQueryNeverCalledForValidationFailure()
            .VerifyCreateUserWithClaimNeverCalledForValidationFailure();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@missinglocal.com")]
    [InlineData("email@")]
    [InlineData("two@@example.com")]
    public async Task AuthenticateAsync_WithInvalidEmailFormat_ReturnsValidationFailureWithCorrectMessage(string invalidEmail)
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<AuthService>();
        
        var command = new AuthenticationCommand { Email = invalidEmail };

        // Act
        var result = await testee.AuthenticateAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(AuthenticationErrorMessages.Validation.InvalidEmailFormat);
        
        // Verify no data service calls were made for invalid input
        automocker
            .VerifyUserQueryNeverCalledForValidationFailure()
            .VerifyCreateUserWithClaimNeverCalledForValidationFailure();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenUserQueryDataServiceThrows_ExceptionBubblesUpNaturally()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<AuthService>();

        const string testEmail = "test@example.com";
        const string databaseErrorMessage = "Database error";
        
        var command = new AuthenticationCommand { Email = testEmail };
        var databaseException = new InvalidOperationException(databaseErrorMessage);
        
        automocker.SetupUserQueryThrows(testEmail, databaseException);

        // Act & Assert
        // With the new no-try-catch architecture, exceptions bubble up naturally
        var thrownException = await testee.Invoking(x => x.AuthenticateAsync(command))
            .Should().ThrowAsync<InvalidOperationException>();
        
        thrownException.Which.Message.Should().Be(databaseErrorMessage);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenJwtServiceThrows_ExceptionBubblesUpAfterUserCreation()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<AuthService>();

        const string testEmail = "test@example.com";
        const string freeTierClaimType = "Free-Tier";
        const string jwtErrorMessage = "JWT configuration error";
        
        var command = new AuthenticationCommand { Email = testEmail };
        var userId = UserId.New();
        var createdUserDto = new UserDto(userId.ToString(), testEmail, new List<ClaimInfo>());
        var jwtException = new InvalidOperationException(jwtErrorMessage);
        
        automocker
            .SetupUserNotFound(testEmail)
            .SetupCreateUserWithClaimSuccess(testEmail, freeTierClaimType, createdUserDto)
            .SetupJwtTokenGenerationThrows(createdUserDto, jwtException);

        // Act & Assert
        // With the new no-try-catch architecture, exceptions bubble up naturally
        var thrownException = await testee.Invoking(x => x.AuthenticateAsync(command))
            .Should().ThrowAsync<InvalidOperationException>();
            
        thrownException.Which.Message.Should().Be(jwtErrorMessage);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenCreateUserWithClaimFails_ReturnsFailureWithOriginalErrors()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<AuthService>();

        const string testEmail = "test@example.com";
        const string freeTierClaimType = "Free-Tier";
        const string databaseCommitError = "Database commit error";
        
        var command = new AuthenticationCommand { Email = testEmail };
        var serviceErrors = new List<ServiceError> { ServiceError.InternalError(databaseCommitError) };
        
        automocker
            .SetupUserNotFound(testEmail)
            .SetupCreateUserWithClaimFailure(testEmail, freeTierClaimType, serviceErrors);

        // Act
        var result = await testee.AuthenticateAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StructuredErrors.Should().BeEquivalentTo(serviceErrors);
        result.Data.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task AuthenticateAsync_WithUserHavingNullClaimsList_HandlesNullClaimsGracefully()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<AuthService>();

        const string testEmail = "test@example.com";
        const string jwtToken = "token";
        
        var command = new AuthenticationCommand { Email = testEmail };
        var userDtoWithNullClaims = new UserDto(
            UserId.New().ToString(), 
            testEmail,
            null! // Null claims collection
        );
        
        automocker
            .SetupExistingUser(testEmail, userDtoWithNullClaims)
            .SetupJwtTokenGeneration(userDtoWithNullClaims, jwtToken);

        // Act
        var result = await testee.AuthenticateAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue(because: "architecture handles null claims gracefully");
        result.Data.Token.Should().Be(jwtToken);
        result.Data.Claims.Should().NotBeNull();
        result.Data.Claims.Should().BeEmpty();
    }
}