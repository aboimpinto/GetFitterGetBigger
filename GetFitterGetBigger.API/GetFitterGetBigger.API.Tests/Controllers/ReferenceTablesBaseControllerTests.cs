using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class ReferenceTablesBaseControllerTests
{
    // Test implementation of ReferenceTablesBaseController for testing
    private class TestReferenceController : ReferenceTablesBaseController
    {
        public TestReferenceController(
            IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
            ICacheService cacheService,
            IOptions<CacheConfiguration> cacheConfiguration,
            ILogger<ReferenceTablesBaseController> logger) 
            : base(unitOfWorkProvider, cacheService, cacheConfiguration, logger)
        {
        }

        // Expose protected method for testing
        public ReferenceDataDto TestMapToDto<TEntity>(TEntity entity) where TEntity : ReferenceDataBase
        {
            return MapToDto(entity);
        }
    }

    private readonly TestReferenceController _controller;
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IOptions<CacheConfiguration>> _mockCacheConfiguration;
    private readonly Mock<ILogger<ReferenceTablesBaseController>> _mockLogger;

    public ReferenceTablesBaseControllerTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockCacheService = new Mock<ICacheService>();
        _mockCacheConfiguration = new Mock<IOptions<CacheConfiguration>>();
        _mockLogger = new Mock<ILogger<ReferenceTablesBaseController>>();
        
        // Setup default cache configuration
        var cacheConfig = new CacheConfiguration
        {
            StaticTables = new TableCacheConfiguration
            {
                DurationInHours = 24,
                Tables = new List<string> { "DifficultyLevels", "KineticChainTypes", "BodyParts", "MuscleRoles" }
            },
            DynamicTables = new TableCacheConfiguration
            {
                DurationInHours = 1,
                Tables = new List<string> { "Equipment", "MetricTypes", "MovementPatterns", "MuscleGroups" }
            }
        };
        _mockCacheConfiguration.Setup(x => x.Value).Returns(cacheConfig);
        
        _controller = new TestReferenceController(
            _mockUnitOfWorkProvider.Object,
            _mockCacheService.Object,
            _mockCacheConfiguration.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void MapToDto_ValidEntity_ReturnsCorrectDto()
    {
        // Arrange
        var bodyPart = BodyPart.Handler.Create(
            BodyPartId.New(),
            "Chest",
            "Pectoral muscles",
            1,
            true
        );

        // Act
        var result = _controller.TestMapToDto(bodyPart);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bodyPart.Id.ToString(), result.Id);
        Assert.Equal(bodyPart.Value, result.Value);
        Assert.Equal(bodyPart.Description, result.Description);
    }

    [Fact]
    public void MapToDto_EntityWithNullDescription_ReturnsCorrectDto()
    {
        // Arrange
        var bodyPart = BodyPart.Handler.Create(
            BodyPartId.New(),
            "Back",
            null,
            2,
            true
        );

        // Act
        var result = _controller.TestMapToDto(bodyPart);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bodyPart.Id.ToString(), result.Id);
        Assert.Equal(bodyPart.Value, result.Value);
        Assert.Null(result.Description);
    }

    [Fact]
    public void MapToDto_DifferentReferenceDataTypes_WorksCorrectly()
    {
        // Test with different entity type (DifficultyLevel)
        var difficultyLevel = DifficultyLevel.Handler.Create(
            DifficultyLevelId.New(),
            "Beginner",
            "Suitable for beginners",
            1,
            true
        );

        // Act
        var result = _controller.TestMapToDto(difficultyLevel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(difficultyLevel.Id.ToString(), result.Id);
        Assert.Equal(difficultyLevel.Value, result.Value);
        Assert.Equal(difficultyLevel.Description, result.Description);
    }
}