using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class WorkoutCategoriesControllerTests
{
    private readonly Mock<IWorkoutCategoryService> _mockService;
    private readonly Mock<ILogger<WorkoutCategoriesController>> _mockLogger;
    private readonly WorkoutCategoriesController _controller;

    public WorkoutCategoriesControllerTests()
    {
        _mockService = new Mock<IWorkoutCategoryService>();
        _mockLogger = new Mock<ILogger<WorkoutCategoriesController>>();
        _controller = new WorkoutCategoriesController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllWorkoutCategories_ReturnsOkResultWithCategories()
    {
        // Arrange
        var categories = new List<WorkoutCategoryDto>
        {
            WorkoutCategoryDtoTestBuilder.UpperBodyPush().Build(),
            WorkoutCategoryDtoTestBuilder.UpperBodyPull().Build()
        };
        _mockService.Setup(s => s.GetAllAsDtosAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.GetAllWorkoutCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategories = Assert.IsAssignableFrom<IEnumerable<WorkoutCategoryDto>>(okResult.Value);
        Assert.Equal(2, returnedCategories.Count());
        _mockService.Verify(s => s.GetAllAsDtosAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutCategoryById_ValidId_ReturnsOkResultWithCategory()
    {
        // Arrange
        var id = TestIds.WorkoutCategoryIds.UpperBodyPush;
        var category = WorkoutCategoryDtoTestBuilder.UpperBodyPush().Build();
        _mockService.Setup(s => s.GetByIdAsDtoAsync(id)).ReturnsAsync(category);

        // Act
        var result = await _controller.GetWorkoutCategoryById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategory = Assert.IsType<WorkoutCategoryDto>(okResult.Value);
        Assert.Equal(id, returnedCategory.WorkoutCategoryId);
        Assert.Equal("Upper Body - Push", returnedCategory.Value);
        Assert.Equal("💪", returnedCategory.Icon);
        _mockService.Verify(s => s.GetByIdAsDtoAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutCategoryById_NotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var id = WorkoutCategoryId.From(TestIds.NotFoundId);
        _mockService.Setup(s => s.GetByIdAsDtoAsync(id.ToString())).ReturnsAsync((WorkoutCategoryDto?)null);

        // Act
        var result = await _controller.GetWorkoutCategoryById(id.ToString());

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = notFoundResult.Value;
        Assert.NotNull(response);
        _mockService.Verify(s => s.GetByIdAsDtoAsync(id.ToString()), Times.Once);
    }

    [Fact]
    public async Task GetAllWorkoutCategories_EmptyCategoryList_LogsCorrectCount()
    {
        // Arrange
        var categories = new List<WorkoutCategoryDto>();
        _mockService.Setup(s => s.GetAllAsDtosAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.GetAllWorkoutCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategories = Assert.IsAssignableFrom<IEnumerable<WorkoutCategoryDto>>(okResult.Value);
        Assert.Empty(returnedCategories);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieved 0 workout categories")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}