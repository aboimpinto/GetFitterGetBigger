using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Services.Stores;
using Microsoft.Extensions.Logging;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services.Stores
{
    public class WorkoutReferenceDataStoreTests
    {
        private readonly Mock<IGenericReferenceDataService> _genericReferenceDataServiceMock;
        private readonly Mock<IWorkoutReferenceDataService> _workoutReferenceDataServiceMock;
        private readonly Mock<ILogger<WorkoutReferenceDataStore>> _loggerMock;
        private readonly WorkoutReferenceDataStore _store;

        public WorkoutReferenceDataStoreTests()
        {
            _genericReferenceDataServiceMock = new Mock<IGenericReferenceDataService>();
            _workoutReferenceDataServiceMock = new Mock<IWorkoutReferenceDataService>();
            _loggerMock = new Mock<ILogger<WorkoutReferenceDataStore>>();
            _store = new WorkoutReferenceDataStore(
                _genericReferenceDataServiceMock.Object,
                _workoutReferenceDataServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task LoadReferenceDataAsync_LoadsWorkoutStatesSuccessfully()
        {
            // Arrange
            var expectedStates = new List<ReferenceDataDto>
            {
                new() { Id = "state-1", Value = "DRAFT", Description = "Draft state" },
                new() { Id = "state-2", Value = "PRODUCTION", Description = "Production state" },
                new() { Id = "state-3", Value = "ARCHIVED", Description = "Archived state" }
            };

            _genericReferenceDataServiceMock.Setup(x => x.GetReferenceDataAsync<WorkoutStates>())
                .ReturnsAsync(expectedStates);

            // Setup other required methods
            _workoutReferenceDataServiceMock.Setup(x => x.GetWorkoutCategoriesAsync())
                .ReturnsAsync(new List<WorkoutCategoryDto>());
            _genericReferenceDataServiceMock.Setup(x => x.GetReferenceDataAsync<DifficultyLevels>())
                .ReturnsAsync(new List<ReferenceDataDto>());
            _workoutReferenceDataServiceMock.Setup(x => x.GetWorkoutObjectivesAsync())
                .ReturnsAsync(new List<ReferenceDataDto>());

            // Act
            await _store.LoadReferenceDataAsync();

            // Assert
            _store.IsLoaded.Should().BeTrue();
            _store.WorkoutStates.Should().HaveCount(3);
            _store.WorkoutStates.Should().BeEquivalentTo(expectedStates);
            
            _genericReferenceDataServiceMock.Verify(x => x.GetReferenceDataAsync<WorkoutStates>(), Times.Once);
        }

        [Fact]
        public async Task LoadReferenceDataAsync_WhenGetWorkoutStatesFails_SetsErrorMessage()
        {
            // Arrange
            _genericReferenceDataServiceMock.Setup(x => x.GetReferenceDataAsync<WorkoutStates>())
                .ThrowsAsync(new Exception("Failed to get workout states"));

            // Setup other methods to succeed
            _workoutReferenceDataServiceMock.Setup(x => x.GetWorkoutCategoriesAsync())
                .ReturnsAsync(new List<WorkoutCategoryDto>());
            _genericReferenceDataServiceMock.Setup(x => x.GetReferenceDataAsync<DifficultyLevels>())
                .ReturnsAsync(new List<ReferenceDataDto>());
            _workoutReferenceDataServiceMock.Setup(x => x.GetWorkoutObjectivesAsync())
                .ReturnsAsync(new List<ReferenceDataDto>());

            // Act
            await _store.LoadReferenceDataAsync();

            // Assert
            _store.IsLoaded.Should().BeFalse();
            _store.ErrorMessage.Should().Contain("Failed to load reference data");
            _store.WorkoutStates.Should().BeEmpty();
        }
    }
}