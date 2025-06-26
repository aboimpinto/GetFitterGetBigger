using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class ReferenceTablesBaseControllerTests
{
    // Test implementation of ReferenceTablesBaseController for testing
    private class TestReferenceController : ReferenceTablesBaseController
    {
        public TestReferenceController(IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider) 
            : base(unitOfWorkProvider)
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

    public ReferenceTablesBaseControllerTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _controller = new TestReferenceController(_mockUnitOfWorkProvider.Object);
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