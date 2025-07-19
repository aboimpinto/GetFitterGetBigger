using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Moq;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Services;

public class WorkoutReferenceDataStateServiceTests
{
    private readonly Mock<IWorkoutReferenceDataService> _mockService;
    private readonly WorkoutReferenceDataStateService _stateService;
    private int _stateChangeCount;

    public WorkoutReferenceDataStateServiceTests()
    {
        _mockService = new Mock<IWorkoutReferenceDataService>();
        _stateService = new WorkoutReferenceDataStateService(_mockService.Object);
        _stateService.OnChange += () => _stateChangeCount++;
    }

    #region Workout Objectives Tests

    [Fact]
    public async Task LoadWorkoutObjectivesAsync_WhenSuccessful_UpdatesStateCorrectly()
    {
        // Arrange
        var objectives = new List<ReferenceDataDto>
        {
            new() { Id = "1", Value = "Weight Loss", Description = "Lose weight effectively" },
            new() { Id = "2", Value = "Muscle Gain", Description = "Build muscle mass" }
        };
        _mockService.Setup(x => x.GetWorkoutObjectivesAsync()).ReturnsAsync(objectives);

        // Act
        await _stateService.LoadWorkoutObjectivesAsync();

        // Assert
        Assert.Equal(2, _stateService.WorkoutObjectives.Count());
        Assert.Equal("Muscle Gain", _stateService.WorkoutObjectives.First().Value); // Should be ordered
        Assert.False(_stateService.IsLoadingObjectives);
        Assert.Null(_stateService.ObjectivesError);
        Assert.Equal(2, _stateChangeCount); // Once for loading start, once for completion
    }

    [Fact]
    public async Task LoadWorkoutObjectivesAsync_WhenFails_SetsErrorMessage()
    {
        // Arrange
        _mockService.Setup(x => x.GetWorkoutObjectivesAsync()).ThrowsAsync(new Exception("API error"));

        // Act
        await _stateService.LoadWorkoutObjectivesAsync();

        // Assert
        Assert.Empty(_stateService.WorkoutObjectives);
        Assert.False(_stateService.IsLoadingObjectives);
        Assert.Equal("Failed to load workout objectives: API error", _stateService.ObjectivesError);
        Assert.Equal(2, _stateChangeCount);
    }

    [Fact]
    public void FilteredWorkoutObjectives_WithSearchTerm_FiltersCorrectly()
    {
        // Arrange
        var objectives = new List<ReferenceDataDto>
        {
            new() { Id = "1", Value = "Weight Loss", Description = "Lose weight effectively" },
            new() { Id = "2", Value = "Muscle Gain", Description = "Build muscle mass" },
            new() { Id = "3", Value = "Endurance", Description = "Improve cardiovascular fitness" }
        };
        _mockService.Setup(x => x.GetWorkoutObjectivesAsync()).ReturnsAsync(objectives);

        // Act
        _stateService.LoadWorkoutObjectivesAsync().Wait();
        _stateService.ObjectivesSearchTerm = "muscle";

        // Assert
        var filtered = _stateService.FilteredWorkoutObjectives.ToList();
        Assert.Single(filtered);
        Assert.Equal("Muscle Gain", filtered[0].Value);
    }

    [Fact]
    public void SelectObjective_UpdatesSelectedObjectiveAndNotifies()
    {
        // Arrange
        var objective = new ReferenceDataDto { Id = "1", Value = "Test" };
        _stateChangeCount = 0;

        // Act
        _stateService.SelectObjective(objective);

        // Assert
        Assert.Equal(objective, _stateService.SelectedObjective);
        Assert.Equal(1, _stateChangeCount);
    }

    [Fact]
    public void ClearObjectivesError_ClearsErrorAndNotifies()
    {
        // Arrange
        _mockService.Setup(x => x.GetWorkoutObjectivesAsync()).ThrowsAsync(new Exception("Error"));
        _stateService.LoadWorkoutObjectivesAsync().Wait();
        _stateChangeCount = 0;

        // Act
        _stateService.ClearObjectivesError();

        // Assert
        Assert.Null(_stateService.ObjectivesError);
        Assert.Equal(1, _stateChangeCount);
    }

    #endregion

    #region Workout Categories Tests

    [Fact]
    public async Task LoadWorkoutCategoriesAsync_WhenSuccessful_UpdatesStateCorrectly()
    {
        // Arrange
        var categories = new List<WorkoutCategoryDto>
        {
            new() { WorkoutCategoryId = "1", Value = "Upper Body", Icon = "icon1", Color = "#FF0000", PrimaryMuscleGroups = "Chest, Back", Description = null, DisplayOrder = 1, IsActive = true },
            new() { WorkoutCategoryId = "2", Value = "Lower Body", Icon = "icon2", Color = "#00FF00", PrimaryMuscleGroups = "Legs", Description = null, DisplayOrder = 2, IsActive = true }
        };
        _mockService.Setup(x => x.GetWorkoutCategoriesAsync()).ReturnsAsync(categories);

        // Act
        await _stateService.LoadWorkoutCategoriesAsync();

        // Assert
        Assert.Equal(2, _stateService.WorkoutCategories.Count());
        Assert.Equal("Lower Body", _stateService.WorkoutCategories.First().Value); // Should be ordered
        Assert.False(_stateService.IsLoadingCategories);
        Assert.Null(_stateService.CategoriesError);
    }

