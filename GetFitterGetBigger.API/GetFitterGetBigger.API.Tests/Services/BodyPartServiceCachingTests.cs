using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class BodyPartServiceCachingTests
    {
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IBodyPartRepository> _mockBodyPartRepository;
        private readonly Mock<IEternalCacheService> _mockCacheService;
        private readonly Mock<ILogger<BodyPartService>> _mockLogger;
        private readonly BodyPartService _bodyPartService;

        public BodyPartServiceCachingTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockBodyPartRepository = new Mock<IBodyPartRepository>();
            _mockCacheService = new Mock<IEternalCacheService>();
            _mockLogger = new Mock<ILogger<BodyPartService>>();

            _mockUnitOfWorkProvider
                .Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);

            _mockReadOnlyUnitOfWork
                .Setup(x => x.GetRepository<IBodyPartRepository>())
                .Returns(_mockBodyPartRepository.Object);

            _bodyPartService = new BodyPartService(
                _mockUnitOfWorkProvider.Object,
                _mockCacheService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCacheHit_DoesNotCallDatabase()
        {
            // Arrange
            var bodyPartId = BodyPartId.New();
            var cachedDto = new BodyPartDto
            {
                Id = bodyPartId.ToString(),
                Value = "Chest",
                Description = "Chest muscles"
            };

            // Setup cache hit
            _mockCacheService
                .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<BodyPartDto>.Hit(cachedDto));

            // Act
            var result = await _bodyPartService.GetByIdAsync(bodyPartId.ToString());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Chest", result.Data.Value);
            
            // Verify database was NOT called
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
            _mockBodyPartRepository.Verify(x => x.GetByIdAsync(It.IsAny<BodyPartId>()), Times.Never);
            
            // Verify cache was checked
            _mockCacheService.Verify(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCacheMiss_CallsDatabaseAndStoresInCache()
        {
            // Arrange
            var bodyPartId = BodyPartId.New();
            var createResult = BodyPart.Handler.Create(
                bodyPartId,
                "Back",
                "Back muscles",
                2,
                true);
            Assert.True(createResult.IsSuccess);
            var bodyPart = createResult.Value;

            // Setup cache miss
            _mockCacheService
                .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<BodyPartDto>.Miss());

            // Setup database return
            _mockBodyPartRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<BodyPartId>()))
                .ReturnsAsync(bodyPart);

            // Act
            var result = await _bodyPartService.GetByIdAsync(bodyPartId.ToString());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Back", result.Data.Value);
            
            // Verify database was called
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockBodyPartRepository.Verify(x => x.GetByIdAsync(It.IsAny<BodyPartId>()), Times.Once);
            
            // Verify cache was checked and then set
            _mockCacheService.Verify(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()), Times.Once);
            _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<BodyPartDto>()), Times.Once);
        }

        [Fact]
        public async Task GetAllActiveAsync_WhenCacheHit_DoesNotCallDatabase()
        {
            // Arrange
            var cachedDtos = new List<BodyPartDto>
            {
                new BodyPartDto { Id = BodyPartId.New().ToString(), Value = "Chest", Description = "Chest muscles" },
                new BodyPartDto { Id = BodyPartId.New().ToString(), Value = "Back", Description = "Back muscles" }
            };

            // Setup cache hit
            _mockCacheService
                .Setup(x => x.GetAsync<IEnumerable<BodyPartDto>>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<IEnumerable<BodyPartDto>>.Hit(cachedDtos));

            // Act
            var result = await _bodyPartService.GetAllActiveAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            
            // Verify database was NOT called
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
            _mockBodyPartRepository.Verify(x => x.GetAllActiveAsync(), Times.Never);
            
            // Verify cache was checked
            _mockCacheService.Verify(x => x.GetAsync<IEnumerable<BodyPartDto>>(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllActiveAsync_WhenCacheMiss_CallsDatabaseAndStoresInCache()
        {
            // Arrange
            var chestResult = BodyPart.Handler.Create(BodyPartId.New(), "Chest", "Chest muscles", 1, true);
            var backResult = BodyPart.Handler.Create(BodyPartId.New(), "Back", "Back muscles", 2, true);
            Assert.True(chestResult.IsSuccess);
            Assert.True(backResult.IsSuccess);
            
            var bodyParts = new List<BodyPart>
            {
                chestResult.Value,
                backResult.Value
            };

            // Setup cache miss
            _mockCacheService
                .Setup(x => x.GetAsync<IEnumerable<BodyPartDto>>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<IEnumerable<BodyPartDto>>.Miss());

            // Setup database return
            _mockBodyPartRepository
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(bodyParts);

            // Act
            var result = await _bodyPartService.GetAllActiveAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            
            // Verify database was called
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockBodyPartRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
            
            // Verify cache was checked and then set
            _mockCacheService.Verify(x => x.GetAsync<IEnumerable<BodyPartDto>>(It.IsAny<string>()), Times.Once);
            _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<BodyPartDto>>()), Times.Once);
        }

        [Fact]
        public async Task GetByValueAsync_WhenCacheHit_DoesNotCallDatabase()
        {
            // Arrange
            var value = "Shoulders";
            var cachedDto = new BodyPartDto
            {
                Id = BodyPartId.New().ToString(),
                Value = value,
                Description = "Shoulder muscles"
            };

            // Setup cache hit
            _mockCacheService
                .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<BodyPartDto>.Hit(cachedDto));

            // Act
            var result = await _bodyPartService.GetByValueAsync(value);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(value, result.Data.Value);
            
            // Verify database was NOT called
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
            _mockBodyPartRepository.Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
            
            // Verify cache was checked
            _mockCacheService.Verify(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_LeveragesGetByIdCache()
        {
            // Arrange
            var bodyPartId = BodyPartId.New();
            var cachedDto = new BodyPartDto
            {
                Id = bodyPartId.ToString(),
                Value = "Legs",
                Description = "Leg muscles"
            };

            // Setup cache hit for GetByIdAsync
            _mockCacheService
                .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<BodyPartDto>.Hit(cachedDto));

            // Act
            var result = await _bodyPartService.ExistsAsync(bodyPartId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data.Value);
            
            // Verify database was NOT called
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
            _mockBodyPartRepository.Verify(x => x.GetByIdAsync(It.IsAny<BodyPartId>()), Times.Never);
            
            // Verify cache was checked (by GetByIdAsync internally)
            _mockCacheService.Verify(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()), Times.Once);
        }
    }
}