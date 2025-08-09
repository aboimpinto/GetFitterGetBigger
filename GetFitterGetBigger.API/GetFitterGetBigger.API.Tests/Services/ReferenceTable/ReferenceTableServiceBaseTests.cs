using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTable;
using Microsoft.Extensions.Logging;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services.ReferenceTable;

public class ReferenceTableServiceBaseTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _readOnlyUnitOfWorkMock;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _writableUnitOfWorkMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly TestReferenceTableService _service;

    public ReferenceTableServiceBaseTests()
    {
        _unitOfWorkProviderMock = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _cacheServiceMock = new Mock<ICacheService>();
        _readOnlyUnitOfWorkMock = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _writableUnitOfWorkMock = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _loggerMock = new Mock<ILogger>();
        
        _unitOfWorkProviderMock
            .Setup(x => x.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);
            
        _unitOfWorkProviderMock
            .Setup(x => x.CreateWritable())
            .Returns(_writableUnitOfWorkMock.Object);

        _service = new TestReferenceTableService(
            _unitOfWorkProviderMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenCached_ReturnsFromCache()
    {
        // Arrange
        var cachedEntities = new List<TestEntity>
        {
            new() { Id = "test-1", Name = "Test 1" },
            new() { Id = "test-2", Name = "Test 2" }
        };
        
        _cacheServiceMock
            .Setup(x => x.GetAsync<IEnumerable<TestEntity>>("test:all"))
            .ReturnsAsync(cachedEntities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(cachedEntities, result);
        _cacheServiceMock.Verify(x => x.GetAsync<IEnumerable<TestEntity>>("test:all"), Times.Once);
        _unitOfWorkProviderMock.Verify(x => x.CreateReadOnly(), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_WhenNotCached_QueriesAndCaches()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Id = "test-1", Name = "Test 1" },
            new() { Id = "test-2", Name = "Test 2" }
        };
        
        _cacheServiceMock
            .Setup(x => x.GetAsync<IEnumerable<TestEntity>>("test:all"))
            .ReturnsAsync((IEnumerable<TestEntity>?)null);
            
        _service.SetupGetAllEntities(entities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(entities, result);
        _cacheServiceMock.Verify(x => x.SetAsync(
            "test:all",
            It.IsAny<IEnumerable<TestEntity>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCached_ReturnsFromCache()
    {
        // Arrange
        var cachedEntity = new TestEntity { Id = "test-1", Name = "Test 1" };
        
        _cacheServiceMock
            .Setup(x => x.GetAsync<TestEntity>("test:id:test-1"))
            .ReturnsAsync(cachedEntity);

        // Act
        var result = await _service.GetByIdAsync("test-1");

        // Assert
        Assert.Equal(cachedEntity, result);
        _unitOfWorkProviderMock.Verify(x => x.CreateReadOnly(), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotCached_QueriesAndCaches()
    {
        // Arrange
        var entity = new TestEntity { Id = "test-1", Name = "Test 1" };
        
        _cacheServiceMock
            .Setup(x => x.GetAsync<TestEntity>("test:id:test-1"))
            .ReturnsAsync((TestEntity?)null);
            
        _service.SetupGetEntityById(entity);

        // Act
        var result = await _service.GetByIdAsync("test-1");

        // Assert
        Assert.Equal(entity, result);
        _cacheServiceMock.Verify(x => x.SetAsync(
            "test:id:test-1",
            It.IsAny<TestEntity>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidatesAndCreatesEntity()
    {
        // Arrange
        var createDto = new { Name = "New Test" };
        var createdEntity = new TestEntity { Id = "test-1", Name = "New Test" };
        
        _service.SetupCreateEntity(createdEntity);
        _service.SetupValidateCreate(true);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        Assert.Equal(createdEntity, result);
        _writableUnitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync("test:*"), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntityExists_UpdatesEntity()
    {
        // Arrange
        var updateDto = new { Name = "Updated Test" };
        var updatedEntity = new TestEntity { Id = "test-1", Name = "Updated Test" };
        
        _service.SetupCheckEntityExists(true);
        _service.SetupUpdateEntity(updatedEntity);

        // Act
        var result = await _service.UpdateAsync("test-1", updateDto);

        // Assert
        Assert.Equal(updatedEntity, result);
        _writableUnitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync("test:*"), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntityNotExists_ThrowsException()
    {
        // Arrange
        var updateDto = new { Name = "Updated Test" };
        _service.SetupCheckEntityExists(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync("test-1", updateDto));
    }

    [Fact]
    public async Task DeleteAsync_WhenEntityExists_DeletesEntity()
    {
        // Arrange
        _service.SetupCheckEntityExists(true);

        // Act
        await _service.DeleteAsync("test-1");

        // Assert
        _writableUnitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync("test:*"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntityNotExists_ThrowsException()
    {
        // Arrange
        _service.SetupCheckEntityExists(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteAsync("test-1"));
    }

    // Test entity class
    private class TestEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    // Test service implementation
    private class TestReferenceTableService : ReferenceTableServiceBase<TestEntity>
    {
        private IEnumerable<TestEntity> _allEntities = new List<TestEntity>();
        private TestEntity? _entityById;
        private TestEntity? _entityByName;
        private TestEntity? _entityByValue;
        private TestEntity? _createdEntity;
        private TestEntity? _updatedEntity;
        private bool _entityExists;
        private bool _validateCreatePasses = true;

        public TestReferenceTableService(
            IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
            ICacheService cacheService,
            ILogger logger)
            : base(unitOfWorkProvider, cacheService, logger)
        {
        }

        protected override string CacheKeyPrefix => "test";
        protected override TimeSpan CacheDuration => TimeSpan.FromHours(24);

        public void SetupGetAllEntities(IEnumerable<TestEntity> entities) => _allEntities = entities;
        public void SetupGetEntityById(TestEntity? entity) => _entityById = entity;
        public void SetupGetEntityByName(TestEntity? entity) => _entityByName = entity;
        public void SetupGetEntityByValue(TestEntity? entity) => _entityByValue = entity;
        public void SetupCreateEntity(TestEntity entity) => _createdEntity = entity;
        public void SetupUpdateEntity(TestEntity entity) => _updatedEntity = entity;
        public void SetupCheckEntityExists(bool exists) => _entityExists = exists;
        public void SetupValidateCreate(bool passes) => _validateCreatePasses = passes;

        protected override Task<IEnumerable<TestEntity>> GetAllEntitiesAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork)
            => Task.FromResult(_allEntities);

        protected override Task<TestEntity?> GetEntityByIdAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string id)
            => Task.FromResult(_entityById);

        protected override Task<TestEntity?> GetEntityByNameAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string name)
            => Task.FromResult(_entityByName);

        protected override Task<TestEntity?> GetEntityByValueAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string value)
            => Task.FromResult(_entityByValue);

        protected override Task<TestEntity> CreateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, object createDto)
            => Task.FromResult(_createdEntity!);

        protected override Task<TestEntity> UpdateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id, object updateDto)
            => Task.FromResult(_updatedEntity!);

        protected override Task DeleteEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id)
            => Task.CompletedTask;

        protected override Task<bool> CheckEntityExistsAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id)
            => Task.FromResult(_entityExists);

        protected override Task ValidateCreateAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, object createDto)
        {
            if (!_validateCreatePasses)
            {
                throw new InvalidOperationException("Validation failed");
            }
            return Task.CompletedTask;
        }
    }
}