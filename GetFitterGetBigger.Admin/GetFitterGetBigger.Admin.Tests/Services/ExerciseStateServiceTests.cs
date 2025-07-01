using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Tests.Builders;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ExerciseStateServiceTests
    {
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly Mock<IReferenceDataService> _referenceDataServiceMock;
        private readonly ExerciseStateService _stateService;

        public ExerciseStateServiceTests()
        {
            _exerciseServiceMock = new Mock<IExerciseService>();
            _referenceDataServiceMock = new Mock<IReferenceDataService>();
            _stateService = new ExerciseStateService(_exerciseServiceMock.Object, _referenceDataServiceMock.Object);
        }

        [Fact]
        public async Task InitializeAsync_LoadsReferenceDataAndExercises()
        {
            // Arrange
            var difficultyLevels = ReferenceDataDtoBuilder.BuildList(3);
            var muscleGroups = ReferenceDataDtoBuilder.BuildList(5);
            var exercises = new ExercisePagedResultDto { Items = new List<ExerciseListDto>() };

            _referenceDataServiceMock.Setup(x => x.GetDifficultyLevelsAsync()).ReturnsAsync(difficultyLevels);
            _referenceDataServiceMock.Setup(x => x.GetMuscleGroupsAsync()).ReturnsAsync(muscleGroups);
            _referenceDataServiceMock.Setup(x => x.GetMuscleRolesAsync()).ReturnsAsync(ReferenceDataDtoBuilder.BuildList(3));
            _referenceDataServiceMock.Setup(x => x.GetEquipmentAsync()).ReturnsAsync(ReferenceDataDtoBuilder.BuildList(4));
            _referenceDataServiceMock.Setup(x => x.GetBodyPartsAsync()).ReturnsAsync(ReferenceDataDtoBuilder.BuildList(2));
            _referenceDataServiceMock.Setup(x => x.GetMovementPatternsAsync()).ReturnsAsync(ReferenceDataDtoBuilder.BuildList(6));
            _referenceDataServiceMock.Setup(x => x.GetExerciseTypesAsync()).ReturnsAsync(new List<ExerciseTypeDto>
            {
                new() { Id = "1", Value = "Warmup", Description = "Warmup exercises" },
                new() { Id = "2", Value = "Workout", Description = "Main workout exercises" },
                new() { Id = "3", Value = "Cooldown", Description = "Cooldown exercises" },
                new() { Id = "4", Value = "Rest", Description = "Rest periods" }
            });
            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>())).ReturnsAsync(exercises);

            // Act
            await _stateService.InitializeAsync();

            // Assert
            _stateService.DifficultyLevels.Should().HaveCount(3);
            _stateService.MuscleGroups.Should().HaveCount(5);
            _stateService.ExerciseTypes.Should().HaveCount(4);
            _stateService.CurrentPage.Should().NotBeNull();
            _stateService.IsLoadingReferenceData.Should().BeFalse();
            _stateService.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task StateService_HandlesCoachNotesAndExerciseTypes_Correctly()
        {
            // Arrange
            var exerciseWithCoachNotes = new ExerciseListDtoBuilder()
                .WithId("exercise-1")
                .WithName("Test Exercise")
                .WithCoachNotes(
                    ("Step 1: Setup", 0),
                    ("Step 2: Execute", 1),
                    ("Step 3: Reset", 2))
                .WithExerciseTypes(
                    ("Warmup", "Warmup description"),
                    ("Workout", "Main workout"))
                .Build();

            var exercises = new ExercisePagedResultDto
            {
                Items = new List<ExerciseListDto> { exerciseWithCoachNotes }
            };

            var exerciseTypes = new List<ExerciseTypeDto>
            {
                new() { Id = "1", Value = "Warmup", Description = "Warmup exercises" },
                new() { Id = "2", Value = "Workout", Description = "Main workout exercises" },
                new() { Id = "3", Value = "Cooldown", Description = "Cooldown exercises" },
                new() { Id = "4", Value = "Rest", Description = "Rest periods" }
            };

            _referenceDataServiceMock.Setup(x => x.GetDifficultyLevelsAsync()).ReturnsAsync(ReferenceDataDtoBuilder.BuildList(3));
            _referenceDataServiceMock.Setup(x => x.GetMuscleGroupsAsync()).ReturnsAsync(ReferenceDataDtoBuilder.BuildList(5));
            _referenceDataServiceMock.Setup(x => x.GetMuscleRolesAsync()).ReturnsAsync(ReferenceDataDtoBuilder.BuildList(3));
            _referenceDataServiceMock.Setup(x => x.GetEquipmentAsync()).ReturnsAsync(ReferenceDataDtoBuilder.BuildList(4));
            _referenceDataServiceMock.Setup(x => x.GetBodyPartsAsync()).ReturnsAsync(ReferenceDataDtoBuilder.BuildList(2));
            _referenceDataServiceMock.Setup(x => x.GetMovementPatternsAsync()).ReturnsAsync(ReferenceDataDtoBuilder.BuildList(6));
            _referenceDataServiceMock.Setup(x => x.GetExerciseTypesAsync()).ReturnsAsync(exerciseTypes);
            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>())).ReturnsAsync(exercises);

            // Act
            await _stateService.InitializeAsync();

            // Assert
            _stateService.ExerciseTypes.Should().HaveCount(4);
            _stateService.CurrentPage.Should().NotBeNull();
            _stateService.CurrentPage!.Items.Should().HaveCount(1);

            var loadedExercise = _stateService.CurrentPage.Items.First();
            loadedExercise.CoachNotes.Should().HaveCount(3);
            loadedExercise.CoachNotes.Should().Contain(cn => cn.Text == "Step 1: Setup" && cn.Order == 0);
            loadedExercise.CoachNotes.Should().Contain(cn => cn.Text == "Step 2: Execute" && cn.Order == 1);
            loadedExercise.CoachNotes.Should().Contain(cn => cn.Text == "Step 3: Reset" && cn.Order == 2);

            loadedExercise.ExerciseTypes.Should().HaveCount(2);
            loadedExercise.ExerciseTypes.Should().Contain(et => et.Value == "Warmup");
            loadedExercise.ExerciseTypes.Should().Contain(et => et.Value == "Workout");
        }

        [Fact]
        public async Task StateService_HandlesIsActiveFiltering_Correctly()
        {
            // Arrange
            var filter = new ExerciseFilterDto { IsActive = true };
            var exercises = new ExercisePagedResultDto { Items = new List<ExerciseListDto>() };

            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.Is<ExerciseFilterDto>(f => f.IsActive == true)))
                .ReturnsAsync(exercises);

            // Act
            await _stateService.LoadExercisesAsync(filter);

            // Assert
            _stateService.CurrentFilter.IsActive.Should().Be(true);
            _exerciseServiceMock.Verify(x => x.GetExercisesAsync(It.Is<ExerciseFilterDto>(f => f.IsActive == true)), Times.Once);
        }

        [Fact]
        public async Task LoadExercisesAsync_UpdatesCurrentPageAndNotifiesChange()
        {
            // Arrange
            var exercises = new ExercisePagedResultDtoBuilder()
                .WithItems(
                    new ExerciseListDtoBuilder()
                        .WithName("Test Exercise")
                        .WithDifficulty("Intermediate", "1")
                        .Build()
                )
                .WithTotalCount(1)
                .Build();
            var changeNotified = false;
            _stateService.OnChange += () => changeNotified = true;

            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>())).ReturnsAsync(exercises);

            // Act
            await _stateService.LoadExercisesAsync();

            // Assert
            _stateService.CurrentPage.Should().NotBeNull();
            _stateService.CurrentPage!.Items.Should().HaveCount(1);
            _stateService.ErrorMessage.Should().BeNull();
            changeNotified.Should().BeTrue();
        }

        [Fact]
        public async Task LoadExercisesAsync_WithFilter_UpdatesCurrentFilter()
        {
            // Arrange
            var filter = new ExerciseFilterDto { Name = "squat", Page = 2 };
            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(new ExercisePagedResultDto());

            // Act
            await _stateService.LoadExercisesAsync(filter);

            // Assert
            _stateService.CurrentFilter.Should().Be(filter);
            _stateService.CurrentFilter.Name.Should().Be("squat");
            _stateService.CurrentFilter.Page.Should().Be(2);
        }

        [Fact]
        public async Task LoadExercisesAsync_OnError_SetsErrorMessage()
        {
            // Arrange
            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ThrowsAsync(new Exception("API Error"));

            // Act
            await _stateService.LoadExercisesAsync();

            // Assert
            _stateService.ErrorMessage.Should().Contain("Failed to load exercises");
            _stateService.ErrorMessage.Should().Contain("API Error");
            _stateService.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task LoadExerciseByIdAsync_LoadsSelectedExercise()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var exercise = new ExerciseDtoBuilder()
                .WithId(exerciseId)
                .WithName("Test Exercise")
                .WithDescription(string.Empty)
                .WithInstructions(string.Empty)
                .Build();
            _exerciseServiceMock.Setup(x => x.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);

            // Act
            await _stateService.LoadExerciseByIdAsync(exerciseId);

            // Assert
            _stateService.SelectedExercise.Should().NotBeNull();
            _stateService.SelectedExercise!.Id.Should().Be(exerciseId);
            _stateService.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task LoadExerciseByIdAsync_WhenNotFound_SetsErrorMessage()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            _exerciseServiceMock.Setup(x => x.GetExerciseByIdAsync(exerciseId)).ReturnsAsync((ExerciseDto?)null);

            // Act
            await _stateService.LoadExerciseByIdAsync(exerciseId);

            // Assert
            _stateService.SelectedExercise.Should().BeNull();
            _stateService.ErrorMessage.Should().Be("Exercise not found");
        }

        [Fact]
        public async Task CreateExerciseAsync_CreatesAndRefreshesPage()
        {
            // Arrange
            var createDto = new ExerciseCreateDtoBuilder()
                .WithName("New Exercise")
                .Build();
            var createdExercise = new ExerciseDtoBuilder()
                .WithName("New Exercise")
                .WithDescription(string.Empty)
                .WithInstructions(string.Empty)
                .Build();

            _exerciseServiceMock.Setup(x => x.CreateExerciseAsync(createDto)).ReturnsAsync(createdExercise);
            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(new ExercisePagedResultDto());

            // Act
            await _stateService.CreateExerciseAsync(createDto);

            // Assert
            _exerciseServiceMock.Verify(x => x.CreateExerciseAsync(createDto), Times.Once);
            _exerciseServiceMock.Verify(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()), Times.Once);
            _stateService.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task UpdateExerciseAsync_UpdatesClearsSelectedAndRefreshes()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var updateDto = new ExerciseUpdateDtoBuilder()
                .WithName("Updated Exercise")
                .Build();

            // Load an exercise first to set SelectedExercise
            _exerciseServiceMock.Setup(x => x.GetExerciseByIdAsync(exerciseId))
                .ReturnsAsync(new ExerciseDtoBuilder()
                    .WithId(exerciseId)
                    .WithName(string.Empty)
                    .WithDescription(string.Empty)
                    .WithInstructions(string.Empty)
                    .Build());
            await _stateService.LoadExerciseByIdAsync(exerciseId);

            _exerciseServiceMock.Setup(x => x.UpdateExerciseAsync(exerciseId, updateDto)).Returns(Task.CompletedTask);
            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(new ExercisePagedResultDto());

            // Act
            await _stateService.UpdateExerciseAsync(exerciseId, updateDto);

            // Assert
            _stateService.SelectedExercise.Should().BeNull();
            _exerciseServiceMock.Verify(x => x.UpdateExerciseAsync(exerciseId, updateDto), Times.Once);
            _exerciseServiceMock.Verify(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()), Times.Once);
        }

        [Fact]
        public async Task DeleteExerciseAsync_DeletesAndRefreshes()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();

            // Load an exercise first to set SelectedExercise
            _exerciseServiceMock.Setup(x => x.GetExerciseByIdAsync(exerciseId))
                .ReturnsAsync(new ExerciseDtoBuilder()
                    .WithId(exerciseId)
                    .WithName(string.Empty)
                    .WithDescription(string.Empty)
                    .WithInstructions(string.Empty)
                    .Build());
            await _stateService.LoadExerciseByIdAsync(exerciseId);

            _exerciseServiceMock.Setup(x => x.DeleteExerciseAsync(exerciseId)).Returns(Task.CompletedTask);
            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(new ExercisePagedResultDto());

            // Act
            await _stateService.DeleteExerciseAsync(exerciseId);

            // Assert
            _stateService.SelectedExercise.Should().BeNull();
            _exerciseServiceMock.Verify(x => x.DeleteExerciseAsync(exerciseId), Times.Once);
            _exerciseServiceMock.Verify(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()), Times.Once);
        }

        [Fact]
        public async Task ClearSelectedExercise_ClearsSelectedAndNotifies()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            _exerciseServiceMock.Setup(x => x.GetExerciseByIdAsync(exerciseId))
                .ReturnsAsync(new ExerciseDtoBuilder()
                    .WithId(exerciseId)
                    .WithName(string.Empty)
                    .WithDescription(string.Empty)
                    .WithInstructions(string.Empty)
                    .Build());
            await _stateService.LoadExerciseByIdAsync(exerciseId);

            var changeNotified = false;
            _stateService.OnChange += () => changeNotified = true;

            // Act
            _stateService.ClearSelectedExercise();

            // Assert
            _stateService.SelectedExercise.Should().BeNull();
            changeNotified.Should().BeTrue();
        }

        [Fact]
        public async Task ClearError_ClearsErrorAndNotifies()
        {
            // Arrange
            // Trigger an error first
            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ThrowsAsync(new Exception("Test error"));
            await _stateService.LoadExercisesAsync();

            var changeNotified = false;
            _stateService.OnChange += () => changeNotified = true;

            // Act
            _stateService.ClearError();

            // Assert
            _stateService.ErrorMessage.Should().BeNull();
            changeNotified.Should().BeTrue();
        }

        [Fact]
        public async Task InitializeAsync_WhenReferenceDataFails_StillLoadsExercises()
        {
            // Arrange
            _referenceDataServiceMock.Setup(x => x.GetDifficultyLevelsAsync())
                .ThrowsAsync(new Exception("Reference data error"));
            _referenceDataServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(ReferenceDataDtoBuilder.BuildList(1));
            _referenceDataServiceMock.Setup(x => x.GetMuscleRolesAsync())
                .ReturnsAsync(ReferenceDataDtoBuilder.BuildList(1));
            _referenceDataServiceMock.Setup(x => x.GetEquipmentAsync())
                .ReturnsAsync(ReferenceDataDtoBuilder.BuildList(1));
            _referenceDataServiceMock.Setup(x => x.GetBodyPartsAsync())
                .ReturnsAsync(ReferenceDataDtoBuilder.BuildList(1));
            _referenceDataServiceMock.Setup(x => x.GetMovementPatternsAsync())
                .ReturnsAsync(ReferenceDataDtoBuilder.BuildList(1));

            var exercises = new ExercisePagedResultDto { Items = new List<ExerciseListDto>() };
            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(exercises);

            // Act
            await _stateService.InitializeAsync();

            // Assert
            _stateService.IsLoadingReferenceData.Should().BeFalse();
            _stateService.CurrentPage.Should().NotBeNull();
            _exerciseServiceMock.Verify(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()), Times.Once);
        }
    }
}