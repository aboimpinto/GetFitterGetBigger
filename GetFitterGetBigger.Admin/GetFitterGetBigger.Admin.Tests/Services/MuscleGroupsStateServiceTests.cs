using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Services;
using Moq;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class MuscleGroupsStateServiceTests
    {
        private readonly Mock<IMuscleGroupsService> _muscleGroupsServiceMock;
        private readonly Mock<IGenericReferenceDataService> _referenceDataServiceMock;
        private readonly MuscleGroupsStateService _stateService;

        public MuscleGroupsStateServiceTests()
        {
            _muscleGroupsServiceMock = new Mock<IMuscleGroupsService>();
            _referenceDataServiceMock = new Mock<IGenericReferenceDataService>();
            _stateService = new MuscleGroupsStateService(
                _muscleGroupsServiceMock.Object,
                _referenceDataServiceMock.Object);
        }

        #region LoadMuscleGroupsAsync Tests

        [Fact]
        public async Task LoadMuscleGroupsAsync_ShouldLoadMuscleGroups_WhenSuccessful()
        {
            // Arrange
            var muscleGroups = new List<MuscleGroupDto>
            {
                new() { Id = "mg-1", Name = "Biceps", BodyPartId = "bp-1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "mg-2", Name = "Triceps", BodyPartId = "bp-1", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _muscleGroupsServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(muscleGroups);

            // Act
            await _stateService.LoadMuscleGroupsAsync();

            // Assert
            _stateService.MuscleGroups.Should().HaveCount(2);
            _stateService.IsLoading.Should().BeFalse();
            _stateService.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task LoadMuscleGroupsAsync_ShouldSetErrorMessage_WhenFails()
        {
            // Arrange
            _muscleGroupsServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            await _stateService.LoadMuscleGroupsAsync();

            // Assert
            _stateService.MuscleGroups.Should().BeEmpty();
            _stateService.IsLoading.Should().BeFalse();
            _stateService.ErrorMessage.Should().Contain("Failed to load muscle groups");
        }

        [Fact]
        public async Task LoadMuscleGroupsAsync_ShouldEnhanceWithBodyPartNames_WhenBodyPartsLoaded()
        {
            // Arrange
            var bodyParts = new List<ReferenceDataDto>
            {
                new() { Id = "bp-1", Value = "Arms" },
                new() { Id = "bp-2", Value = "Legs" }
            };
            var muscleGroups = new List<MuscleGroupDto>
            {
                new() { Id = "mg-1", Name = "Biceps", BodyPartId = "bp-1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "mg-2", Name = "Quadriceps", BodyPartId = "bp-2", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            _referenceDataServiceMock.Setup(x => x.GetReferenceDataAsync<BodyParts>())
                .ReturnsAsync(bodyParts);
            _muscleGroupsServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(muscleGroups);

            // Act
            await _stateService.LoadBodyPartsAsync();
            await _stateService.LoadMuscleGroupsAsync();

            // Assert
            _stateService.MuscleGroups.First(mg => mg.Id == "mg-1").BodyPartName.Should().Be("Arms");
            _stateService.MuscleGroups.First(mg => mg.Id == "mg-2").BodyPartName.Should().Be("Legs");
        }

        #endregion

        #region LoadBodyPartsAsync Tests

        [Fact]
        public async Task LoadBodyPartsAsync_ShouldLoadBodyParts_WhenSuccessful()
        {
            // Arrange
            var bodyParts = new List<ReferenceDataDto>
            {
                new() { Id = "bp-1", Value = "Arms" },
                new() { Id = "bp-2", Value = "Legs" }
            };
            _referenceDataServiceMock.Setup(x => x.GetReferenceDataAsync<BodyParts>())
                .ReturnsAsync(bodyParts);

            // Act
            await _stateService.LoadBodyPartsAsync();

            // Assert
            _stateService.BodyParts.Should().HaveCount(2);
            _stateService.IsLoadingBodyParts.Should().BeFalse();
        }

        #endregion

        #region FilteredMuscleGroups Tests

        [Fact]
        public async Task FilteredMuscleGroups_ShouldFilterBySearchTerm()
        {
            // Arrange
            var muscleGroups = new List<MuscleGroupDto>
            {
                new() { Id = "mg-1", Name = "Biceps", BodyPartId = "bp-1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "mg-2", Name = "Triceps", BodyPartId = "bp-1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "mg-3", Name = "Quadriceps", BodyPartId = "bp-2", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _muscleGroupsServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(muscleGroups);
            await _stateService.LoadMuscleGroupsAsync();

            // Act
            _stateService.SearchTerm = "ceps";

            // Assert
            _stateService.FilteredMuscleGroups.Should().HaveCount(3);
            _stateService.FilteredMuscleGroups.Should().Contain(mg => mg.Name == "Biceps");
            _stateService.FilteredMuscleGroups.Should().Contain(mg => mg.Name == "Triceps");
            _stateService.FilteredMuscleGroups.Should().Contain(mg => mg.Name == "Quadriceps");
        }

        [Fact]
        public async Task FilteredMuscleGroups_ShouldFilterByBodyPart()
        {
            // Arrange
            var muscleGroups = new List<MuscleGroupDto>
            {
                new() { Id = "mg-1", Name = "Biceps", BodyPartId = "bp-1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "mg-2", Name = "Triceps", BodyPartId = "bp-1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "mg-3", Name = "Quadriceps", BodyPartId = "bp-2", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _muscleGroupsServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(muscleGroups);
            await _stateService.LoadMuscleGroupsAsync();

            // Act
            _stateService.SelectedBodyPartId = "bp-1";

            // Assert
            _stateService.FilteredMuscleGroups.Should().HaveCount(2);
            _stateService.FilteredMuscleGroups.Should().NotContain(mg => mg.Name == "Quadriceps");
        }

        [Fact]
        public async Task FilteredMuscleGroups_ShouldCombineFilters()
        {
            // Arrange
            var muscleGroups = new List<MuscleGroupDto>
            {
                new() { Id = "mg-1", Name = "Biceps", BodyPartId = "bp-1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "mg-2", Name = "Triceps", BodyPartId = "bp-1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "mg-3", Name = "Quadriceps", BodyPartId = "bp-2", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _muscleGroupsServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(muscleGroups);
            await _stateService.LoadMuscleGroupsAsync();

            // Act
            _stateService.SearchTerm = "Bi";
            _stateService.SelectedBodyPartId = "bp-1";

            // Assert
            _stateService.FilteredMuscleGroups.Should().HaveCount(1);
            _stateService.FilteredMuscleGroups.First().Name.Should().Be("Biceps");
        }

        #endregion

        #region CreateMuscleGroupAsync Tests

        [Fact]
        public async Task CreateMuscleGroupAsync_ShouldReloadList_WhenSuccessful()
        {
            // Arrange
            var createDto = new CreateMuscleGroupDto { Name = "Deltoids", BodyPartId = "bp-1" };
            var created = new MuscleGroupDto
            {
                Id = "mg-new",
                Name = "Deltoids",
                BodyPartId = "bp-1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _muscleGroupsServiceMock.Setup(x => x.CreateMuscleGroupAsync(It.IsAny<CreateMuscleGroupDto>()))
                .ReturnsAsync(created);
            _muscleGroupsServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(new List<MuscleGroupDto> { created });

            // Act
            await _stateService.CreateMuscleGroupAsync(createDto);

            // Assert
            _stateService.MuscleGroups.Should().Contain(mg => mg.Name == "Deltoids");
            _stateService.IsCreating.Should().BeFalse();
            _stateService.ErrorMessage.Should().BeNull();
            _muscleGroupsServiceMock.Verify(x => x.GetMuscleGroupsAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateMuscleGroupAsync_ShouldSetErrorMessage_WhenFails()
        {
            // Arrange
            var createDto = new CreateMuscleGroupDto { Name = "Deltoids", BodyPartId = "bp-1" };
            _muscleGroupsServiceMock.Setup(x => x.CreateMuscleGroupAsync(It.IsAny<CreateMuscleGroupDto>()))
                .ThrowsAsync(new InvalidOperationException("Name already exists"));

            // Act
            await _stateService.CreateMuscleGroupAsync(createDto);

            // Assert
            _stateService.IsCreating.Should().BeFalse();
            _stateService.ErrorMessage.Should().Be("Name already exists");
        }

        #endregion

        #region UpdateMuscleGroupAsync Tests

        [Fact]
        public async Task UpdateMuscleGroupAsync_ShouldReloadListAndUpdateSelected_WhenSuccessful()
        {
            // Arrange
            var muscleGroup = new MuscleGroupDto
            {
                Id = "mg-1",
                Name = "Biceps",
                BodyPartId = "bp-1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var updateDto = new UpdateMuscleGroupDto { Name = "Updated Biceps", BodyPartId = "bp-1" };
            var updated = new MuscleGroupDto
            {
                Id = "mg-1",
                Name = "Updated Biceps",
                BodyPartId = "bp-1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _muscleGroupsServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(new List<MuscleGroupDto> { muscleGroup });
            await _stateService.LoadMuscleGroupsAsync();
            _stateService.SelectMuscleGroup(muscleGroup);

            _muscleGroupsServiceMock.Setup(x => x.UpdateMuscleGroupAsync("mg-1", It.IsAny<UpdateMuscleGroupDto>()))
                .ReturnsAsync(updated);
            _muscleGroupsServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(new List<MuscleGroupDto> { updated });

            // Act
            await _stateService.UpdateMuscleGroupAsync("mg-1", updateDto);

            // Assert
            _stateService.MuscleGroups.First().Name.Should().Be("Updated Biceps");
            _stateService.SelectedMuscleGroup?.Name.Should().Be("Updated Biceps");
            _stateService.IsUpdating.Should().BeFalse();
            _stateService.ErrorMessage.Should().BeNull();
        }

        #endregion

        #region DeleteMuscleGroupAsync Tests

        [Fact]
        public async Task DeleteMuscleGroupAsync_ShouldRemoveFromListAndClearSelected_WhenSuccessful()
        {
            // Arrange
            var muscleGroups = new List<MuscleGroupDto>
            {
                new() { Id = "mg-1", Name = "Biceps", BodyPartId = "bp-1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "mg-2", Name = "Triceps", BodyPartId = "bp-1", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            _muscleGroupsServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(muscleGroups);
            await _stateService.LoadMuscleGroupsAsync();
            _stateService.SelectMuscleGroup(muscleGroups.First());

            _muscleGroupsServiceMock.Setup(x => x.DeleteMuscleGroupAsync("mg-1"))
                .Returns(Task.CompletedTask);
            _muscleGroupsServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(new List<MuscleGroupDto> { muscleGroups[1] });

            // Act
            await _stateService.DeleteMuscleGroupAsync("mg-1");

            // Assert
            _stateService.MuscleGroups.Should().HaveCount(1);
            _stateService.MuscleGroups.Should().NotContain(mg => mg.Id == "mg-1");
            _stateService.SelectedMuscleGroup.Should().BeNull();
            _stateService.IsDeleting.Should().BeFalse();
            _stateService.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task DeleteMuscleGroupAsync_ShouldSetErrorAndReload_WhenFails()
        {
            // Arrange
            var muscleGroups = new List<MuscleGroupDto>
            {
                new() { Id = "mg-1", Name = "Biceps", BodyPartId = "bp-1", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            _muscleGroupsServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(muscleGroups);
            await _stateService.LoadMuscleGroupsAsync();

            _muscleGroupsServiceMock.Setup(x => x.DeleteMuscleGroupAsync("mg-1"))
                .ThrowsAsync(new InvalidOperationException("Cannot delete - in use"));

            // Act
            await _stateService.DeleteMuscleGroupAsync("mg-1");

            // Assert
            _stateService.ErrorMessage.Should().Be("Cannot delete - in use");
            _stateService.IsDeleting.Should().BeFalse();
            _muscleGroupsServiceMock.Verify(x => x.GetMuscleGroupsAsync(), Times.Exactly(2)); // Initial load + reload after error
        }

        #endregion

        #region State Change Notification Tests

        [Fact]
        public void SearchTerm_ShouldTriggerOnChange_WhenSet()
        {
            // Arrange
            var changeTriggered = false;
            _stateService.OnChange += () => changeTriggered = true;

            // Act
            _stateService.SearchTerm = "test";

            // Assert
            changeTriggered.Should().BeTrue();
        }

        [Fact]
        public void SelectedBodyPartId_ShouldTriggerOnChange_WhenSet()
        {
            // Arrange
            var changeTriggered = false;
            _stateService.OnChange += () => changeTriggered = true;

            // Act
            _stateService.SelectedBodyPartId = "bp-1";

            // Assert
            changeTriggered.Should().BeTrue();
        }

        [Fact]
        public void SelectMuscleGroup_ShouldTriggerOnChange()
        {
            // Arrange
            var changeTriggered = false;
            _stateService.OnChange += () => changeTriggered = true;
            var muscleGroup = new MuscleGroupDto
            {
                Id = "mg-1",
                Name = "Biceps",
                BodyPartId = "bp-1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            _stateService.SelectMuscleGroup(muscleGroup);

            // Assert
            changeTriggered.Should().BeTrue();
            _stateService.SelectedMuscleGroup.Should().Be(muscleGroup);
        }

        [Fact]
        public void ClearError_ShouldTriggerOnChange()
        {
            // Arrange
            var changeTriggered = false;
            _stateService.OnChange += () => changeTriggered = true;

            // Act
            _stateService.ClearError();

            // Assert
            changeTriggered.Should().BeTrue();
        }

        #endregion

        #region InitializeAsync Tests

        [Fact]
        public async Task InitializeAsync_ShouldLoadBothBodyPartsAndMuscleGroups()
        {
            // Arrange
            var bodyParts = new List<ReferenceDataDto>
            {
                new() { Id = "bp-1", Value = "Arms" }
            };
            var muscleGroups = new List<MuscleGroupDto>
            {
                new() { Id = "mg-1", Name = "Biceps", BodyPartId = "bp-1", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            _referenceDataServiceMock.Setup(x => x.GetReferenceDataAsync<BodyParts>())
                .ReturnsAsync(bodyParts);
            _muscleGroupsServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(muscleGroups);

            // Act
            await _stateService.InitializeAsync();

            // Assert
            _stateService.BodyParts.Should().HaveCount(1);
            _stateService.MuscleGroups.Should().HaveCount(1);
            _stateService.MuscleGroups.First().BodyPartName.Should().Be("Arms");
        }

        #endregion
    }
}