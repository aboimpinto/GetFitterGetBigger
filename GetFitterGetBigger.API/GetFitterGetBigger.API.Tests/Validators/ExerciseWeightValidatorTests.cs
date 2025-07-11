using System;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Validators;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Validators;

public class ExerciseWeightValidatorTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
    private readonly Mock<IExerciseRepository> _mockExerciseRepository;
    private readonly Mock<IExerciseWeightTypeRepository> _mockWeightTypeRepository;
    private readonly ExerciseWeightValidator _validator;
    
    // Predefined weight type IDs from the feature description
    private readonly ExerciseWeightTypeId _bodyweightOnlyId = ExerciseWeightTypeId.From(Guid.Parse("b2a4c5d7-6b8c-5d9e-0f1a-2b3c4d5e6f7a"));
    private readonly ExerciseWeightTypeId _bodyweightOptionalId = ExerciseWeightTypeId.From(Guid.Parse("a1b3c5d7-5b7c-4d8e-9f0a-1b2c3d4e5f6a"));
    private readonly ExerciseWeightTypeId _weightRequiredId = ExerciseWeightTypeId.From(Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a"));
    private readonly ExerciseWeightTypeId _machineWeightId = ExerciseWeightTypeId.From(Guid.Parse("d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b"));
    private readonly ExerciseWeightTypeId _noWeightId = ExerciseWeightTypeId.From(Guid.Parse("e5d7a8b9-9d0e-8f1a-2b3c-4d5e6f7a8b9c"));
    
    public ExerciseWeightValidatorTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockExerciseRepository = new Mock<IExerciseRepository>();
        _mockWeightTypeRepository = new Mock<IExerciseWeightTypeRepository>();
        
        _mockReadOnlyUnitOfWork
            .Setup(uow => uow.GetRepository<IExerciseRepository>())
            .Returns(_mockExerciseRepository.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(uow => uow.GetRepository<IExerciseWeightTypeRepository>())
            .Returns(_mockWeightTypeRepository.Object);
            
        _mockUnitOfWorkProvider
            .Setup(p => p.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
        _validator = new ExerciseWeightValidator(_mockUnitOfWorkProvider.Object);
    }
    
    #region ValidateWeightAsync Tests
    
    [Fact]
    public async Task ValidateWeightAsync_ExerciseNotFound_ReturnsFailure()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        _mockExerciseRepository
            .Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(Exercise.Empty);
        
        // Act
        var result = await _validator.ValidateWeightAsync(exerciseId, 50m);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains($"Exercise with ID {exerciseId} not found", result.ErrorMessage);
    }
    
    [Fact]
    public async Task ValidateWeightAsync_ExerciseWithNoWeightType_ReturnsSuccess()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exercise = CreateExercise(exerciseId, null);
        
        _mockExerciseRepository
            .Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);
        
        // Act
        var result = await _validator.ValidateWeightAsync(exerciseId, 50m);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
    
    [Fact]
    public async Task ValidateWeightAsync_ExerciseWithWeightType_DelegatesToValidateByType()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var exercise = CreateExercise(exerciseId, _weightRequiredId);
        
        _mockExerciseRepository
            .Setup(r => r.GetByIdAsync(exerciseId))
            .ReturnsAsync(exercise);
            
        SetupWeightType(_weightRequiredId, "WEIGHT_REQUIRED");
        
        // Act
        var resultValid = await _validator.ValidateWeightAsync(exerciseId, 50m);
        var resultInvalid = await _validator.ValidateWeightAsync(exerciseId, null);
        
        // Assert
        Assert.True(resultValid.IsValid);
        Assert.False(resultInvalid.IsValid);
    }
    
    #endregion
    
    #region BODYWEIGHT_ONLY Tests
    
    [Fact]
    public async Task ValidateWeightByTypeAsync_BodyweightOnly_NullWeight_ReturnsSuccess()
    {
        // Arrange
        SetupWeightType(_bodyweightOnlyId, "BODYWEIGHT_ONLY");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_bodyweightOnlyId, null);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
    
    [Fact]
    public async Task ValidateWeightByTypeAsync_BodyweightOnly_ZeroWeight_ReturnsSuccess()
    {
        // Arrange
        SetupWeightType(_bodyweightOnlyId, "BODYWEIGHT_ONLY");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_bodyweightOnlyId, 0m);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
    
    [Theory]
    [InlineData(10.0)]
    [InlineData(50.5)]
    [InlineData(100.0)]
    public async Task ValidateWeightByTypeAsync_BodyweightOnly_WithWeight_ReturnsFailure(double weightDouble)
    {
        // Arrange
        var weight = (decimal)weightDouble;
        SetupWeightType(_bodyweightOnlyId, "BODYWEIGHT_ONLY");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_bodyweightOnlyId, weight);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Bodyweight-only exercises cannot have external weight specified. Weight must be null or 0.", result.ErrorMessage);
    }
    
    #endregion
    
    #region BODYWEIGHT_OPTIONAL Tests
    
    [Fact]
    public async Task ValidateWeightByTypeAsync_BodyweightOptional_NullWeight_ReturnsSuccess()
    {
        // Arrange
        SetupWeightType(_bodyweightOptionalId, "BODYWEIGHT_OPTIONAL");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_bodyweightOptionalId, null);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
    
    [Theory]
    [InlineData(0.0)]
    [InlineData(10.0)]
    [InlineData(50.5)]
    public async Task ValidateWeightByTypeAsync_BodyweightOptional_ValidWeight_ReturnsSuccess(double weightDouble)
    {
        // Arrange
        var weight = (decimal)weightDouble;
        SetupWeightType(_bodyweightOptionalId, "BODYWEIGHT_OPTIONAL");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_bodyweightOptionalId, weight);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
    
    [Fact]
    public async Task ValidateWeightByTypeAsync_BodyweightOptional_NegativeWeight_ReturnsFailure()
    {
        // Arrange
        SetupWeightType(_bodyweightOptionalId, "BODYWEIGHT_OPTIONAL");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_bodyweightOptionalId, -10m);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Weight cannot be negative.", result.ErrorMessage);
    }
    
    #endregion
    
    #region WEIGHT_REQUIRED Tests
    
    [Theory]
    [InlineData(0.1)]
    [InlineData(10.0)]
    [InlineData(100.5)]
    public async Task ValidateWeightByTypeAsync_WeightRequired_ValidWeight_ReturnsSuccess(double weightDouble)
    {
        // Arrange
        var weight = (decimal)weightDouble;
        SetupWeightType(_weightRequiredId, "WEIGHT_REQUIRED");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_weightRequiredId, weight);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
    
    [Fact]
    public async Task ValidateWeightByTypeAsync_WeightRequired_NullWeight_ReturnsFailure()
    {
        // Arrange
        SetupWeightType(_weightRequiredId, "WEIGHT_REQUIRED");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_weightRequiredId, null);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("This exercise requires external weight to be specified. Weight must be greater than 0.", result.ErrorMessage);
    }
    
    [Theory]
    [InlineData(0.0)]
    [InlineData(-10.0)]
    public async Task ValidateWeightByTypeAsync_WeightRequired_InvalidWeight_ReturnsFailure(double weightDouble)
    {
        // Arrange
        var weight = (decimal)weightDouble;
        SetupWeightType(_weightRequiredId, "WEIGHT_REQUIRED");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_weightRequiredId, weight);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("This exercise requires external weight to be specified. Weight must be greater than 0.", result.ErrorMessage);
    }
    
    #endregion
    
    #region MACHINE_WEIGHT Tests
    
    [Theory]
    [InlineData(5.0)]
    [InlineData(20.0)]
    [InlineData(100.0)]
    public async Task ValidateWeightByTypeAsync_MachineWeight_ValidWeight_ReturnsSuccess(double weightDouble)
    {
        // Arrange
        var weight = (decimal)weightDouble;
        SetupWeightType(_machineWeightId, "MACHINE_WEIGHT");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_machineWeightId, weight);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
    
    [Fact]
    public async Task ValidateWeightByTypeAsync_MachineWeight_NullWeight_ReturnsFailure()
    {
        // Arrange
        SetupWeightType(_machineWeightId, "MACHINE_WEIGHT");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_machineWeightId, null);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Machine exercises require weight to be specified. Weight must be greater than 0.", result.ErrorMessage);
    }
    
    [Theory]
    [InlineData(0.0)]
    [InlineData(-5.0)]
    public async Task ValidateWeightByTypeAsync_MachineWeight_InvalidWeight_ReturnsFailure(double weightDouble)
    {
        // Arrange
        var weight = (decimal)weightDouble;
        SetupWeightType(_machineWeightId, "MACHINE_WEIGHT");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_machineWeightId, weight);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Machine exercises require weight to be specified. Weight must be greater than 0.", result.ErrorMessage);
    }
    
    #endregion
    
    #region NO_WEIGHT Tests
    
    [Fact]
    public async Task ValidateWeightByTypeAsync_NoWeight_NullWeight_ReturnsSuccess()
    {
        // Arrange
        SetupWeightType(_noWeightId, "NO_WEIGHT");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_noWeightId, null);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
    
    [Fact]
    public async Task ValidateWeightByTypeAsync_NoWeight_ZeroWeight_ReturnsSuccess()
    {
        // Arrange
        SetupWeightType(_noWeightId, "NO_WEIGHT");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_noWeightId, 0m);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
    
    [Theory]
    [InlineData(1.0)]
    [InlineData(10.0)]
    [InlineData(50.0)]
    public async Task ValidateWeightByTypeAsync_NoWeight_WithWeight_ReturnsFailure(double weightDouble)
    {
        // Arrange
        var weight = (decimal)weightDouble;
        SetupWeightType(_noWeightId, "NO_WEIGHT");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(_noWeightId, weight);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("This exercise type does not use weight. Weight must be null or 0.", result.ErrorMessage);
    }
    
    #endregion
    
    #region Edge Cases
    
    [Fact]
    public async Task ValidateWeightByTypeAsync_UnknownWeightTypeCode_ReturnsFailure()
    {
        // Arrange
        var unknownId = ExerciseWeightTypeId.New();
        SetupWeightType(unknownId, "UNKNOWN_CODE");
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(unknownId, 50m);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Unknown weight type code: UNKNOWN_CODE", result.ErrorMessage);
    }
    
    [Fact]
    public async Task ValidateWeightByTypeAsync_WeightTypeNotFound_ReturnsFailure()
    {
        // Arrange
        var unknownId = ExerciseWeightTypeId.New();
        _mockWeightTypeRepository
            .Setup(r => r.GetByIdAsync(unknownId))
            .ReturnsAsync((ExerciseWeightType?)null);
        
        // Act
        var result = await _validator.ValidateWeightByTypeAsync(unknownId, 50m);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains($"Exercise weight type with ID {unknownId} not found", result.ErrorMessage);
    }
    
    #endregion
    
    #region WeightValidationResult Tests
    
    [Fact]
    public void WeightValidationResult_Success_CreatesValidResult()
    {
        // Act
        var result = WeightValidationResult.Success();
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
    
    [Fact]
    public void WeightValidationResult_Failure_CreatesInvalidResult()
    {
        // Arrange
        var errorMessage = "Test error message";
        
        // Act
        var result = WeightValidationResult.Failure(errorMessage);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(errorMessage, result.ErrorMessage);
    }
    
    #endregion
    
    private void SetupWeightType(ExerciseWeightTypeId id, string code)
    {
        var weightType = ExerciseWeightType.Handler.Create(
            id,
            code,
            $"{code} Name",
            $"{code} Description",
            1,
            true
        );
        
        _mockWeightTypeRepository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(weightType);
    }
    
    private Exercise CreateExercise(ExerciseId id, ExerciseWeightTypeId? weightTypeId)
    {
        return Exercise.Handler.Create(
            id,
            "Test Exercise",
            "Test Description",
            null,
            null,
            false,
            true,
            DifficultyLevelId.New(),
            KineticChainTypeId.New(),
            weightTypeId
        );
    }
}