using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using GetFitterGetBigger.API.Tests.TestConstants;
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
    public async Task GetWorkoutCategories_ReturnsOkResultWithCategories()
    {
        // Arrange
        var categories = new List<WorkoutCategoryDto>
        {
            WorkoutCategoryDtoTestBuilder.UpperBodyPush().Build(),
            WorkoutCategoryDtoTestBuilder.UpperBodyPull().Build()
        };
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(ServiceResult<IEnumerable<WorkoutCategoryDto>>.Success(categories));

        // Act
        var result = await _controller.GetWorkoutCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<WorkoutCategoriesResponseDto>(okResult.Value);
        Assert.Equal(2, response.WorkoutCategories.Count());
        _mockService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutCategoryById_ValidId_ReturnsOkResultWithCategory()
    {
        // Arrange
        var id = TestIds.WorkoutCategoryIds.UpperBodyPush.ToString();
        var category = WorkoutCategoryDtoTestBuilder.UpperBodyPush().Build();
        _mockService.Setup(s => s.GetByIdAsync(WorkoutCategoryId.ParseOrEmpty(id)))
            .ReturnsAsync(ServiceResult<WorkoutCategoryDto>.Success(category));

        // Act
        var result = await _controller.GetWorkoutCategoryById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategory = Assert.IsType<WorkoutCategoryDto>(okResult.Value);
        Assert.Equal(id, returnedCategory.WorkoutCategoryId);
        Assert.Equal(category.Value, returnedCategory.Value);
        Assert.Equal(category.Icon, returnedCategory.Icon);
        _mockService.Verify(s => s.GetByIdAsync(It.IsAny<WorkoutCategoryId>()), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutCategoryById_NotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var id = WorkoutCategoryId.ParseOrEmpty($"workoutcategory-{TestIds.NotFoundId}").ToString();
        var emptyDto = CreateEmptyDto();
        _mockService.Setup(s => s.GetByIdAsync(It.IsAny<WorkoutCategoryId>()))
            .ReturnsAsync(ServiceResult<WorkoutCategoryDto>.Failure(emptyDto, ServiceError.NotFound(WorkoutCategoryTestConstants.ErrorMessages.NotFound)));

        // Act
        var result = await _controller.GetWorkoutCategoryById(id);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockService.Verify(s => s.GetByIdAsync(It.IsAny<WorkoutCategoryId>()), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutCategories_EmptyCategoryList_ReturnsEmptyArray()
    {
        // Arrange
        var categories = new List<WorkoutCategoryDto>();
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(ServiceResult<IEnumerable<WorkoutCategoryDto>>.Success(categories));

        // Act
        var result = await _controller.GetWorkoutCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<WorkoutCategoriesResponseDto>(okResult.Value);
        Assert.Empty(response.WorkoutCategories);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting all active workout categories")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWorkoutCategoryById_InvalidIdFormat_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = "invalid-id";
        var emptyDto = CreateEmptyDto();
        _mockService.Setup(s => s.GetByIdAsync(WorkoutCategoryId.Empty))
            .ReturnsAsync(ServiceResult<WorkoutCategoryDto>.Failure(emptyDto, ServiceError.ValidationFailed(WorkoutCategoryTestConstants.ErrorMessages.InvalidIdFormat)));

        // Act
        var result = await _controller.GetWorkoutCategoryById(invalidId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        _mockService.Verify(s => s.GetByIdAsync(WorkoutCategoryId.Empty), Times.Once);
    }
    
    [Fact]
    public async Task GetWorkoutCategoryByValue_ValidValue_ReturnsOkResultWithCategory()
    {
        // Arrange
        var value = "Upper Body - Push";
        var category = WorkoutCategoryDtoTestBuilder.UpperBodyPush().Build();
        _mockService.Setup(s => s.GetByValueAsync(value))
            .ReturnsAsync(ServiceResult<WorkoutCategoryDto>.Success(category));

        // Act
        var result = await _controller.GetWorkoutCategoryByValue(value);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategory = Assert.IsType<WorkoutCategoryDto>(okResult.Value);
        Assert.Equal(value, returnedCategory.Value);
        _mockService.Verify(s => s.GetByValueAsync(value), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutCategoryByValue_NotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var value = "NonExistent";
        var emptyDto = CreateEmptyDto();
        _mockService.Setup(s => s.GetByValueAsync(value))
            .ReturnsAsync(ServiceResult<WorkoutCategoryDto>.Failure(emptyDto, ServiceError.NotFound(WorkoutCategoryTestConstants.ErrorMessages.NotFound)));

        // Act
        var result = await _controller.GetWorkoutCategoryByValue(value);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockService.Verify(s => s.GetByValueAsync(value), Times.Once);
    }
    
    private static WorkoutCategoryDto CreateEmptyDto() => new()
    {
        WorkoutCategoryId = WorkoutCategoryTestConstants.Defaults.EmptyId,
        Value = WorkoutCategoryTestConstants.Defaults.EmptyValue,
        Icon = WorkoutCategoryTestConstants.Defaults.EmptyIcon,
        Color = WorkoutCategoryTestConstants.Defaults.DefaultColor,
        DisplayOrder = WorkoutCategoryTestConstants.Defaults.DefaultDisplayOrder,
        IsActive = WorkoutCategoryTestConstants.Defaults.DefaultIsActive
    };
}