    [Fact]
    public void FilteredWorkoutCategories_WithMuscleGroupSearch_FiltersCorrectly()
    {
        // Arrange
        var categories = new List<WorkoutCategoryDto>
        {
            new() { WorkoutCategoryId = "1", Value = "Upper Body", Icon = "icon1", Color = "#FF0000", PrimaryMuscleGroups = "Chest, Back", Description = null, DisplayOrder = 1, IsActive = true },
            new() { WorkoutCategoryId = "2", Value = "Lower Body", Icon = "icon2", Color = "#00FF00", PrimaryMuscleGroups = "Legs, Glutes", Description = null, DisplayOrder = 2, IsActive = true }
        };
        _mockService.Setup(x => x.GetWorkoutCategoriesAsync()).ReturnsAsync(categories);

        // Act
        _stateService.LoadWorkoutCategoriesAsync().Wait();
        _stateService.CategoriesSearchTerm = "chest";

        // Assert
        var filtered = _stateService.FilteredWorkoutCategories.ToList();
        Assert.Single(filtered);
        Assert.Equal("Upper Body", filtered[0].Value);
    }

    #endregion

    #region Execution Protocols Tests

    [Fact]
    public async Task LoadExecutionProtocolsAsync_WhenSuccessful_UpdatesStateCorrectly()
    {
        // Arrange
        var protocols = new List<ExecutionProtocolDto>
        {
            new() { ExecutionProtocolId = "1", Value = "Standard", Code = "STD", TimeBase = true, RepBase = true, RestPattern = "60s", IntensityLevel = "Medium", Description = null, DisplayOrder = 1, IsActive = true },
            new() { ExecutionProtocolId = "2", Value = "Advanced", Code = "ADV", TimeBase = true, RepBase = true, RestPattern = "90s", IntensityLevel = "High", Description = null, DisplayOrder = 2, IsActive = true }
        };
        _mockService.Setup(x => x.GetExecutionProtocolsAsync()).ReturnsAsync(protocols);

        // Act
        await _stateService.LoadExecutionProtocolsAsync();

        // Assert
        Assert.Equal(2, _stateService.ExecutionProtocols.Count());
        Assert.Equal("ADV", _stateService.ExecutionProtocols.First().Code); // Should be ordered by code
        Assert.False(_stateService.IsLoadingProtocols);
        Assert.Null(_stateService.ProtocolsError);
    }

    [Fact]
    public void FilteredExecutionProtocols_WithIntensityFilter_FiltersCorrectly()
    {
        // Arrange
        var protocols = new List<ExecutionProtocolDto>
        {
            new() { ExecutionProtocolId = "1", Value = "Standard", Code = "STD", IntensityLevel = "Medium", TimeBase = false, RepBase = false, RestPattern = null, Description = null, DisplayOrder = 1, IsActive = true },
            new() { ExecutionProtocolId = "2", Value = "Advanced", Code = "ADV", IntensityLevel = "High", TimeBase = false, RepBase = false, RestPattern = null, Description = null, DisplayOrder = 2, IsActive = true },
            new() { ExecutionProtocolId = "3", Value = "Beginner", Code = "BEG", IntensityLevel = "Low", TimeBase = false, RepBase = false, RestPattern = null, Description = null, DisplayOrder = 3, IsActive = true }
        };
        _mockService.Setup(x => x.GetExecutionProtocolsAsync()).ReturnsAsync(protocols);

        // Act
        _stateService.LoadExecutionProtocolsAsync().Wait();
        _stateService.SelectedIntensityLevel = "High";

        // Assert
        var filtered = _stateService.FilteredExecutionProtocols.ToList();
        Assert.Single(filtered);
        Assert.Equal("Advanced", filtered[0].Value);
    }

    [Fact]
    public void FilteredExecutionProtocols_WithSearchAndIntensity_AppliesBothFilters()
    {
        // Arrange
        var protocols = new List<ExecutionProtocolDto>
        {
            new() { ExecutionProtocolId = "1", Value = "Standard Training", Code = "STD", IntensityLevel = "Medium", TimeBase = false, RepBase = false, RestPattern = null, Description = null, DisplayOrder = 1, IsActive = true },
            new() { ExecutionProtocolId = "2", Value = "Advanced Training", Code = "ADV", IntensityLevel = "High", TimeBase = false, RepBase = false, RestPattern = null, Description = null, DisplayOrder = 2, IsActive = true },
            new() { ExecutionProtocolId = "3", Value = "Advanced Cardio", Code = "AC", IntensityLevel = "High", TimeBase = false, RepBase = false, RestPattern = null, Description = null, DisplayOrder = 3, IsActive = true }
        };
        _mockService.Setup(x => x.GetExecutionProtocolsAsync()).ReturnsAsync(protocols);

        // Act
        _stateService.LoadExecutionProtocolsAsync().Wait();
        _stateService.SelectedIntensityLevel = "High";
        _stateService.ProtocolsSearchTerm = "Training";

        // Assert
        var filtered = _stateService.FilteredExecutionProtocols.ToList();
        Assert.Single(filtered);
        Assert.Equal("Advanced Training", filtered[0].Value);
    }

    #endregion

    #region State Change Tests

    [Fact]
    public void SearchTermSetters_TriggerStateChange()
    {
        // Arrange
        _stateChangeCount = 0;

        // Act & Assert
        _stateService.ObjectivesSearchTerm = "test";
        Assert.Equal(1, _stateChangeCount);

        _stateService.CategoriesSearchTerm = "test";
        Assert.Equal(2, _stateChangeCount);

        _stateService.ProtocolsSearchTerm = "test";
        Assert.Equal(3, _stateChangeCount);

        _stateService.SelectedIntensityLevel = "High";
        Assert.Equal(4, _stateChangeCount);
    }

    #endregion
}