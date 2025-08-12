using FluentAssertions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.Services.Builders;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Tests.TestConstants;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class BodyPartServiceTests
    {

        [Fact]
        public async Task ExistsAsync_WithBodyPartId_WhenBodyPartExists_ReturnsTrue()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<BodyPartService>();

            var bodyPartId = BodyPartId.New();
            var bodyPart = new BodyPartBuilder()
                .WithBodyPartId(bodyPartId)
                .Build();

            automocker
                .SetupBodyPartUnitOfWork()
                .SetupCacheMiss<BodyPartDto>()
                .SetupBodyPartGetById(bodyPart);

            // Act
            var result = await testee.ExistsAsync(bodyPartId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Value.Should().BeTrue();

            automocker
                .VerifyReadOnlyUnitOfWorkCreatedOnce()
                .VerifyBodyPartGetByIdOnce(bodyPartId);
        }

        [Fact]
        public async Task ExistsAsync_WithBodyPartId_WhenBodyPartDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<BodyPartService>();

            var bodyPartId = BodyPartId.New();

            automocker
                .SetupBodyPartUnitOfWork()
                .SetupCacheMiss<BodyPartDto>()
                .SetupBodyPartGetById(BodyPart.Empty);

            // Act
            var result = await testee.ExistsAsync(bodyPartId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Value.Should().BeFalse();

            automocker
                .VerifyReadOnlyUnitOfWorkCreatedOnce()
                .VerifyBodyPartGetByIdOnce(bodyPartId);
        }

        [Fact]
        public async Task ExistsAsync_WithStringId_WhenBodyPartExists_ReturnsTrue()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<BodyPartService>();

            var bodyPartId = BodyPartId.New();
            var bodyPartIdString = bodyPartId.ToString();
            var bodyPart = new BodyPartBuilder()
                .WithBodyPartId(bodyPartId)
                .Build();

            automocker
                .SetupBodyPartUnitOfWork()
                .SetupCacheMiss<BodyPartDto>()
                .SetupBodyPartGetById(bodyPart);

            // Act
            var result = await testee.ExistsAsync(BodyPartId.ParseOrEmpty(bodyPartIdString));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Value.Should().BeTrue();

            automocker
                .VerifyReadOnlyUnitOfWorkCreatedOnce()
                .VerifyBodyPartGetByIdOnce(bodyPartId);
        }

        [Fact]
        public async Task GetAllActiveAsync_ReturnsSuccessWithBodyParts()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<BodyPartService>();

            var chest = new BodyPartBuilder()
                .WithBodyPartId(BodyPartId.New())
                .Build();

            var back = new BodyPartBuilder()
                .WithBodyPartId(BodyPartId.New())
                .Build();

            var bodyParts = new List<BodyPart> { chest, back };

            automocker
                .SetupBodyPartUnitOfWork()
                .SetupCacheMissForList<BodyPartDto>()
                .SetupBodyPartGetAllActive(bodyParts);

            // Act
            var result = await testee.GetAllActiveAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Count().Should().Be(2);
            result.Errors.Should().BeEmpty();

            automocker.VerifyBodyPartGetAllActiveOnce();
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsSuccessWithBodyPart()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<BodyPartService>();

            var bodyPartId = BodyPartId.New();
            var bodyPartIdString = bodyPartId.ToString();
            var bodyPart = new BodyPartBuilder()
                .WithBodyPartId(bodyPartId)
                .WithValue("Chest")  // Need this for assertion
                .Build();

            automocker
                .SetupBodyPartUnitOfWork()
                .SetupCacheMiss<BodyPartDto>()
                .SetupBodyPartGetById(bodyPart);

            // Act
            var result = await testee.GetByIdAsync(bodyPartIdString);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(bodyPartIdString);
            result.Data.Value.Should().Be("Chest");
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_WithBodyPartId_ReturnsSuccessWithBodyPart()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<BodyPartService>();

            var bodyPartId = BodyPartId.New();
            var bodyPart = new BodyPartBuilder()
                .WithBodyPartId(bodyPartId)
                .WithValue("Back")  // Need this for assertion
                .Build();

            automocker
                .SetupBodyPartUnitOfWork()
                .SetupCacheMiss<BodyPartDto>()
                .SetupBodyPartGetById(bodyPart);

            // Act
            var result = await testee.GetByIdAsync(bodyPartId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(bodyPartId.ToString());
            result.Data.Value.Should().Be("Back");
            result.Errors.Should().BeEmpty();

            automocker.VerifyBodyPartGetByIdOnce(bodyPartId);
        }

        [Fact]
        public async Task GetByIdAsync_WithEmptyBodyPartId_ReturnsValidationFailure()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<BodyPartService>();
            var emptyBodyPartId = BodyPartId.Empty;

            // No setup needed - service returns ValidationFailed immediately for empty IDs

            // Act
            var result = await testee.GetByIdAsync(emptyBodyPartId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Data.Should().NotBeNull();
            result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
            result.Errors.Should().Contain(BodyPartErrorMessages.InvalidIdFormat);

            // Verify the repository was NOT called (optimization - empty IDs are rejected immediately)
            automocker.VerifyBodyPartGetByIdNeverCalled();
        }

        [Fact]
        public async Task GetByIdAsync_WithEmptyString_ReturnsValidationFailure()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<BodyPartService>();
            var emptyId = "";

            // Note: The service only validates for null/empty. Format validation 
            // is handled by the controller and BodyPartId.ParseOrEmpty()

            // Act
            var result = await testee.GetByIdAsync(emptyId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Data.Should().NotBeNull();
            result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
            result.Errors.Should().Contain(BodyPartErrorMessages.InvalidIdFormat);

            automocker.VerifyBodyPartGetByIdNeverCalled();
        }

        [Fact]
        public async Task GetByIdAsync_WithNullString_ReturnsValidationFailure()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<BodyPartService>();
            string? nullId = null;

            // Act
            var result = await testee.GetByIdAsync(nullId!);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Data.Should().NotBeNull();
            result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
            result.Errors.Should().Contain(BodyPartErrorMessages.InvalidIdFormat);

            automocker.VerifyBodyPartGetByIdNeverCalled();
        }

        [Fact]
        public async Task GetByIdAsync_WithInactiveBodyPart_ReturnsNotFound()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<BodyPartService>();

            var bodyPartId = BodyPartId.New();
            var inactiveBodyPart = new BodyPartBuilder()
                .WithBodyPartId(bodyPartId)
                .WithInactiveFlag()  // Only testing inactive state
                .Build();

            automocker
                .SetupBodyPartUnitOfWork()
                .SetupCacheMiss<BodyPartDto>()
                .SetupBodyPartGetById(inactiveBodyPart);

            // Act
            var result = await testee.GetByIdAsync(bodyPartId.ToString());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Data.Should().NotBeNull();
            result.Errors.Should().NotBeEmpty();
            result.Errors[0].Should().Contain(BodyPartTestConstants.NotFoundPartialMessage);

            automocker.VerifyBodyPartGetByIdOnce(bodyPartId);
        }

        [Fact]
        public async Task GetByValueAsync_WithExistingValue_ReturnsSuccess()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<BodyPartService>();

            var value = "Chest";
            var bodyPart = new BodyPartBuilder()
                .WithBodyPartId(BodyPartId.New())
                .WithValue(value)  // Need this for test and assertion
                .Build();

            automocker
                .SetupBodyPartUnitOfWork()
                .SetupCacheMiss<BodyPartDto>()
                .SetupBodyPartGetByValue(value, bodyPart);

            // Act
            var result = await testee.GetByValueAsync(value);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Value.Should().Be(value);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByValueAsync_WithNonExistingValue_ReturnsFailure()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<BodyPartService>();

            var value = "NonExistent";

            automocker
                .SetupBodyPartUnitOfWork()
                .SetupCacheMiss<BodyPartDto>()
                .SetupBodyPartGetByValue(value, BodyPart.Empty);

            // Act
            var result = await testee.GetByValueAsync(value);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Data.Should().NotBeNull();
            result.Errors.Should().NotBeEmpty();
            result.Errors[0].Should().Contain(BodyPartTestConstants.NotFoundPartialMessage);
        }

        [Fact]
        public async Task GetByValueAsync_WithInactiveBodyPart_ReturnsNotFound()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<BodyPartService>();

            var value = "InactiveBodyPart";
            var inactiveBodyPart = new BodyPartBuilder()
                .WithBodyPartId(BodyPartId.New())
                .WithValue(value)  // Need this for test setup
                .WithInactiveFlag()  // Testing inactive state
                .Build();

            automocker
                .SetupBodyPartUnitOfWork()
                .SetupCacheMiss<BodyPartDto>()
                .SetupBodyPartGetByValue(value, inactiveBodyPart);

            // Act
            var result = await testee.GetByValueAsync(value);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Data.Should().NotBeNull();
            result.Errors.Should().NotBeEmpty();
            result.Errors[0].Should().Contain(BodyPartTestConstants.NotFoundPartialMessage);

            automocker.VerifyBodyPartGetByValueOnce(value);
        }
    }
}