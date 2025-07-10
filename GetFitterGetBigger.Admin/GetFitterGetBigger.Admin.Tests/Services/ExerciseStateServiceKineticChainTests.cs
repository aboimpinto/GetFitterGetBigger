using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Tests.Builders;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ExerciseStateServiceKineticChainTests
    {
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly Mock<IReferenceDataService> _referenceDataServiceMock;
        private readonly ExerciseStateService _stateService;

        public ExerciseStateServiceKineticChainTests()
        {
            _exerciseServiceMock = new Mock<IExerciseService>();
            _referenceDataServiceMock = new Mock<IReferenceDataService>();
            _stateService = new ExerciseStateService(_exerciseServiceMock.Object, _referenceDataServiceMock.Object);
        }

        [Fact]
        public async Task InitializeAsync_LoadsKineticChainTypes_Successfully()
        {
            // Arrange
            var kineticChainTypes = new List<ReferenceDataDto>
            {
                new() { Id = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4", Value = "Compound", Description = "Multi-muscle movement" },
                new() { Id = "kineticchaintype-2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b", Value = "Isolation", Description = "Single-muscle movement" }
            };

            SetupAllReferenceData();
            _referenceDataServiceMock.Setup(x => x.GetKineticChainTypesAsync()).ReturnsAsync(kineticChainTypes);
            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(new ExercisePagedResultDto { Items = new List<ExerciseListDto>() });

            // Act
            await _stateService.InitializeAsync();

            // Assert
            _stateService.KineticChainTypes.Should().HaveCount(2);
            _stateService.KineticChainTypes.Should().BeEquivalentTo(kineticChainTypes);
            _stateService.KineticChainTypes.First().Value.Should().Be("Compound");
            _stateService.KineticChainTypes.Last().Value.Should().Be("Isolation");
        }

        [Fact]
        public async Task InitializeAsync_HandlesEmptyKineticChainTypes_Gracefully()
        {
            // Arrange
            SetupAllReferenceData();
            _referenceDataServiceMock.Setup(x => x.GetKineticChainTypesAsync())
                .ReturnsAsync(Enumerable.Empty<ReferenceDataDto>());
            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(new ExercisePagedResultDto { Items = new List<ExerciseListDto>() });

            // Act
            await _stateService.InitializeAsync();

            // Assert
            _stateService.KineticChainTypes.Should().BeEmpty();
            _stateService.IsLoadingReferenceData.Should().BeFalse();
            _stateService.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task InitializeAsync_LoadsKineticChainTypes_AlongWithOtherReferenceData()
        {
            // Arrange
            var kineticChainTypes = new List<ReferenceDataDto>
            {
                new() { Id = "1", Value = "Compound", Description = "Multi-muscle" },
                new() { Id = "2", Value = "Isolation", Description = "Single-muscle" }
            };

            SetupAllReferenceData();
            _referenceDataServiceMock.Setup(x => x.GetKineticChainTypesAsync()).ReturnsAsync(kineticChainTypes);
            _exerciseServiceMock.Setup(x => x.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                .ReturnsAsync(new ExercisePagedResultDto { Items = new List<ExerciseListDto>() });

            // Act
            await _stateService.InitializeAsync();

            // Assert - All reference data including KineticChainTypes should be loaded
            _stateService.KineticChainTypes.Should().HaveCount(2);
            _stateService.DifficultyLevels.Should().HaveCount(3);
            _stateService.MuscleGroups.Should().HaveCount(5);
            _stateService.MuscleRoles.Should().HaveCount(3);
            _stateService.Equipment.Should().HaveCount(4);
            _stateService.BodyParts.Should().HaveCount(2);
            _stateService.MovementPatterns.Should().HaveCount(6);
            _stateService.ExerciseTypes.Should().HaveCount(2);
        }

        [Fact]
        public void KineticChainTypes_DefaultsToEmpty_WhenNotInitialized()
        {
            // Act & Assert
            _stateService.KineticChainTypes.Should().BeEmpty();
            _stateService.KineticChainTypes.Should().NotBeNull();
        }

        private void SetupAllReferenceData()
        {
            _referenceDataServiceMock.Setup(x => x.GetDifficultyLevelsAsync())
                .ReturnsAsync(ReferenceDataDtoBuilder.BuildList(3));
            _referenceDataServiceMock.Setup(x => x.GetMuscleGroupsAsync())
                .ReturnsAsync(ReferenceDataDtoBuilder.BuildList(5));
            _referenceDataServiceMock.Setup(x => x.GetMuscleRolesAsync())
                .ReturnsAsync(ReferenceDataDtoBuilder.BuildList(3));
            _referenceDataServiceMock.Setup(x => x.GetEquipmentAsync())
                .ReturnsAsync(ReferenceDataDtoBuilder.BuildList(4));
            _referenceDataServiceMock.Setup(x => x.GetBodyPartsAsync())
                .ReturnsAsync(ReferenceDataDtoBuilder.BuildList(2));
            _referenceDataServiceMock.Setup(x => x.GetMovementPatternsAsync())
                .ReturnsAsync(ReferenceDataDtoBuilder.BuildList(6));
            _referenceDataServiceMock.Setup(x => x.GetExerciseTypesAsync())
                .ReturnsAsync(new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Warmup", Description = "Warmup exercises" },
                    new() { Id = "2", Value = "Workout", Description = "Main workout exercises" }
                });
        }
    }
}