using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
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
            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>())).ReturnsAsync(exercises);

            // Act
            await _stateService.InitializeAsync();

            // Assert
            _stateService.DifficultyLevels.Should().HaveCount(3);
            _stateService.MuscleGroups.Should().HaveCount(5);
            _stateService.CurrentPage.Should().NotBeNull();
            _stateService.IsLoadingReferenceData.Should().BeFalse();
            _stateService.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task LoadExercisesAsync_UpdatesCurrentPageAndNotifiesChange()
        {
            // Arrange
            var exercises = new ExercisePagedResultDto 
            { 
                Items = new List<ExerciseListDto> 
                { 
                    new() { 
                        Id = Guid.NewGuid().ToString(), 
                        Name = "Test Exercise",
                        Difficulty = new ReferenceDataDto { Id = "1", Value = "Intermediate", Description = "Intermediate level" },
                        MuscleGroups = new List<MuscleGroupListItemDto>(),
                        Equipment = new List<ReferenceDataDto>(),
                        MovementPatterns = new List<ReferenceDataDto>(),
                        BodyParts = new List<ReferenceDataDto>()
                    } 
                },
                TotalCount = 1
            };
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
            var exercise = new ExerciseDto { 
                Id = exerciseId, 
                Name = "Test Exercise",
                Description = string.Empty,
                Instructions = string.Empty,
                Difficulty = new ReferenceDataDto { Id = "1", Value = "Intermediate", Description = "Intermediate level" },
                MuscleGroups = new List<MuscleGroupWithRoleDto>(),
                Equipment = new List<ReferenceDataDto>(),
                BodyParts = new List<ReferenceDataDto>(),
                MovementPatterns = new List<ReferenceDataDto>()
            };
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
            var createDto = new ExerciseCreateDto { Name = "New Exercise" };
            var createdExercise = new ExerciseDto { 
                Id = Guid.NewGuid().ToString(), 
                Name = "New Exercise",
                Description = string.Empty,
                Instructions = string.Empty,
                Difficulty = new ReferenceDataDto { Id = "1", Value = "Intermediate", Description = "Intermediate level" },
                MuscleGroups = new List<MuscleGroupWithRoleDto>(),
                Equipment = new List<ReferenceDataDto>(),
                BodyParts = new List<ReferenceDataDto>(),
                MovementPatterns = new List<ReferenceDataDto>()
            };
            
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
            var updateDto = new ExerciseUpdateDto { Name = "Updated Exercise" };
            
            // Load an exercise first to set SelectedExercise
            _exerciseServiceMock.Setup(x => x.GetExerciseByIdAsync(exerciseId))
                .ReturnsAsync(new ExerciseDto { 
                    Id = exerciseId,
                    Name = string.Empty,
                    Description = string.Empty,
                    Instructions = string.Empty,
                    Difficulty = new ReferenceDataDto { Id = "1", Value = "Intermediate", Description = "Intermediate level" },
                    MuscleGroups = new List<MuscleGroupWithRoleDto>(),
                    Equipment = new List<ReferenceDataDto>(),
                    BodyParts = new List<ReferenceDataDto>(),
                    MovementPatterns = new List<ReferenceDataDto>()
                });
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
                .ReturnsAsync(new ExerciseDto { 
                    Id = exerciseId,
                    Name = string.Empty,
                    Description = string.Empty,
                    Instructions = string.Empty,
                    Difficulty = new ReferenceDataDto { Id = "1", Value = "Intermediate", Description = "Intermediate level" },
                    MuscleGroups = new List<MuscleGroupWithRoleDto>(),
                    Equipment = new List<ReferenceDataDto>(),
                    BodyParts = new List<ReferenceDataDto>(),
                    MovementPatterns = new List<ReferenceDataDto>()
                });
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
                .ReturnsAsync(new ExerciseDto { 
                    Id = exerciseId,
                    Name = string.Empty,
                    Description = string.Empty,
                    Instructions = string.Empty,
                    Difficulty = new ReferenceDataDto { Id = "1", Value = "Intermediate", Description = "Intermediate level" },
                    MuscleGroups = new List<MuscleGroupWithRoleDto>(),
                    Equipment = new List<ReferenceDataDto>(),
                    BodyParts = new List<ReferenceDataDto>(),
                    MovementPatterns = new List<ReferenceDataDto>()
                });
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