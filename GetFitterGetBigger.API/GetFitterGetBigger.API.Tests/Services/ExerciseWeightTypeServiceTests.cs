using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.Constants;
using Microsoft.Extensions.Logging;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class ExerciseWeightTypeServiceTests
    {
        private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
        private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
        private readonly Mock<IExerciseWeightTypeRepository> _mockRepository;
        private readonly Mock<ILogger<ExerciseWeightTypeService>> _mockLogger;
        private readonly ExerciseWeightTypeService _service;
        
        private readonly List<ExerciseWeightType> _testData;
        
        public ExerciseWeightTypeServiceTests()
        {
            _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
            _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
            _mockRepository = new Mock<IExerciseWeightTypeRepository>();
            _mockLogger = new Mock<ILogger<ExerciseWeightTypeService>>();
            
            _mockUnitOfWorkProvider
                .Setup(x => x.CreateReadOnly())
                .Returns(_mockReadOnlyUnitOfWork.Object);
                
            _mockReadOnlyUnitOfWork
                .Setup(x => x.GetRepository<IExerciseWeightTypeRepository>())
                .Returns(_mockRepository.Object);
                
            _service = new ExerciseWeightTypeService(
                _mockUnitOfWorkProvider.Object,
                _mockLogger.Object);
                
            // Initialize test data
            _testData = new List<ExerciseWeightType>
            {
                ExerciseWeightType.Handler.Create(
                    ExerciseWeightTypeTestConstants.BodyweightOnlyId,
                    ExerciseWeightTypeTestConstants.BodyweightOnlyCode,
                    ExerciseWeightTypeTestConstants.BodyweightOnlyValue,
                    ExerciseWeightTypeTestConstants.BodyweightOnlyDescription,
                    1,
                    true).Value,
                ExerciseWeightType.Handler.Create(
                    ExerciseWeightTypeTestConstants.BodyweightOptionalId,
                    ExerciseWeightTypeTestConstants.BodyweightOptionalCode,
                    ExerciseWeightTypeTestConstants.BodyweightOptionalValue,
                    ExerciseWeightTypeTestConstants.BodyweightOptionalDescription,
                    2,
                    true).Value,
                ExerciseWeightType.Handler.Create(
                    ExerciseWeightTypeTestConstants.WeightRequiredId,
                    ExerciseWeightTypeTestConstants.WeightRequiredCode,
                    ExerciseWeightTypeTestConstants.WeightRequiredValue,
                    ExerciseWeightTypeTestConstants.WeightRequiredDescription,
                    3,
                    true).Value,
                ExerciseWeightType.Handler.Create(
                    ExerciseWeightTypeTestConstants.MachineWeightId,
                    ExerciseWeightTypeTestConstants.MachineWeightCode,
                    ExerciseWeightTypeTestConstants.MachineWeightValue,
                    ExerciseWeightTypeTestConstants.MachineWeightDescription,
                    4,
                    true).Value,
                ExerciseWeightType.Handler.Create(
                    ExerciseWeightTypeTestConstants.NoWeightId,
                    ExerciseWeightTypeTestConstants.NoWeightCode,
                    ExerciseWeightTypeTestConstants.NoWeightValue,
                    ExerciseWeightTypeTestConstants.NoWeightDescription,
                    5,
                    true).Value
            };
        }
        
        #region GetAllActiveAsync Tests
        
        [Fact]
        public async Task GetAllActiveAsync_WhenCached_ReturnsCachedData()
        {
            // Arrange
            var cachedDtos = _testData.Select(MapToDto).ToList();
            
            _mockRepository
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(_testData);
            
            // Act
            var result = await _service.GetAllActiveAsync();
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(5, result.Data.Count());
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
        }
        
        [Fact]
        public async Task GetAllActiveAsync_WhenNotCached_LoadsFromDatabase()
        {
            // Arrange
                
            _mockRepository
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(_testData);
                
            
            // Act
            var result = await _service.GetAllActiveAsync();
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(5, result.Data.Count());
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
        }
        
        #endregion
        
        #region GetByIdAsync Tests
        
        [Fact]
        public async Task GetByIdAsync_WithEmptyId_ReturnsValidationError()
        {
            // Arrange
            var emptyId = ExerciseWeightTypeId.Empty;
            
            // Act
            var result = await _service.GetByIdAsync(emptyId);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
        }
        
        [Fact]
        public async Task GetByIdAsync_WhenCached_ReturnsCachedData()
        {
            // Arrange
            var id = ExerciseWeightTypeTestConstants.BodyweightOnlyId;
            var weightType = _testData[0];
            var dto = new ReferenceDataDto
                {
                Id = id.ToString(),
                Value = ExerciseWeightTypeTestConstants.BodyweightOnlyValue,
                Description = ExerciseWeightTypeTestConstants.BodyweightOnlyDescription
            };
            
            _mockRepository
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(weightType);
            
            // Act
            var result = await _service.GetByIdAsync(id);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(dto.Id, result.Data.Id);
            Assert.Equal(dto.Value, result.Data.Value);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
        }
        
        [Fact]
        public async Task GetByIdAsync_WhenNotCachedAndExists_LoadsFromDatabase()
        {
            // Arrange
            var weightType = _testData[0];
            var id = weightType.Id;
            
                
            _mockRepository
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(weightType);
                
            
            // Act
            var result = await _service.GetByIdAsync(id);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(id.ToString(), result.Data.Id);
            Assert.Equal(weightType.Value, result.Data.Value);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockRepository.Verify(x => x.GetByIdAsync(id), Times.Once);
        }
        
        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var id = ExerciseWeightTypeId.From(Guid.NewGuid());
            
                
            _mockRepository
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(ExerciseWeightType.Empty);
            
            // Act
            var result = await _service.GetByIdAsync(id);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockRepository.Verify(x => x.GetByIdAsync(id), Times.Once);
        }
        
        #endregion
        
        #region GetByValueAsync Tests
        
        [Fact]
        public async Task GetByValueAsync_WithEmptyValue_ReturnsValidationError()
        {
            // Act
            var result = await _service.GetByValueAsync(string.Empty);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
        }
        
        [Fact]
        public async Task GetByValueAsync_WithNullValue_ReturnsValidationError()
        {
            // Act
            var result = await _service.GetByValueAsync(null!);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
        }
        
        [Fact]
        public async Task GetByValueAsync_WhenCached_ReturnsCachedData()
        {
            // Arrange
            var value = ExerciseWeightTypeTestConstants.BodyweightOnlyValue;
            var weightType = _testData[0];
            var dto = new ReferenceDataDto
                {
                Id = ExerciseWeightTypeTestConstants.BodyweightOnlyDtoId,
                Value = value,
                Description = ExerciseWeightTypeTestConstants.BodyweightOnlyDescription
            };
            
            _mockRepository
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(weightType);
            
            // Act
            var result = await _service.GetByValueAsync(value);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(dto.Value, result.Data.Value);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
        }
        
        [Fact]
        public async Task GetByValueAsync_WhenNotCachedAndExists_LoadsFromDatabase()
        {
            // Arrange
            var weightType = _testData[0];
            var value = weightType.Value;
            
                
            _mockRepository
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(weightType);
                
            
            // Act
            var result = await _service.GetByValueAsync(value);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(value, result.Data.Value);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockRepository.Verify(x => x.GetByValueAsync(value), Times.Once);
        }
        
        [Fact]
        public async Task GetByValueAsync_WhenNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var value = ExerciseWeightTypeTestConstants.NonExistentValue;
            
                
            _mockRepository
                .Setup(x => x.GetByValueAsync(value))
                .ReturnsAsync(ExerciseWeightType.Empty);
            
            // Act
            var result = await _service.GetByValueAsync(value);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        }
        
        #endregion
        
        #region GetByCodeAsync Tests
        
        [Fact]
        public async Task GetByCodeAsync_WithEmptyCode_ReturnsValidationError()
        {
            // Act
            var result = await _service.GetByCodeAsync(string.Empty);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
        }
        
        [Fact]
        public async Task GetByCodeAsync_WhenCached_ReturnsCachedData()
        {
            // Arrange
            var code = ExerciseWeightTypeTestConstants.BodyweightOnlyCode;
            var weightType = _testData[0];
            var dto = new ReferenceDataDto
                {
                Id = ExerciseWeightTypeTestConstants.BodyweightOnlyDtoId,
                Value = ExerciseWeightTypeTestConstants.BodyweightOnlyValue,
                Description = ExerciseWeightTypeTestConstants.BodyweightOnlyDescription
            };
            
            _mockRepository
                .Setup(x => x.GetByCodeAsync(code))
                .ReturnsAsync(weightType);
            
            // Act
            var result = await _service.GetByCodeAsync(code);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(dto.Value, result.Data.Value);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
        }
        
        [Fact]
        public async Task GetByCodeAsync_WhenNotCachedAndExists_LoadsFromDatabase()
        {
            // Arrange
            var weightType = _testData[0];
            var code = weightType.Code;
            
                
            _mockRepository
                .Setup(x => x.GetByCodeAsync(code))
                .ReturnsAsync(weightType);
                
            
            // Act
            var result = await _service.GetByCodeAsync(code);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(weightType.Value, result.Data.Value);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
            _mockRepository.Verify(x => x.GetByCodeAsync(code), Times.Once);
        }
        
        #endregion
        
        #region ExistsAsync Tests
        
        [Fact]
        public async Task ExistsAsync_WithEmptyId_ReturnsFalse()
        {
            // Arrange
            var emptyId = ExerciseWeightTypeId.Empty;
            
            // Act
            var result = await _service.ExistsAsync(emptyId);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorCode.InvalidFormat, result.PrimaryErrorCode);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
        }
        
        [Fact]
        public async Task ExistsAsync_WhenExists_ReturnsTrue()
        {
            // Arrange
            var id = ExerciseWeightTypeTestConstants.BodyweightOnlyId;
            
            _mockRepository
                .Setup(x => x.ExistsAsync(id))
                .ReturnsAsync(true);
            
            // Act
            var result = await _service.ExistsAsync(id);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
        }
        
        [Fact]
        public async Task ExistsAsync_WhenNotExists_ReturnsFalse()
        {
            // Arrange
            var id = ExerciseWeightTypeId.From(Guid.NewGuid());
            
                
            _mockRepository
                .Setup(x => x.ExistsAsync(id))
                .ReturnsAsync(false);
            
            // Act
            var result = await _service.ExistsAsync(id);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.Data);
        }
        
        #endregion
        
        #region IsValidWeightForTypeAsync Tests
        
        [Fact]
        public async Task IsValidWeightForTypeAsync_WithEmptyId_ReturnsFalse()
        {
            // Arrange
            var emptyId = ExerciseWeightTypeId.Empty;
            
            // Act
            var result = await _service.IsValidWeightForTypeAsync(emptyId, 10);
            
            // Assert
            Assert.False(result);
            _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
        }
        
        [Fact]
        public async Task IsValidWeightForTypeAsync_BodyweightOnly_WithNullWeight_ReturnsTrue()
        {
            // Arrange
            var weightType = _testData[0]; // BODYWEIGHT_ONLY
            var id = weightType.Id;
            
            SetupGetByIdForValidation(id, weightType);
            
            // Act
            var result = await _service.IsValidWeightForTypeAsync(id, null);
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task IsValidWeightForTypeAsync_BodyweightOnly_WithZeroWeight_ReturnsTrue()
        {
            // Arrange
            var weightType = _testData[0]; // BODYWEIGHT_ONLY
            var id = weightType.Id;
            
            SetupGetByIdForValidation(id, weightType);
            
            // Act
            var result = await _service.IsValidWeightForTypeAsync(id, 0);
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task IsValidWeightForTypeAsync_BodyweightOnly_WithPositiveWeight_ReturnsFalse()
        {
            // Arrange
            var weightType = _testData[0]; // BODYWEIGHT_ONLY
            var id = weightType.Id;
            
            SetupGetByIdForValidation(id, weightType);
            
            // Act
            var result = await _service.IsValidWeightForTypeAsync(id, 10);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task IsValidWeightForTypeAsync_WeightRequired_WithPositiveWeight_ReturnsTrue()
        {
            // Arrange
            var weightType = _testData[2]; // WEIGHT_REQUIRED
            var id = weightType.Id;
            
            SetupGetByIdForValidation(id, weightType);
            
            // Act
            var result = await _service.IsValidWeightForTypeAsync(id, 50);
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task IsValidWeightForTypeAsync_WeightRequired_WithNullWeight_ReturnsFalse()
        {
            // Arrange
            var weightType = _testData[2]; // WEIGHT_REQUIRED
            var id = weightType.Id;
            
            SetupGetByIdForValidation(id, weightType);
            
            // Act
            var result = await _service.IsValidWeightForTypeAsync(id, null);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task IsValidWeightForTypeAsync_BodyweightOptional_WithAnyWeight_ReturnsTrue()
        {
            // Arrange
            var weightType = _testData[1]; // BODYWEIGHT_OPTIONAL
            var id = weightType.Id;
            
            SetupGetByIdForValidation(id, weightType);
            
            // Act
            var resultNull = await _service.IsValidWeightForTypeAsync(id, null);
            var resultZero = await _service.IsValidWeightForTypeAsync(id, 0);
            var resultPositive = await _service.IsValidWeightForTypeAsync(id, 25);
            
            // Assert
            Assert.True(resultNull);
            Assert.True(resultZero);
            Assert.True(resultPositive);
        }
        
        #endregion
        
        #region Helper Methods
        
        private static ReferenceDataDto MapToDto(ExerciseWeightType entity)
        {
            return new ReferenceDataDto
            {
                Id = entity.Id.ToString(),
                Value = entity.Value,
                Description = entity.Description
            };
        }
        
        private void SetupGetByIdForValidation(ExerciseWeightTypeId id, ExerciseWeightType weightType)
        {
            var dto = MapToDto(weightType);
            
                
            _mockRepository
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(weightType);
        }
        
        #endregion
    }
}