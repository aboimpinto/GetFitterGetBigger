using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class EquipmentStateServiceTests
    {
        private readonly Mock<IEquipmentService> _equipmentServiceMock;
        private readonly EquipmentStateService _stateService;
        private int _stateChangeCount;

        public EquipmentStateServiceTests()
        {
            _equipmentServiceMock = new Mock<IEquipmentService>();
            _stateService = new EquipmentStateService(_equipmentServiceMock.Object);
            _stateService.OnChange += () => _stateChangeCount++;
        }

        [Fact]
        public async Task InitializeAsync_LoadsEquipment()
        {
            // Arrange
            var equipment = new List<EquipmentDto>
            {
                new() { Id = "1", Name = "Barbell", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "2", Name = "Dumbbell", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _equipmentServiceMock.Setup(x => x.GetEquipmentAsync())
                .ReturnsAsync(equipment);

            // Act
            await _stateService.InitializeAsync();

            // Assert
            _stateService.Equipment.Should().HaveCount(2);
            _stateService.Equipment.First().Name.Should().Be("Barbell");
            _stateService.IsLoading.Should().BeFalse();
            _stateService.ErrorMessage.Should().BeNull();
            _stateChangeCount.Should().Be(2); // Once for loading, once for loaded
        }

        [Fact]
        public async Task LoadEquipmentAsync_WhenFails_SetsErrorMessage()
        {
            // Arrange
            _equipmentServiceMock.Setup(x => x.GetEquipmentAsync())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            await _stateService.LoadEquipmentAsync();

            // Assert
            _stateService.Equipment.Should().BeEmpty();
            _stateService.ErrorMessage.Should().Be("Failed to load equipment: Network error");
            _stateService.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task FilteredEquipment_FiltersBasedOnSearchTerm()
        {
            // Arrange
            var equipment = new List<EquipmentDto>
            {
                new() { Id = "1", Name = "Barbell", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "2", Name = "Dumbbell", IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = "3", Name = "Kettlebell", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _equipmentServiceMock.Setup(x => x.GetEquipmentAsync())
                .ReturnsAsync(equipment);

            // Act
            await _stateService.LoadEquipmentAsync();
            _stateService.SearchTerm = "bell";

            // Assert
            _stateService.FilteredEquipment.Should().HaveCount(3);
            _stateService.FilteredEquipment.Select(e => e.Name)
                .Should().BeEquivalentTo(new[] { "Barbell", "Dumbbell", "Kettlebell" });

            // Test case-insensitive search
            _stateService.SearchTerm = "BARBELL";
            _stateService.FilteredEquipment.Should().HaveCount(1);
            _stateService.FilteredEquipment.First().Name.Should().Be("Barbell");
        }

        [Fact]
        public async Task CreateEquipmentAsync_WhenSuccessful_ReloadsListAndSelectsNew()
        {
            // Arrange
            var createDto = new CreateEquipmentDto { Name = "New Equipment" };
            var createdEquipment = new EquipmentDto
            {
                Id = "new-1",
                Name = "New Equipment",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _equipmentServiceMock.Setup(x => x.CreateEquipmentAsync(createDto))
                .ReturnsAsync(createdEquipment);

            _equipmentServiceMock.Setup(x => x.GetEquipmentAsync())
                .ReturnsAsync(new List<EquipmentDto> { createdEquipment }); // After creation

            // Act
            await _stateService.CreateEquipmentAsync(createDto);

            // Assert
            _stateService.Equipment.Should().HaveCount(1);
            _stateService.SelectedEquipment.Should().NotBeNull();
            _stateService.SelectedEquipment!.Id.Should().Be("new-1");
            _stateService.IsCreating.Should().BeFalse();
            _stateService.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task CreateEquipmentAsync_WhenNameConflict_SetsErrorMessage()
        {
            // Arrange
            var createDto = new CreateEquipmentDto { Name = "Existing" };
            _equipmentServiceMock.Setup(x => x.CreateEquipmentAsync(createDto))
                .ThrowsAsync(new InvalidOperationException("Equipment with this name already exists"));

            // Act
            await _stateService.CreateEquipmentAsync(createDto);

            // Assert
            _stateService.ErrorMessage.Should().Be("Equipment with this name already exists");
            _stateService.IsCreating.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateEquipmentAsync_WhenSuccessful_ReloadsListAndUpdatesSelected()
        {
            // Arrange
            var equipmentId = "equipment-1";
            var updateDto = new UpdateEquipmentDto { Name = "Updated Equipment" };
            var originalEquipment = new EquipmentDto
            {
                Id = equipmentId,
                Name = "Original",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var updatedEquipment = new EquipmentDto
            {
                Id = equipmentId,
                Name = "Updated Equipment",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            _equipmentServiceMock.Setup(x => x.UpdateEquipmentAsync(equipmentId, updateDto))
                .ReturnsAsync(updatedEquipment);

            _equipmentServiceMock.SetupSequence(x => x.GetEquipmentAsync())
                .ReturnsAsync(new List<EquipmentDto> { originalEquipment })
                .ReturnsAsync(new List<EquipmentDto> { updatedEquipment });

            await _stateService.LoadEquipmentAsync();
            _stateService.SelectEquipment(originalEquipment);

            // Act
            await _stateService.UpdateEquipmentAsync(equipmentId, updateDto);

            // Assert
            _stateService.Equipment.Should().HaveCount(1);
            _stateService.Equipment.First().Name.Should().Be("Updated Equipment");
            _stateService.SelectedEquipment!.Name.Should().Be("Updated Equipment");
            _stateService.IsUpdating.Should().BeFalse();
            _stateService.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task DeleteEquipmentAsync_WhenSuccessful_ReloadsListAndClearsSelected()
        {
            // Arrange
            var equipmentId = "equipment-1";
            var equipment = new EquipmentDto
            {
                Id = equipmentId,
                Name = "To Delete",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _equipmentServiceMock.Setup(x => x.DeleteEquipmentAsync(equipmentId))
                .Returns(Task.CompletedTask);

            _equipmentServiceMock.SetupSequence(x => x.GetEquipmentAsync())
                .ReturnsAsync(new List<EquipmentDto> { equipment })
                .ReturnsAsync(new List<EquipmentDto>()); // Empty after deletion

            await _stateService.LoadEquipmentAsync();
            _stateService.SelectEquipment(equipment);

            // Act
            await _stateService.DeleteEquipmentAsync(equipmentId);

            // Assert
            _stateService.Equipment.Should().BeEmpty();
            _stateService.SelectedEquipment.Should().BeNull();
            _stateService.IsDeleting.Should().BeFalse();
            _stateService.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task DeleteEquipmentAsync_WhenInUse_SetsErrorMessage()
        {
            // Arrange
            var equipmentId = "equipment-1";
            _equipmentServiceMock.Setup(x => x.DeleteEquipmentAsync(equipmentId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete equipment that is in use by exercises"));

            // Act
            await _stateService.DeleteEquipmentAsync(equipmentId);

            // Assert
            _stateService.ErrorMessage.Should().Be("Cannot delete equipment that is in use by exercises");
            _stateService.IsDeleting.Should().BeFalse();
        }

        [Fact]
        public void SelectEquipment_UpdatesSelectedAndNotifies()
        {
            // Arrange
            var equipment = new EquipmentDto
            {
                Id = "1",
                Name = "Selected",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _stateChangeCount = 0;

            // Act
            _stateService.SelectEquipment(equipment);

            // Assert
            _stateService.SelectedEquipment.Should().Be(equipment);
            _stateChangeCount.Should().Be(1);
        }

        [Fact]
        public void ClearSelectedEquipment_ClearsAndNotifies()
        {
            // Arrange
            var equipment = new EquipmentDto
            {
                Id = "1",
                Name = "Selected",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _stateService.SelectEquipment(equipment);
            _stateChangeCount = 0;

            // Act
            _stateService.ClearSelectedEquipment();

            // Assert
            _stateService.SelectedEquipment.Should().BeNull();
            _stateChangeCount.Should().Be(1);
        }

        [Fact]
        public async Task ClearError_ClearsErrorMessageAndNotifies()
        {
            // Arrange
            _equipmentServiceMock.Setup(x => x.GetEquipmentAsync())
                .ThrowsAsync(new Exception("Error"));
            await _stateService.LoadEquipmentAsync();
            _stateChangeCount = 0;

            // Act
            _stateService.ClearError();

            // Assert
            _stateService.ErrorMessage.Should().BeNull();
            _stateChangeCount.Should().Be(1);
        }

        [Fact]
        public void SetSearchTerm_UpdatesTermAndNotifies()
        {
            // Arrange
            _stateChangeCount = 0;

            // Act
            _stateService.SetSearchTerm("test");

            // Assert
            _stateService.SearchTerm.Should().Be("test");
            _stateChangeCount.Should().Be(1);
        }

        [Fact]
        public void IsLoadingEquipment_AlwaysReturnsFalse()
        {
            // This property is not used for equipment management
            _stateService.IsLoadingEquipment.Should().BeFalse();
        }
    }
}