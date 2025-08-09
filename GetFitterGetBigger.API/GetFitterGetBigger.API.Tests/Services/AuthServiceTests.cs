using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.Authentication;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _mockUnitOfWork;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IUserRepository> _mockReadOnlyUserRepository;
        private readonly Mock<IClaimService> _mockClaimService;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockJwtService = new Mock<IJwtService>();
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockReadOnlyUserRepository = new Mock<IUserRepository>();
            _mockClaimService = new Mock<IClaimService>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            _mockUnitOfWorkProvider
                .Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);

            _mockUnitOfWorkProvider
                .Setup(x => x.CreateWritable())
                .Returns(_mockUnitOfWork.Object);

            // Setup repository connections
            _mockReadOnlyUnitOfWork
                .Setup(x => x.GetRepository<IUserRepository>())
                .Returns(_mockReadOnlyUserRepository.Object);
                
            _mockUnitOfWork
                .Setup(x => x.GetRepository<IUserRepository>())
                .Returns(_mockUserRepository.Object);

            _authService = new AuthService(_mockJwtService.Object, _mockUnitOfWorkProvider.Object, _mockClaimService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_WithNewUser_CreatesUserAndFreeTierClaim()
        {
            // Arrange
            var email = "newuser@example.com";
            var command = new AuthenticationCommand { Email = email };
            var expectedToken = "jwt-token";

            User? capturedUser = null;
            ClaimId? capturedClaimId = null;
            
            // Setup for user repository - sequential calls to ReadOnlyUserRepository
            _mockReadOnlyUserRepository
                .SetupSequence(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync((User?)null) // First call: New user doesn't exist
                .ReturnsAsync(() => capturedUser); // Second call: Return created user with claims
            
            _mockUserRepository
                .Setup(x => x.AddUserAsync(It.IsAny<User>()))
                .Callback<User>(u => 
                {
                    capturedUser = u;
                    // Initialize Claims collection for new user
                    u.Claims = new List<Claim>();
                })
                .ReturnsAsync((User u) => u);

            // Setup for the WritableUnitOfWork repository (for race condition check)
            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync((User?)null);

            _mockClaimService
                .Setup(x => x.CreateUserClaimAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<IWritableUnitOfWork<FitnessDbContext>>()))
                .Callback<UserId, string, IWritableUnitOfWork<FitnessDbContext>>((userId, claimType, uow) => 
                {
                    capturedClaimId = ClaimId.New();
                    // Add the claim to the user's claims collection
                    if (capturedUser != null && capturedUser.Claims != null)
                    {
                        capturedUser.Claims.Add(new Claim 
                        { 
                            Id = capturedClaimId.Value,
                            UserId = userId,
                            ClaimType = claimType,
                            ExpirationDate = null,
                            Resource = null
                        });
                    }
                })
                .ReturnsAsync((UserId userId, string claimType, IWritableUnitOfWork<FitnessDbContext> uow) => capturedClaimId ?? ClaimId.New());

            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns(expectedToken);

            // Act
            var result = await _authService.AuthenticateAsync(command);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(capturedUser);
            Assert.Equal(email, capturedUser.Email);

            Assert.NotNull(capturedClaimId);
            _mockClaimService.Verify(x => x.CreateUserClaimAsync(capturedUser.Id, "Free-Tier", _mockUnitOfWork.Object), Times.Once);

            Assert.Equal(expectedToken, result.Data.Token);
            Assert.Single(result.Data.Claims);
            Assert.Equal("Free-Tier", result.Data.Claims[0].ClaimType);

            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithExistingUser_ReturnsUserClaims()
        {
            // Arrange
            var email = "existing@example.com";
            var command = new AuthenticationCommand { Email = email };
            var userId = UserId.New();
            var expectedToken = "jwt-token";

            var claims = new List<Claim>
            {
                new Claim 
                { 
                    Id = ClaimId.New(), 
                    UserId = userId, 
                    ClaimType = "Free-Tier", 
                    ExpirationDate = null,
                    Resource = null
                },
                new Claim 
                { 
                    Id = ClaimId.New(), 
                    UserId = userId, 
                    ClaimType = "Premium-Tier", 
                    ExpirationDate = DateTime.UtcNow.AddDays(30),
                    Resource = "premium-features"
                }
            };

            var existingUser = new User 
            { 
                Id = userId, 
                Email = email,
                Claims = claims
            };

            _mockReadOnlyUserRepository
                .Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(existingUser);

            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns(expectedToken);

            // Act
            var result = await _authService.AuthenticateAsync(command);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedToken, result.Data.Token);
            Assert.Equal(2, result.Data.Claims.Count);
            Assert.Contains(result.Data.Claims, c => c.ClaimType == "Free-Tier");
            Assert.Contains(result.Data.Claims, c => c.ClaimType == "Premium-Tier");

            _mockUserRepository.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Never);
            _mockClaimService.Verify(x => x.CreateUserClaimAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<IWritableUnitOfWork<FitnessDbContext>>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
            _mockReadOnlyUserRepository.Verify(x => x.GetUserByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_FiltersExpiredClaims()
        {
            // Arrange
            var email = "user@example.com";
            var command = new AuthenticationCommand { Email = email };
            var userId = UserId.New();

            var claims = new List<Claim>
            {
                new Claim 
                { 
                    Id = ClaimId.New(), 
                    UserId = userId, 
                    ClaimType = "Active-Claim", 
                    ExpirationDate = DateTime.UtcNow.AddDays(30),
                    Resource = null
                },
                new Claim 
                { 
                    Id = ClaimId.New(), 
                    UserId = userId, 
                    ClaimType = "Expired-Claim", 
                    ExpirationDate = DateTime.UtcNow.AddDays(-1),
                    Resource = null
                },
                new Claim 
                { 
                    Id = ClaimId.New(), 
                    UserId = userId, 
                    ClaimType = "No-Expiry-Claim", 
                    ExpirationDate = null,
                    Resource = null
                }
            };

            var existingUser = new User 
            { 
                Id = userId, 
                Email = email,
                Claims = claims
            };

            // Setup for user repository
            _mockReadOnlyUserRepository
                .Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(existingUser);
                
            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
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
        public async Task AuthenticateAsync_DisposesUnitOfWork()
        {
            // Arrange
            var command = new AuthenticationCommand { Email = "test@example.com" };
            // Setup for user repository
            _mockReadOnlyUserRepository
                .Setup(x => x.GetUserByEmailAsync("test@example.com"))
                .ReturnsAsync((User?)null);
                
            _mockUserRepository
                .Setup(x => x.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => {
                    u.Claims = new List<Claim>();
                    return u;
                });
                
            _mockClaimService
                .Setup(x => x.CreateUserClaimAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<IWritableUnitOfWork<FitnessDbContext>>()))
                .ReturnsAsync(ClaimId.New());
                
            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("token");

            // Act
            await _authService.AuthenticateAsync(command);

            // Assert
            _mockUnitOfWork.Verify(x => x.Dispose(), Times.Once);
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
            
            // Verify no repository calls were made
            _mockReadOnlyUserRepository.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenUserRepositoryThrows_ReturnsInternalError()
        {
            // Arrange
            var command = new AuthenticationCommand { Email = "test@example.com" };
            _mockReadOnlyUserRepository
                .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _authService.AuthenticateAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.InternalError, result.PrimaryErrorCode);
            _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenJwtServiceThrows_ReturnsInternalError()
        {
            // Arrange
            var command = new AuthenticationCommand { Email = "test@example.com" };
            var user = new User { Id = UserId.New(), Email = command.Email, Claims = new List<Claim>() };
            
            // Setup for user repository
            _mockReadOnlyUserRepository
                .Setup(x => x.GetUserByEmailAsync(command.Email))
                .ReturnsAsync((User?)null);
                
            _mockUserRepository
                .Setup(x => x.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => {
                    u.Claims = new List<Claim>();
                    return u;
                });
                
            _mockClaimService
                .Setup(x => x.CreateUserClaimAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<IWritableUnitOfWork<FitnessDbContext>>()))
                .ReturnsAsync(ClaimId.New());
                
            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Throws(new InvalidOperationException("JWT configuration error"));

            // Act
            var result = await _authService.AuthenticateAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.InternalError, result.PrimaryErrorCode);
            _mockUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenCommitFails_ReturnsInternalError()
        {
            // Arrange
            var command = new AuthenticationCommand { Email = "test@example.com" };
            
            // Setup for user repository - it's called twice in the new flow
            // First call: check if user exists (returns null)
            // Second call: after failed creation, try to load again (still returns null)
            _mockReadOnlyUserRepository
                .SetupSequence(x => x.GetUserByEmailAsync(command.Email))
                .ReturnsAsync((User?)null)  // First call: user doesn't exist
                .ReturnsAsync((User?)null);  // Second call: still doesn't exist after failed creation
                
            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("token");
                
            _mockUserRepository
                .Setup(x => x.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => {
                    u.Claims = new List<Claim>();
                    return u;
                });

            _mockClaimService
                .Setup(x => x.CreateUserClaimAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<IWritableUnitOfWork<FitnessDbContext>>()))
                .ReturnsAsync(ClaimId.New());

            _mockUnitOfWork
                .Setup(x => x.CommitAsync())
                .ThrowsAsync(new InvalidOperationException("Database commit error"));

            // Act
            var result = await _authService.AuthenticateAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.InternalError, result.PrimaryErrorCode);
            _mockUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithUserHavingNullClaimsList_ReturnsInternalError()
        {
            // Arrange
            var command = new AuthenticationCommand { Email = "test@example.com" };
            var user = new User 
            { 
                Id = UserId.New(), 
                Email = command.Email,
                Claims = null! // Null claims collection
            };
            
            // Setup for user repository
            _mockReadOnlyUserRepository
                .Setup(x => x.GetUserByEmailAsync(command.Email))
                .ReturnsAsync(user);
                
            _mockJwtService
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("token");

            // Act
            var result = await _authService.AuthenticateAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.InternalError, result.PrimaryErrorCode);
        }
    }
}