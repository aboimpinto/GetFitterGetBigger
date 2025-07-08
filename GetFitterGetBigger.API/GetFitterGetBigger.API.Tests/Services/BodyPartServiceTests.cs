using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class BodyPartServiceTests
    {
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IBodyPartRepository> _mockBodyPartRepository;
        private readonly BodyPartService _bodyPartService;

        public BodyPartServiceTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockBodyPartRepository = new Mock<IBodyPartRepository>();

            _mockUnitOfWorkProvider
                .Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);

            _mockReadOnlyUnitOfWork
                .Setup(x => x.GetRepository<IBodyPartRepository>())
                .Returns(_mockBodyPartRepository.Object);

            _bodyPartService = new BodyPartService(
                _mockUnitOfWorkProvider.Object);
        }

        [Fact]
        public async Task ExistsAsync_WhenBodyPartExists_ReturnsTrue()
        {
            // Arrange
            var bodyPartId = BodyPartId.New();
            var bodyPart = BodyPart.Handler.Create(
                bodyPartId,
                "Chest",
                "Chest muscles",
                1,
                true);

            _mockBodyPartRepository
                .Setup(x => x.GetByIdAsync(bodyPartId))
                .ReturnsAsync(bodyPart);

            // Act
            var result = await _bodyPartService.ExistsAsync(bodyPartId);

            // Assert
            Assert.True(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockBodyPartRepository.Verify(x => x.GetByIdAsync(bodyPartId), Times.Once);
            _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WhenBodyPartDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var bodyPartId = BodyPartId.New();

            _mockBodyPartRepository
                .Setup(x => x.GetByIdAsync(bodyPartId))
                .ReturnsAsync((BodyPart?)null);

            // Act
            var result = await _bodyPartService.ExistsAsync(bodyPartId);

            // Assert
            Assert.False(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockBodyPartRepository.Verify(x => x.GetByIdAsync(bodyPartId), Times.Once);
            _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
        }
    }
}