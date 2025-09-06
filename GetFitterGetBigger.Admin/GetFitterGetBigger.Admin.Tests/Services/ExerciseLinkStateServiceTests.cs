using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Services.Exceptions;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Tests.Builders;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ExerciseLinkStateServiceTests
    {
        private readonly Mock<IExerciseLinkService> _exerciseLinkServiceMock;
        private readonly ExerciseLinkStateService _stateService;
        private int _stateChangeCount;

        public ExerciseLinkStateServiceTests()
        {
            _exerciseLinkServiceMock = new Mock<IExerciseLinkService>();
            _stateService = new ExerciseLinkStateService(_exerciseLinkServiceMock.Object);
            _stateService.OnChange += () => _stateChangeCount++;
            _stateChangeCount = 0;
        }

        #region Initialization Tests

        [Fact]
        public async Task InitializeForExerciseAsync_SetsExerciseContextAndLoadsLinks()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var exerciseName = "Test Exercise";
            var expectedLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithExerciseName(exerciseName)
                .WithLinks(
                    new ExerciseLinkDtoBuilder().AsWarmup().WithDisplayOrder(1).Build(),
                    new ExerciseLinkDtoBuilder().AsCooldown().WithDisplayOrder(1).Build()
                )
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exerciseId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedLinks);

            // Act
            await _stateService.InitializeForExerciseAsync(exerciseId, exerciseName);

            // Assert
            _stateService.CurrentExerciseId.Should().Be(exerciseId);
            _stateService.CurrentExerciseName.Should().Be(exerciseName);
            _stateService.CurrentLinks.Should().NotBeNull();
            _stateService.CurrentLinks!.Links.Should().HaveCount(2);
            _stateService.ErrorMessage.Should().BeNull();
            _stateService.SuccessMessage.Should().BeNull();
            _stateChangeCount.Should().BeGreaterThan(0);
        }

        #endregion

        #region LoadLinksAsync Tests

        [Fact]
        public async Task LoadLinksAsync_WithNoExerciseSelected_SetsErrorMessage()
        {
            // Act
            await _stateService.LoadLinksAsync();

            // Assert
            _stateService.ErrorMessage.Should().Be("No exercise selected");
            _stateService.IsLoadingLinks.Should().BeFalse();
            _stateChangeCount.Should().BeGreaterThan(0);
            _exerciseLinkServiceMock.Verify(x => x.GetLinksAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task LoadLinksAsync_Success_UpdatesStateCorrectly()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");
            _stateChangeCount = 0;

            var expectedLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(
                    new ExerciseLinkDtoBuilder().AsWarmup().Build(),
                    new ExerciseLinkDtoBuilder().AsCooldown().Build()
                )
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exerciseId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedLinks);

            // Act
            await _stateService.LoadLinksAsync();

            // Assert
            _stateService.CurrentLinks.Should().NotBeNull();
            _stateService.CurrentLinks!.Links.Should().HaveCount(2);
            _stateService.IsLoadingLinks.Should().BeFalse();
            _stateService.ErrorMessage.Should().BeNull();
            _stateChangeCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task LoadLinksAsync_ExerciseNotFound_SetsErrorMessage()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exerciseId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ThrowsAsync(new ExerciseNotFoundException(exerciseId));

            // Act
            await _stateService.LoadLinksAsync();

            // Assert
            _stateService.ErrorMessage.Should().Be("Exercise not found");
            _stateService.IsLoadingLinks.Should().BeFalse();
        }

        #endregion

        #region Computed Properties Tests

        [Fact]
        public async Task ComputedProperties_ReturnCorrectCounts()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var expectedLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(
                    new ExerciseLinkDtoBuilder().AsWarmup().WithDisplayOrder(1).Build(),
                    new ExerciseLinkDtoBuilder().AsWarmup().WithDisplayOrder(2).Build(),
                    new ExerciseLinkDtoBuilder().AsWarmup().WithDisplayOrder(3).Build(),
                    new ExerciseLinkDtoBuilder().AsCooldown().WithDisplayOrder(1).Build(),
                    new ExerciseLinkDtoBuilder().AsCooldown().WithDisplayOrder(2).Build()
                )
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exerciseId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedLinks);

            // Act
            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");

            // Assert
            _stateService.WarmupLinkCount.Should().Be(3);
            _stateService.CooldownLinkCount.Should().Be(2);
            _stateService.WarmupLinks.Should().HaveCount(3);
            _stateService.CooldownLinks.Should().HaveCount(2);
            _stateService.HasMaxWarmupLinks.Should().BeFalse();
            _stateService.HasMaxCooldownLinks.Should().BeFalse();
        }

        [Fact]
        public async Task HasMaxLinks_AlwaysReturnsFalse_NoLimits()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var links = new List<ExerciseLinkDto>();

            // Add many warmup links (previously would hit limit)
            for (int i = 0; i < 20; i++)
            {
                links.Add(new ExerciseLinkDtoBuilder().AsWarmup().WithDisplayOrder(i).Build());
            }

            // Add many cooldown links (previously would hit limit)
            for (int i = 0; i < 20; i++)
            {
                links.Add(new ExerciseLinkDtoBuilder().AsCooldown().WithDisplayOrder(i).Build());
            }

            var expectedLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(links.ToArray())
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exerciseId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedLinks);

            // Act
            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");

            // Assert - No limits anymore, always returns false
            _stateService.HasMaxWarmupLinks.Should().BeFalse();
            _stateService.HasMaxCooldownLinks.Should().BeFalse();
        }

        #endregion

        #region CreateLinkAsync Tests

        [Fact]
        public async Task CreateLinkAsync_WithNoExercise_SetsErrorMessage()
        {
            // Arrange
            var createDto = new CreateExerciseLinkDtoBuilder().AsWarmup().Build();

            // Act
            await _stateService.CreateLinkAsync(createDto);

            // Assert
            _stateService.ErrorMessage.Should().Be("No exercise selected");
            _exerciseLinkServiceMock.Verify(x => x.CreateBidirectionalLinkAsync(It.IsAny<string>(), It.IsAny<CreateExerciseLinkDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateLinkAsync_WithManyExistingLinks_StillAllowsMore()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var links = new List<ExerciseLinkDto>();

            // Add many warmup links (no limit anymore)
            for (int i = 0; i < 20; i++)
            {
                links.Add(new ExerciseLinkDtoBuilder().AsWarmup().WithDisplayOrder(i).Build());
            }

            var expectedLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(links.ToArray())
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exerciseId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedLinks);

            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");

            var createDto = new CreateExerciseLinkDtoBuilder().AsWarmup().Build();
            var createdLink = new ExerciseLinkDtoBuilder().AsWarmup().Build();

            _exerciseLinkServiceMock
                .Setup(x => x.CreateLinkAsync(exerciseId, It.IsAny<CreateExerciseLinkDto>()))
                .ReturnsAsync(createdLink);

            // Act
            await _stateService.CreateLinkAsync(createDto);

            // Assert - Should succeed even with many existing links
            _stateService.ErrorMessage.Should().BeNull();
            _exerciseLinkServiceMock.Verify(x => x.CreateLinkAsync(exerciseId, It.IsAny<CreateExerciseLinkDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateLinkAsync_Success_PerformsOptimisticUpdateAndReloads()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var initialLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(new ExerciseLinkDtoBuilder().AsWarmup().Build())
                .Build();

            var updatedLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(
                    new ExerciseLinkDtoBuilder().AsWarmup().Build(),
                    new ExerciseLinkDtoBuilder().AsWarmup().Build()
                )
                .Build();

            _exerciseLinkServiceMock
                .SetupSequence(x => x.GetLinksAsync(exerciseId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(initialLinks)
                .ReturnsAsync(updatedLinks);

            _exerciseLinkServiceMock
                .Setup(x => x.CreateLinkAsync(exerciseId, It.IsAny<CreateExerciseLinkDto>()))
                .ReturnsAsync(new ExerciseLinkDtoBuilder().AsWarmup().Build());

            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");
            var createDto = new CreateExerciseLinkDtoBuilder().AsWarmup().Build();

            // Act
            await _stateService.CreateLinkAsync(createDto);

            // Assert
            _stateService.CurrentLinks!.Links.Should().HaveCount(2);
            _stateService.SuccessMessage.Should().Be("Warmup link created successfully");
            _stateService.ErrorMessage.Should().BeNull();
            _stateService.IsProcessingLink.Should().BeFalse();
        }

        [Fact]
        public async Task CreateLinkAsync_DuplicateLink_HandlesErrorCorrectly()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var initialLink = new ExerciseLinkDtoBuilder().AsWarmup().Build();
            var initialLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(initialLink)
                .Build();

            // We need a new instance for the reload to work properly
            var reloadedLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(new ExerciseLinkDtoBuilder().AsWarmup().Build())
                .Build();

            _exerciseLinkServiceMock
                .SetupSequence(x => x.GetLinksAsync(exerciseId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(initialLinks)
                .ReturnsAsync(reloadedLinks); // For reload after error

            _exerciseLinkServiceMock
                .Setup(x => x.CreateLinkAsync(exerciseId, It.IsAny<CreateExerciseLinkDto>()))
                .ThrowsAsync(new DuplicateExerciseLinkException(exerciseId, "target", "Warmup"));

            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");
            var createDto = new CreateExerciseLinkDtoBuilder().AsWarmup().Build();

            // Act
            await _stateService.CreateLinkAsync(createDto);

            // Assert
            _stateService.ErrorMessage.Should().Be("This exercise is already linked. Each exercise can only be linked once per type.");
            _stateService.SuccessMessage.Should().BeNull();
            _stateService.IsProcessingLink.Should().BeFalse();
            // After error and reload, we should have the reloaded state
            _stateService.CurrentLinks!.Links.Should().HaveCount(1);
        }

        #endregion

        #region UpdateLinkAsync Tests

        [Fact]
        public async Task UpdateLinkAsync_Success_PerformsOptimisticUpdate()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var linkId = Guid.NewGuid().ToString();
            var link = new ExerciseLinkDtoBuilder()
                .WithId(linkId)
                .AsWarmup()
                .WithDisplayOrder(1)
                .Build();

            var initialLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(link)
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exerciseId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(initialLinks);

            _exerciseLinkServiceMock
                .Setup(x => x.UpdateLinkAsync(exerciseId, linkId, It.IsAny<UpdateExerciseLinkDto>()))
                .ReturnsAsync(new ExerciseLinkDtoBuilder().WithId(linkId).AsWarmup().WithDisplayOrder(2).Build());

            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");
            var updateDto = new UpdateExerciseLinkDtoBuilder().WithDisplayOrder(2).Build();

            // Act
            await _stateService.UpdateLinkAsync(linkId, updateDto);

            // Assert
            _stateService.SuccessMessage.Should().Be("Link updated successfully");
            _stateService.ErrorMessage.Should().BeNull();
            _stateService.IsProcessingLink.Should().BeFalse();
        }

        #endregion

        #region DeleteLinkAsync Tests

        [Fact]
        public async Task DeleteLinkAsync_Success_RemovesLinkOptimistically()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var linkId = Guid.NewGuid().ToString();
            var link = new ExerciseLinkDtoBuilder()
                .WithId(linkId)
                .AsWarmup()
                .Build();

            var initialLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(link)
                .Build();

            var emptyLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .Build();

            _exerciseLinkServiceMock
                .SetupSequence(x => x.GetLinksAsync(exerciseId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(initialLinks)
                .ReturnsAsync(emptyLinks);

            _exerciseLinkServiceMock
                .Setup(x => x.DeleteLinkAsync(exerciseId, linkId))
                .Returns(Task.CompletedTask);

            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");

            // Act
            await _stateService.DeleteLinkAsync(linkId);

            // Assert
            _stateService.CurrentLinks!.Links.Should().HaveCount(0);
            _stateService.SuccessMessage.Should().Be("Warmup link removed successfully");
            _stateService.ErrorMessage.Should().BeNull();
        }

        #endregion

        #region ReorderLinksAsync Tests

        [Fact]
        public async Task ReorderLinksAsync_Success_UpdatesMultipleLinks()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            var link1 = new ExerciseLinkDtoBuilder().WithId("1").AsWarmup().WithDisplayOrder(1).Build();
            var link2 = new ExerciseLinkDtoBuilder().WithId("2").AsWarmup().WithDisplayOrder(2).Build();
            var link3 = new ExerciseLinkDtoBuilder().WithId("3").AsWarmup().WithDisplayOrder(3).Build();

            var initialLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(link1, link2, link3)
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exerciseId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(initialLinks);

            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");

            var reorderMap = new Dictionary<string, int>
            {
                { "1", 3 },
                { "2", 1 },
                { "3", 2 }
            };

            // Act
            await _stateService.ReorderLinksAsync("Warmup", reorderMap);

            // Assert
            _exerciseLinkServiceMock.Verify(x => x.UpdateLinkAsync(exerciseId, It.IsAny<string>(), It.IsAny<UpdateExerciseLinkDto>()), Times.Exactly(3));
            _stateService.SuccessMessage.Should().Be("Warmup links reordered successfully");
        }

        #endregion

        #region LoadSuggestedLinksAsync Tests

        [Fact]
        public async Task LoadSuggestedLinksAsync_Success_LoadsSuggestions()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");

            var suggestions = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDtoBuilder().AsWarmup().Build(),
                new ExerciseLinkDtoBuilder().AsCooldown().Build()
            };

            _exerciseLinkServiceMock
                .Setup(x => x.GetSuggestedLinksAsync(exerciseId, 5))
                .ReturnsAsync(suggestions);

            // Act
            await _stateService.LoadSuggestedLinksAsync();

            // Assert
            _stateService.SuggestedLinks.Should().HaveCount(2);
            _stateService.IsLoadingSuggestions.Should().BeFalse();
        }

        [Fact]
        public async Task LoadSuggestedLinksAsync_Failure_SilentlyFails()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");

            _exerciseLinkServiceMock
                .Setup(x => x.GetSuggestedLinksAsync(exerciseId, 5))
                .ThrowsAsync(new Exception("API Error"));

            // Act
            await _stateService.LoadSuggestedLinksAsync();

            // Assert
            _stateService.SuggestedLinks.Should().NotBeNull();
            _stateService.SuggestedLinks.Should().BeEmpty();
            _stateService.ErrorMessage.Should().BeNull(); // No error shown for suggestions
            _stateService.IsLoadingSuggestions.Should().BeFalse();
        }

        #endregion

        #region Clear Methods Tests

        [Fact]
        public async Task ClearExerciseContext_ResetsAllState()
        {
            // Arrange
            await _stateService.InitializeForExerciseAsync("123", "Test");
            _stateService.IncludeExerciseDetails = false;
            _stateService.LinkTypeFilter = "Warmup";

            // Act
            _stateService.ClearExerciseContext();

            // Assert
            _stateService.CurrentExerciseId.Should().BeNull();
            _stateService.CurrentExerciseName.Should().BeNull();
            _stateService.CurrentLinks.Should().BeNull();
            _stateService.SuggestedLinks.Should().BeNull();
            _stateService.LinkTypeFilter.Should().BeNull();
            _stateService.ErrorMessage.Should().BeNull();
            _stateService.SuccessMessage.Should().BeNull();
        }

        [Fact]
        public async Task ClearMessages_ClearsBothErrorAndSuccess()
        {
            // Arrange
            await _stateService.InitializeForExerciseAsync("123", "Test");
            await _stateService.CreateLinkAsync(new CreateExerciseLinkDtoBuilder().Build()); // This will set error
            await _stateService.LoadLinksAsync(); // Might set success

            // Act
            _stateService.ClearMessages();

            // Assert
            _stateService.ErrorMessage.Should().BeNull();
            _stateService.SuccessMessage.Should().BeNull();
        }

        #endregion

        #region Alternative Links Tests

        [Fact]
        public async Task LoadAlternativeLinksAsync_WithNoExercise_ReturnsEarly()
        {
            // Act
            await _stateService.LoadAlternativeLinksAsync();

            // Assert
            _stateService.AlternativeLinks.Should().BeEmpty();
            _exerciseLinkServiceMock.Verify(x => x.GetLinksAsync(It.IsAny<string>(), "Alternative", It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task LoadAlternativeLinksAsync_Success_LoadsAlternativeLinks()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");

            var alternativeLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(
                    new ExerciseLinkDtoBuilder().AsAlternative().Build(),
                    new ExerciseLinkDtoBuilder().AsAlternative().Build()
                )
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exerciseId, "Alternative", It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(alternativeLinks);

            // Act
            await _stateService.LoadAlternativeLinksAsync();

            // Assert
            _stateService.AlternativeLinks.Should().HaveCount(2);
            _stateService.AlternativeLinkCount.Should().Be(2);
        }

        [Fact]
        public async Task CreateLinkAsync_AlternativeType_DoesNotCheckMaxLinks()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");

            var createDto = new CreateExerciseLinkDtoBuilder()
                .WithLinkType("Alternative")
                .WithTargetExerciseId(Guid.NewGuid().ToString())
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.CreateLinkAsync(exerciseId, It.IsAny<CreateExerciseLinkDto>()))
                .ReturnsAsync(new ExerciseLinkDtoBuilder().AsAlternative().Build());

            var alternativeLinksResponse = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(new ExerciseLinkDtoBuilder().AsAlternative().Build())
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exerciseId, "Alternative", It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(alternativeLinksResponse);

            // Act
            await _stateService.CreateLinkAsync(createDto);

            // Assert
            _stateService.ErrorMessage.Should().BeNull(); // No max limit error for alternatives
            _stateService.SuccessMessage.Should().Be("Alternative link created successfully");
            _exerciseLinkServiceMock.Verify(x => x.CreateLinkAsync(exerciseId, createDto), Times.Once);
        }

        [Fact]
        public async Task CreateBidirectionalLinkAsync_WithNonAlternativeType_SetsError()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");

            var createDto = new CreateExerciseLinkDtoBuilder().AsWarmup().Build();

            // Act
            await _stateService.CreateBidirectionalLinkAsync(createDto);

            // Assert
            _stateService.ErrorMessage.Should().Be("Bidirectional links are only supported for Alternative relationships");
            _exerciseLinkServiceMock.Verify(x => x.CreateBidirectionalLinkAsync(It.IsAny<string>(), It.IsAny<CreateExerciseLinkDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateBidirectionalLinkAsync_Success_CreatesLinkAndReloads()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");

            var createDto = new CreateExerciseLinkDtoBuilder()
                .WithLinkType("Alternative")
                .WithTargetExerciseId(Guid.NewGuid().ToString())
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.CreateLinkAsync(exerciseId, It.IsAny<CreateExerciseLinkDto>()))
                .ReturnsAsync(new ExerciseLinkDtoBuilder().AsAlternative().Build());

            // Updated response to include alternative links in the null filter response
            var allLinksResponse = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(new ExerciseLinkDtoBuilder().AsAlternative().Build())
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exerciseId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(allLinksResponse);

            // Act
            await _stateService.CreateBidirectionalLinkAsync(createDto);

            // Assert
            _stateService.SuccessMessage.Should().Be("Alternative link created (bidirectional)");
            _stateService.ScreenReaderAnnouncement.Should().Be("Alternative exercise link has been created for both exercises");
            _stateService.ErrorMessage.Should().BeNull();
            _exerciseLinkServiceMock.Verify(x => x.CreateBidirectionalLinkAsync(exerciseId, createDto), Times.Once);
            // Verify the optimized service call pattern (null filter instead of "Alternative")
            _exerciseLinkServiceMock.Verify(x => x.GetLinksAsync(exerciseId, null, It.IsAny<bool>(), It.IsAny<bool>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task DeleteBidirectionalLinkAsync_Success_RemovesLinkAndReloads()
        {
            // Arrange
            var exerciseId = Guid.NewGuid().ToString();
            await _stateService.InitializeForExerciseAsync(exerciseId, "Test");

            var linkId = Guid.NewGuid().ToString();
            var alternativeLink = new ExerciseLinkDtoBuilder()
                .AsAlternative()
                .WithId(linkId)
                .Build();

            // Manually add a link to the alternative collection for testing
            var alternativeLinksResponse = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exerciseId)
                .WithLinks(alternativeLink)
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exerciseId, "Alternative", It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(alternativeLinksResponse);

            await _stateService.LoadAlternativeLinksAsync();

            _exerciseLinkServiceMock
                .Setup(x => x.DeleteLinkAsync(exerciseId, linkId))
                .Returns(Task.CompletedTask);

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exerciseId, "Alternative", It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new ExerciseLinksResponseDtoBuilder().WithExerciseId(exerciseId).Build());

            // Act
            await _stateService.DeleteBidirectionalLinkAsync(linkId);

            // Assert
            _stateService.SuccessMessage.Should().Be("Alternative link removed (bidirectional)");
            _stateService.ScreenReaderAnnouncement.Should().Be("Alternative exercise link has been removed from both exercises");
            _stateService.ErrorMessage.Should().BeNull();
            _exerciseLinkServiceMock.Verify(x => x.DeleteBidirectionalLinkAsync(exerciseId, linkId, true), Times.Once);
        }

        #endregion

        #region Context Management Tests

        [Fact]
        public async Task InitializeForExerciseAsync_WithExerciseDto_SetsContextCorrectly()
        {
            // Arrange
            var exercise = new ExerciseDtoBuilder()
                .WithId(Guid.NewGuid().ToString())
                .WithName("Multi-Type Exercise")
                .WithExerciseTypes(("Workout", "Workout exercise type"), ("Warmup", "Warmup exercise type"))
                .Build();

            var expectedLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exercise.Id)
                .WithExerciseName(exercise.Name)
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exercise.Id, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedLinks);

            // Act
            await _stateService.InitializeForExerciseAsync(exercise);

            // Assert
            _stateService.CurrentExerciseId.Should().Be(exercise.Id);
            _stateService.CurrentExerciseName.Should().Be(exercise.Name);
            _stateService.HasMultipleContexts.Should().BeTrue();
            _stateService.AvailableContexts.Should().Contain(new[] { "Workout", "Warmup" });
            _stateService.ActiveContext.Should().Be("Workout"); // Workout is prioritized over Warmup/Cooldown
        }

        [Fact]
        public async Task SwitchContextAsync_WithInvalidContext_SetsError()
        {
            // Arrange
            var exercise = new ExerciseDtoBuilder()
                .WithExerciseTypes(("Workout", "Workout exercise type"))
                .Build();
            
            await _stateService.InitializeForExerciseAsync(exercise);

            // Act
            await _stateService.SwitchContextAsync("Warmup");

            // Assert
            _stateService.ErrorMessage.Should().Be("Invalid context type: Warmup");
        }

        [Fact]
        public async Task SwitchContextAsync_ToWorkoutContext_LoadsWorkoutAndAlternativeLinks()
        {
            // Arrange
            var exercise = new ExerciseDtoBuilder()
                .WithId(Guid.NewGuid().ToString())
                .WithExerciseTypes(("Workout", "Workout exercise type"), ("Warmup", "Warmup exercise type"))
                .Build();

            await _stateService.InitializeForExerciseAsync(exercise);

            // Updated to match optimized service pattern - all link types returned in single call with null filter
            var allLinks = new ExerciseLinksResponseDtoBuilder()
                .WithExerciseId(exercise.Id)
                .WithLinks(
                    new ExerciseLinkDtoBuilder().AsWarmup().Build(),
                    new ExerciseLinkDtoBuilder().AsAlternative().Build()
                )
                .Build();

            _exerciseLinkServiceMock
                .Setup(x => x.GetLinksAsync(exercise.Id, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(allLinks);

            // Act
            await _stateService.SwitchContextAsync("Workout");

            // Assert
            _stateService.ActiveContext.Should().Be("Workout");
            _stateService.ScreenReaderAnnouncement.Should().Be("Switched to Workout context");
            // Verify the optimized service call pattern (null filter gets all links)
            _exerciseLinkServiceMock.Verify(x => x.GetLinksAsync(exercise.Id, null, It.IsAny<bool>(), It.IsAny<bool>()), Times.AtLeastOnce);
        }

        #endregion

        #region Validation Tests

        [Fact]
        public void ValidateLinkCompatibility_SelfReference_ReturnsFailure()
        {
            // Arrange
            var exercise = new ExerciseDtoBuilder()
                .WithId("same-id")
                .WithName("Test Exercise")
                .Build();

            // Act
            var result = _stateService.ValidateLinkCompatibility(exercise, exercise, "Alternative");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Message.Should().Be("An exercise cannot be linked to itself");
        }

        [Fact]
        public void ValidateLinkCompatibility_AlternativeWithNoSharedTypes_ReturnsFailure()
        {
            // Arrange
            var sourceExercise = new ExerciseDtoBuilder()
                .WithId("source-id")
                .WithExerciseTypes(("Workout", "Workout exercise type"))
                .Build();

            var targetExercise = new ExerciseDtoBuilder()
                .WithId("target-id")
                .WithExerciseTypes(("Warmup", "Warmup exercise type"))
                .Build();

            // Act
            var result = _stateService.ValidateLinkCompatibility(sourceExercise, targetExercise, "Alternative");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Message.Should().Be("Alternative exercises must share at least one exercise type");
        }

        [Fact]
        public void ValidateLinkCompatibility_AlternativeWithSharedTypes_ReturnsSuccess()
        {
            // Arrange
            var sourceExercise = new ExerciseDtoBuilder()
                .WithId("source-id")
                .WithExerciseTypes(("Workout", "Workout exercise type"), ("Warmup", "Warmup exercise type"))
                .Build();

            var targetExercise = new ExerciseDtoBuilder()
                .WithId("target-id")
                .WithExerciseTypes(("Workout", "Workout exercise type"))
                .Build();

            // Act
            var result = _stateService.ValidateLinkCompatibility(sourceExercise, targetExercise, "Alternative");

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ValidateLinkCompatibility_NonAlternativeTypes_ReturnsSuccess()
        {
            // Arrange
            var sourceExercise = new ExerciseDtoBuilder()
                .WithId("source-id")
                .WithExerciseTypes(("Workout", "Workout exercise type"))
                .Build();

            var targetExercise = new ExerciseDtoBuilder()
                .WithId("target-id")
                .WithExerciseTypes(("Warmup", "Warmup exercise type"))
                .Build();

            // Act
            var result = _stateService.ValidateLinkCompatibility(sourceExercise, targetExercise, "Warmup");

            // Assert
            result.IsSuccess.Should().BeTrue(); // No type compatibility requirement for warmup/cooldown
        }

        #endregion

        #region State Change Notification Tests

        [Fact]
        public void StateChanges_TriggerOnChangeEvent()
        {
            // Arrange
            var eventFired = false;
            _stateService.OnChange += () => eventFired = true;

            // Act
            _stateService.ClearError();

            // Assert
            eventFired.Should().BeTrue();
        }

        #endregion
    }
}