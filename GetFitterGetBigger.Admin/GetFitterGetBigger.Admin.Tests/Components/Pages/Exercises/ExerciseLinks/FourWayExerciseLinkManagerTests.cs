using Bunit;
using GetFitterGetBigger.Admin.Components.Pages.Exercises.ExerciseLinks;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises.ExerciseLinks
{
    /// <summary>
    /// Tests for FourWayExerciseLinkManager component
    /// </summary>
    public class FourWayExerciseLinkManagerTests : TestContext
    {
        private readonly Mock<IExerciseLinkStateService> _mockStateService;
        private readonly Mock<IExerciseService> _mockExerciseService;
        private readonly Mock<IExerciseLinkValidationService> _mockValidationService;

        public FourWayExerciseLinkManagerTests()
        {
            _mockStateService = new Mock<IExerciseLinkStateService>();
            _mockExerciseService = new Mock<IExerciseService>();
            _mockValidationService = new Mock<IExerciseLinkValidationService>();

            Services.AddSingleton(_mockStateService.Object);
            Services.AddSingleton(_mockExerciseService.Object);
            Services.AddSingleton(_mockValidationService.Object);

            // Setup default state service behavior
            _mockStateService.Setup(s => s.IsLoading).Returns(false);
            _mockStateService.Setup(s => s.IsProcessingLink).Returns(false);
            _mockStateService.Setup(s => s.IsSaving).Returns(false);
            _mockStateService.Setup(s => s.IsDeleting).Returns(false);
            _mockStateService.Setup(s => s.ErrorMessage).Returns((string?)null);
            _mockStateService.Setup(s => s.ActiveContext).Returns("Workout");
            _mockStateService.Setup(s => s.WarmupLinks).Returns(new List<ExerciseLinkDto>());
            _mockStateService.Setup(s => s.CooldownLinks).Returns(new List<ExerciseLinkDto>());
            _mockStateService.Setup(s => s.AlternativeLinks).Returns(new List<ExerciseLinkDto>());
            _mockStateService.Setup(s => s.WorkoutLinks).Returns(new List<ExerciseLinkDto>());
            _mockStateService.Setup(s => s.CurrentLinks).Returns(new ExerciseLinksResponseDto
            {
                Links = new List<ExerciseLinkDto>()
            });
        }

        [Fact]
        public void Component_Should_Not_Render_For_Exercise_Without_Types()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Test Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>() // No types
            };

            var exerciseTypes = new List<ExerciseTypeDto>();

            // Act
            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Assert
            var mainContainer = component.Find("[data-testid='four-way-exercise-link-manager']");
            Assert.Empty(mainContainer.Children);
        }

        [Fact]
        public void Component_Should_Show_REST_Restriction_For_REST_Exercise()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Rest Period",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "REST" }
                }
            };

            var exerciseTypes = new List<ExerciseTypeDto>();

            // Act
            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Assert
            var restMessage = component.Find("div.bg-gray-50");
            Assert.Contains("REST Exercise", restMessage.TextContent);
            Assert.Contains("REST exercises cannot have relationships", restMessage.TextContent);
        }

        [Fact]
        public void Component_Should_Show_Context_Selector_For_Multi_Type_Exercise()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Multi-Type Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" },
                    new() { Id = "2", Value = "Warmup" }
                }
            };

            var exerciseTypes = new List<ExerciseTypeDto>();

            // Act
            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Assert
            var contextSelector = component.Find("[data-testid='context-selector']");
            Assert.NotNull(contextSelector);
            var workoutTab = component.Find("[data-testid='context-tab-workout']");
            var warmupTab = component.Find("[data-testid='context-tab-warmup']");
            Assert.NotNull(workoutTab);
            Assert.NotNull(warmupTab);
        }

        [Fact]
        public void Component_Should_Not_Show_Context_Selector_For_Single_Type_Exercise()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Single-Type Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" }
                }
            };

            var exerciseTypes = new List<ExerciseTypeDto>();

            // Act
            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Assert
            var contextSelectors = component.FindAll("[data-testid='context-selector']");
            Assert.Empty(contextSelectors);
        }

        [Fact]
        public void Component_Should_Show_Loading_State()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Test Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" }
                }
            };

            _mockStateService.Setup(s => s.IsLoading).Returns(true);

            var exerciseTypes = new List<ExerciseTypeDto>();

            // Act
            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Assert
            var loadingSpinner = component.Find("[data-testid='loading-spinner']");
            Assert.NotNull(loadingSpinner);
            Assert.Contains("animate-spin", loadingSpinner.InnerHtml);
        }

        [Fact]
        public void Component_Should_Show_Error_State()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Test Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" }
                }
            };

            _mockStateService.Setup(s => s.ErrorMessage).Returns("Test error message");

            var exerciseTypes = new List<ExerciseTypeDto>();

            // Act
            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Assert
            var errorMessage = component.Find("[data-testid='error-message']");
            Assert.NotNull(errorMessage);
            Assert.Contains("Test error message", errorMessage.TextContent);
            
            var retryButton = component.Find("button");
            Assert.Contains("Try again", retryButton.TextContent);
        }

        [Fact]
        public void Component_Should_Show_Workout_Context_View_By_Default()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Test Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" }
                }
            };

            var exerciseTypes = new List<ExerciseTypeDto>();

            // Act
            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Assert
            var workoutContextView = component.Find("[data-testid='workout-context-view']");
            Assert.NotNull(workoutContextView);
        }

        [Fact]
        public void Component_Should_Show_Warmup_Context_View_When_Active()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Multi-Type Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" },
                    new() { Id = "2", Value = "Warmup" }
                }
            };

            _mockStateService.Setup(s => s.ActiveContext).Returns("Warmup");

            var exerciseTypes = new List<ExerciseTypeDto>();

            // Act
            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Assert
            var warmupContextView = component.Find("[data-testid='warmup-context-view']");
            Assert.NotNull(warmupContextView);
            
            var workoutLinksSection = component.Find("[data-testid='workout-links-section']");
            Assert.NotNull(workoutLinksSection);
            Assert.Contains("Workouts Using This Warmup", workoutLinksSection.TextContent);

            var alternativeWarmupsSection = component.Find("[data-testid='alternative-warmups-section']");
            Assert.NotNull(alternativeWarmupsSection);
            Assert.Contains("Alternative Warmups", alternativeWarmupsSection.TextContent);
        }

        [Fact]
        public void Component_Should_Show_Cooldown_Context_View_When_Active()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Multi-Type Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" },
                    new() { Id = "2", Value = "Cooldown" }
                }
            };

            _mockStateService.Setup(s => s.ActiveContext).Returns("Cooldown");

            var exerciseTypes = new List<ExerciseTypeDto>();

            // Act
            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Assert
            var cooldownContextView = component.Find("[data-testid='cooldown-context-view']");
            Assert.NotNull(cooldownContextView);
            
            var workoutLinksSection = component.Find("[data-testid='workout-links-section']");
            Assert.NotNull(workoutLinksSection);
            Assert.Contains("Workouts Using This Cooldown", workoutLinksSection.TextContent);

            var alternativeCooldownsSection = component.Find("[data-testid='alternative-cooldowns-section']");
            Assert.NotNull(alternativeCooldownsSection);
            Assert.Contains("Alternative Cooldowns", alternativeCooldownsSection.TextContent);
        }

        [Fact]
        public async Task Component_Should_Handle_Context_Switch()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Multi-Type Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" },
                    new() { Id = "2", Value = "Warmup" }
                }
            };

            var exerciseTypes = new List<ExerciseTypeDto>();

            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Act
            var warmupTab = component.Find("[data-testid='context-tab-warmup']");
            await warmupTab.ClickAsync(new MouseEventArgs());

            // Assert
            _mockStateService.Verify(s => s.SwitchContextAsync("Warmup"), Times.Once);
        }

        [Fact]
        public void Component_Should_Show_Success_Notification()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Test Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" }
                }
            };

            var exerciseTypes = new List<ExerciseTypeDto>();

            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Act - Set success message through reflection to test display
            var componentInstance = component.Instance;
            var successMessageField = componentInstance.GetType().GetField("_successMessage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            successMessageField?.SetValue(componentInstance, "Test success message");
            component.Render();

            // Assert
            var successNotification = component.Find("[data-testid='success-notification']");
            Assert.NotNull(successNotification);
            Assert.Contains("Test success message", successNotification.TextContent);

            var dismissButton = successNotification.QuerySelector("button");
            Assert.NotNull(dismissButton);
        }

        [Fact]
        public async Task Component_Should_Initialize_State_Service_On_Startup()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Test Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" }
                }
            };

            var exerciseTypes = new List<ExerciseTypeDto>();

            // Act
            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Wait for initialization
            await Task.Delay(100);

            // Assert
            _mockStateService.Verify(s => s.InitializeForExerciseAsync("1", "Test Exercise"), Times.Once);
        }

        [Fact]
        public async Task Component_Should_Handle_Exercise_Parameter_Change()
        {
            // Arrange
            var exercise1 = new ExerciseDto
            {
                Id = "1",
                Name = "Exercise 1",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" }
                }
            };

            var exercise2 = new ExerciseDto
            {
                Id = "2",
                Name = "Exercise 2",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" }
                }
            };

            _mockStateService.Setup(s => s.CurrentExerciseId).Returns("1");

            var exerciseTypes = new List<ExerciseTypeDto>();

            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise1)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Act - Update exercise parameter
            component.SetParametersAndRender(parameters => parameters
                .Add(p => p.Exercise, exercise2));

            await Task.Delay(100);

            // Assert
            _mockStateService.Verify(s => s.InitializeForExerciseAsync("2", "Exercise 2"), Times.Once);
        }

        [Fact]
        public void Component_Should_Dispose_Properly()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Test Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" }
                }
            };

            var exerciseTypes = new List<ExerciseTypeDto>();

            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Act
            component.Dispose();

            // Assert - Component should dispose without errors
            // Component is still available for further testing
            Assert.NotNull(component);
        }

        [Fact]
        public void Component_Should_Have_Proper_ARIA_Attributes()
        {
            // Arrange
            var exercise = new ExerciseDto
            {
                Id = "1",
                Name = "Test Exercise",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new() { Id = "1", Value = "Workout" }
                }
            };

            var exerciseTypes = new List<ExerciseTypeDto>();

            // Setup state service to have a screen reader announcement
            _mockStateService.SetupGet(s => s.ScreenReaderAnnouncement).Returns("Test announcement for accessibility");

            // Act
            var component = RenderComponent<FourWayExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExerciseTypes, exerciseTypes));

            // Assert - Check for ARIA live region
            var ariaLiveRegion = component.FindAll("[aria-live]");
            Assert.NotEmpty(ariaLiveRegion);
            
            // Verify the aria-live attribute value
            var liveRegion = ariaLiveRegion.First();
            Assert.Equal("assertive", liveRegion.GetAttribute("aria-live"));
            Assert.Equal("status", liveRegion.GetAttribute("role"));
            Assert.Equal("true", liveRegion.GetAttribute("aria-atomic"));
        }
    }
}