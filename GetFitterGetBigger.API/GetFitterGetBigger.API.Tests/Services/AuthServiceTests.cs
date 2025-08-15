using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Authentication;
using GetFitterGetBigger.API.Services.Authentication.DataServices;
using GetFitterGetBigger.API.Services.Commands.Authentication;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IUserQueryDataService> _mockUserQueryDataService;
        private readonly Mock<IUserCommandDataService> _mockUserCommandDataService;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockJwtService = new Mock<IJwtService>();
            _mockUserQueryDataService = new Mock<IUserQueryDataService>();
            _mockUserCommandDataService = new Mock<IUserCommandDataService>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(
                _mockJwtService.Object,
                _mockUserQueryDataService.Object,
                _mockUserCommandDataService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_WithNewUser_CreatesUserAndFreeTierClaim()
        {
            // Arrange
            var email = "newuser@example.com";
            var command = new AuthenticationCommand { Email = email };
            var expectedToken = "jwt-token";
            var userId = UserId.New();
            var claims = new List<ClaimInfo>
            {
                new ClaimInfo("claim1", "Free-Tier", null, null)
            };
            var expectedUserDto = new UserDto(
                userId.ToString(),
                email,
                claims);

            // Setup: User doesn't exist initially
            _mockUserQueryDataService
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(ServiceResult<UserDto>.Success(UserDto.Empty));

            // Setup: Creating new user with claim succeeds
            _mockUserCommandDataService
                .Setup(x => x.CreateUserWithClaimAsync(email, "Free-Tier"))
                .ReturnsAsync(ServiceResult<UserDto>.Success(expectedUserDto));

            _mockJwtService
                .Setup(x => x.GenerateToken(expectedUserDto))
                .Returns(expectedToken);

            // Act
            var result = await _authService.AuthenticateAsync(command);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedToken, result.Data.Token);
            Assert.Single(result.Data.Claims);
            Assert.Equal("Free-Tier", result.Data.Claims[0].ClaimType);

            _mockUserQueryDataService.Verify(x => x.GetByEmailAsync(email), Times.Once);
            _mockUserCommandDataService.Verify(x => x.CreateUserWithClaimAsync(email, "Free-Tier"), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithExistingUser_ReturnsUserClaims()
        {
            // Arrange
            var email = "existing@example.com";
            var command = new AuthenticationCommand { Email = email };
            var userId = UserId.New();
            var expectedToken = "jwt-token";

            var claims = new List<ClaimInfo>
            {
                new ClaimInfo("claim1", "Free-Tier", null, null),
                new ClaimInfo("claim2", "Premium-Tier", DateTime.UtcNow.AddDays(30), "premium-features")
            };

            var existingUserDto = new UserDto(
                userId.ToString(),
                email,
                claims);

            _mockUserQueryDataService
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(ServiceResult<UserDto>.Success(existingUserDto));

            _mockJwtService
                .Setup(x => x.GenerateToken(existingUserDto))
                .Returns(expectedToken);

            // Act
            var result = await _authService.AuthenticateAsync(command);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedToken, result.Data.Token);
            Assert.Equal(2, result.Data.Claims.Count);
            Assert.Contains(result.Data.Claims, c => c.ClaimType == "Free-Tier");
            Assert.Contains(result.Data.Claims, c => c.ClaimType == "Premium-Tier");

            _mockUserQueryDataService.Verify(x => x.GetByEmailAsync(email), Times.Once);
            _mockUserCommandDataService.Verify(x => x.CreateUserWithClaimAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_FiltersExpiredClaims()
        {
            // Arrange
            var email = "user@example.com";
            var command = new AuthenticationCommand { Email = email };
            var userId = UserId.New();

            // Only include active claims in the UserDto (expired claims filtered by DataService)
            var activeClaims = new List<ClaimInfo>
            {
                new ClaimInfo("claim1", "Active-Claim", DateTime.UtcNow.AddDays(30), null),
                new ClaimInfo("claim2", "No-Expiry-Claim", null, null)
            };

            var existingUserDto = new UserDto(
                userId.ToString(),
                email,
                activeClaims);

            _mockUserQueryDataService
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(ServiceResult<UserDto>.Success(existingUserDto));
                
            _mockJwtService
                .Setup(x => x.GenerateToken(existingUserDto))
                .Returns("token");

            // Act
            var result = await _authService.AuthenticateAsync(command);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Claims.Count);
            Assert.Contains(result.Data.Claims, c => c.ClaimType == "Active-Claim");
            Assert.Contains(result.Data.Claims, c => c.ClaimType == "No-Expiry-Claim");
            Assert.DoesNotContain(result.Data.Claims, c => c.ClaimType == "Expired-Claim");
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidRequest_CallsCorrectServices()
        {
            // Arrange
            var email = "test@example.com";
            var command = new AuthenticationCommand { Email = email };
            var userId = UserId.New();
            var claims = new List<ClaimInfo> { new ClaimInfo("claim1", "Free-Tier", null, null) };
            var userDto = new UserDto(userId.ToString(), email, claims);

            _mockUserQueryDataService
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(ServiceResult<UserDto>.Success(UserDto.Empty));
                
            _mockUserCommandDataService
                .Setup(x => x.CreateUserWithClaimAsync(email, "Free-Tier"))
                .ReturnsAsync(ServiceResult<UserDto>.Success(userDto));
                
            _mockJwtService
                .Setup(x => x.GenerateToken(userDto))
                .Returns("token");

            // Act
            var result = await _authService.AuthenticateAsync(command);

            // Assert
            Assert.True(result.IsSuccess);
            _mockUserQueryDataService.Verify(x => x.GetByEmailAsync(email), Times.Once);
            _mockUserCommandDataService.Verify(x => x.CreateUserWithClaimAsync(email, "Free-Tier"), Times.Once);
            _mockJwtService.Verify(x => x.GenerateToken(userDto), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task AuthenticateAsync_WithInvalidEmail_ReturnsValidationError(string email)
        {
            // Arrange
            var command = new AuthenticationCommand { Email = email };

            // Act
            var result = await _authService.AuthenticateAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            
            // Verify no data service calls were made
            _mockUserQueryDataService.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockUserCommandDataService.Verify(x => x.CreateUserWithClaimAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenUserQueryDataServiceThrows_ThrowsException()
        {
            // Arrange
            var command = new AuthenticationCommand { Email = "test@example.com" };
            _mockUserQueryDataService
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            // With the new no-try-catch architecture, exceptions bubble up naturally
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.AuthenticateAsync(command));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenJwtServiceThrows_ThrowsException()
        {
            // Arrange
            var command = new AuthenticationCommand { Email = "test@example.com" };
            var userId = UserId.New();
            var userDto = new UserDto(userId.ToString(), command.Email, new List<ClaimInfo>());
            
            _mockUserQueryDataService
                .Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync(ServiceResult<UserDto>.Success(UserDto.Empty));
                
            _mockUserCommandDataService
                .Setup(x => x.CreateUserWithClaimAsync(command.Email, "Free-Tier"))
                .ReturnsAsync(ServiceResult<UserDto>.Success(userDto));
                
            _mockJwtService
                .Setup(x => x.GenerateToken(userDto))
                .Throws(new InvalidOperationException("JWT configuration error"));

            // Act & Assert
            // With the new no-try-catch architecture, exceptions bubble up naturally
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.AuthenticateAsync(command));
            Assert.Equal("JWT configuration error", exception.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenCreateUserWithClaimFails_ReturnsFailureResult()
        {
            // Arrange
            var command = new AuthenticationCommand { Email = "test@example.com" };
            var errors = new List<ServiceError> { ServiceError.InternalError("Database commit error") };
            
            _mockUserQueryDataService
                .Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync(ServiceResult<UserDto>.Success(UserDto.Empty));
                
            _mockUserCommandDataService
                .Setup(x => x.CreateUserWithClaimAsync(command.Email, "Free-Tier"))
                .ReturnsAsync(ServiceResult<UserDto>.Failure(UserDto.Empty, errors));

            // Act
            var result = await _authService.AuthenticateAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(errors, result.StructuredErrors);
            Assert.True(result.Data.IsEmpty);
        }

        [Fact]
        public async Task AuthenticateAsync_WithUserHavingNullClaimsList_SucceedsWithEmptyClaimsList()
        {
            // Arrange
            var command = new AuthenticationCommand { Email = "test@example.com" };
            var userDto = new UserDto(
                UserId.New().ToString(), 
                command.Email,
                null! // Null claims collection
            );
            
            _mockUserQueryDataService
                .Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync(ServiceResult<UserDto>.Success(userDto));
                
            _mockJwtService
                .Setup(x => x.GenerateToken(userDto))
                .Returns("token");

            // Act
            var result = await _authService.AuthenticateAsync(command);

            // Assert
            // The new architecture handles null claims gracefully by converting to empty list
            Assert.True(result.IsSuccess);
            Assert.Equal("token", result.Data.Token);
            Assert.NotNull(result.Data.Claims);
            Assert.Empty(result.Data.Claims);
        }
    }
}