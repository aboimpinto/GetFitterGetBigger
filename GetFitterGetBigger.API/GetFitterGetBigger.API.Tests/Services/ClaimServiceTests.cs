using System;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Authentication;
using GetFitterGetBigger.API.Services.Authentication.DataServices;
using GetFitterGetBigger.API.Services.Authentication.Models.DTOs;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class ClaimServiceTests
    {
        private readonly Mock<IClaimQueryDataService> _mockClaimQueryDataService;
        private readonly Mock<IClaimCommandDataService> _mockClaimCommandDataService;
        private readonly Mock<ILogger<ClaimService>> _mockLogger;
        private readonly ClaimService _claimService;

        public ClaimServiceTests()
        {
            _mockClaimQueryDataService = new Mock<IClaimQueryDataService>();
            _mockClaimCommandDataService = new Mock<IClaimCommandDataService>();
            _mockLogger = new Mock<ILogger<ClaimService>>();

            _claimService = new ClaimService(
                _mockClaimQueryDataService.Object,
                _mockClaimCommandDataService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CreateClaimAsync_WithValidData_CreatesClaimAndReturnsClaimInfo()
        {
            // Arrange
            var userId = UserId.New();
            var claimType = "Free-Tier";
            var claimId = ClaimId.New();
            var expectedClaimInfo = new ClaimInfo(claimId.ToString(), claimType, null, null);

            _mockClaimCommandDataService
                .Setup(x => x.CreateClaimAsync(userId, claimType, null, null))
                .ReturnsAsync(ServiceResult<ClaimInfo>.Success(expectedClaimInfo));

            // Act
            var result = await _claimService.CreateClaimAsync(userId, claimType);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedClaimInfo, result.Data);
            
            _mockClaimCommandDataService.Verify(x => x.CreateClaimAsync(userId, claimType, null, null), Times.Once);
        }

        [Fact]
        public async Task CreateClaimAsync_DifferentClaimTypes_CreatesCorrectClaims()
        {
            // Arrange
            var userId = UserId.New();
            var claimTypes = new[] { "Free-Tier", "Admin-Tier", "PT-Tier" };

            foreach (var claimType in claimTypes)
            {
                var claimId = ClaimId.New();
                var expectedClaimInfo = new ClaimInfo(claimId.ToString(), claimType, null, null);

                _mockClaimCommandDataService
                    .Setup(x => x.CreateClaimAsync(userId, claimType, null, null))
                    .ReturnsAsync(ServiceResult<ClaimInfo>.Success(expectedClaimInfo));

                // Act
                var result = await _claimService.CreateClaimAsync(userId, claimType);

                // Assert
                Assert.True(result.IsSuccess);
                Assert.Equal(claimType, result.Data.ClaimType);
                Assert.NotEqual(default(string), result.Data.Id);
            }
        }

        [Fact]
        public async Task CreateClaimAsync_MultipleCallsForSameUser_CreatesDistinctClaims()
        {
            // Arrange
            var userId = UserId.New();
            var claimType = "Free-Tier";
            var claimId1 = ClaimId.New();
            var claimId2 = ClaimId.New();
            var claimInfo1 = new ClaimInfo(claimId1.ToString(), claimType, null, null);
            var claimInfo2 = new ClaimInfo(claimId2.ToString(), claimType, null, null);

            _mockClaimCommandDataService
                .SetupSequence(x => x.CreateClaimAsync(userId, claimType, null, null))
                .ReturnsAsync(ServiceResult<ClaimInfo>.Success(claimInfo1))
                .ReturnsAsync(ServiceResult<ClaimInfo>.Success(claimInfo2));

            // Act
            var result1 = await _claimService.CreateClaimAsync(userId, claimType);
            var result2 = await _claimService.CreateClaimAsync(userId, claimType);

            // Assert
            Assert.True(result1.IsSuccess);
            Assert.True(result2.IsSuccess);
            Assert.NotEqual(result1.Data.Id, result2.Data.Id);
            Assert.Equal(claimType, result1.Data.ClaimType);
            Assert.Equal(claimType, result2.Data.ClaimType);
            
            _mockClaimCommandDataService.Verify(x => x.CreateClaimAsync(userId, claimType, null, null), Times.Exactly(2));
        }
    }
}