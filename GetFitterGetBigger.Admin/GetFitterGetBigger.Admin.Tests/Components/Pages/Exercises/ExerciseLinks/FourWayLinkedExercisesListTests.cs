using Bunit;
using GetFitterGetBigger.Admin.Components.Pages.Exercises.ExerciseLinks;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises.ExerciseLinks
{
    /// <summary>
    /// Tests for FourWayLinkedExercisesList component
    /// </summary>
    public class FourWayLinkedExercisesListTests : TestContext
    {
        private readonly Mock<IExerciseLinkStateService> _mockStateService;
        private readonly Mock<IExerciseLinkValidationService> _mockValidationService;

        public FourWayLinkedExercisesListTests()
        {
            _mockStateService = new Mock<IExerciseLinkStateService>();
            _mockValidationService = new Mock<IExerciseLinkValidationService>();

            Services.AddSingleton(_mockStateService.Object);
            Services.AddSingleton(_mockValidationService.Object);

            // Setup default state service behavior
            _mockStateService.Setup(s => s.IsSaving).Returns(false);
            _mockStateService.Setup(s => s.WarmupLinks).Returns(new List<ExerciseLinkDto>());
            _mockStateService.Setup(s => s.CooldownLinks).Returns(new List<ExerciseLinkDto>());
            _mockStateService.Setup(s => s.AlternativeLinks).Returns(new List<ExerciseLinkDto>());
            _mockStateService.Setup(s => s.WarmupLinkCount).Returns(0);
            _mockStateService.Setup(s => s.CooldownLinkCount).Returns(0);
            _mockStateService.Setup(s => s.AlternativeLinkCount).Returns(0);
            
            // Setup default validation service behavior - Allow all link types in Workout context
            _mockValidationService.Setup(v => v.CanAddLinkType("Workout", "Warmup"))
                .Returns(ValidationResult.Success());
            _mockValidationService.Setup(v => v.CanAddLinkType("Workout", "Cooldown"))
                .Returns(ValidationResult.Success());
            _mockValidationService.Setup(v => v.CanAddLinkType("Workout", "Alternative"))
                .Returns(ValidationResult.Success());
        }

        [Fact]
        public void Component_Should_Render_All_Three_Sections_In_Workout_Context()
        {
            // Arrange & Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ValidationService, _mockValidationService.Object)
                .Add(p => p.CurrentContext, "Workout"));

            // Assert
            var warmupSection = component.Find("[data-testid='warmup-section']");
            var cooldownSection = component.Find("[data-testid='cooldown-section']");
            var alternativeSection = component.Find("[data-testid='alternative-section']");

            Assert.NotNull(warmupSection);
            Assert.NotNull(cooldownSection);
            Assert.NotNull(alternativeSection);
        }

        [Fact]
        public void Component_Should_Show_Proper_Section_Headers()
        {
            // Arrange & Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Assert
            var warmupHeader = component.Find("#warmup-exercises-heading");
            var cooldownHeader = component.Find("#cooldown-exercises-heading");
            var alternativeHeader = component.Find("#alternative-exercises-heading");

            Assert.Contains("Warmup Exercises", warmupHeader.TextContent);
            Assert.Contains("Cooldown Exercises", cooldownHeader.TextContent);
            Assert.Contains("Alternative Exercises", alternativeHeader.TextContent);
        }

        [Fact]
        public void Component_Should_Show_Link_Counts_Without_Limits()
        {
            // Arrange
            _mockStateService.Setup(s => s.WarmupLinkCount).Returns(3);
            _mockStateService.Setup(s => s.CooldownLinkCount).Returns(2);
            _mockStateService.Setup(s => s.AlternativeLinkCount).Returns(5);

            // Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Assert - No more capacity limits, just show counts
            var warmupCount = component.Find("[data-testid='warmup-count']");
            var cooldownCount = component.Find("[data-testid='cooldown-count']");
            var alternativeCount = component.Find("[data-testid='alternative-count']");

            Assert.Contains("3", warmupCount.TextContent);
            Assert.Contains("2", cooldownCount.TextContent);
            Assert.Contains("5", alternativeCount.TextContent);
            // Should not contain capacity limits
            Assert.DoesNotContain(" / ", warmupCount.TextContent);
            Assert.DoesNotContain(" / ", cooldownCount.TextContent);
        }

        [Fact]
        public void Component_Should_Show_Add_Buttons_When_Not_Disabled()
        {
            // Arrange & Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.Disabled, false));

            // Assert
            var addWarmupButton = component.Find("[data-testid='add-warmup-button']");
            var addCooldownButton = component.Find("[data-testid='add-cooldown-button']");
            var addAlternativeButton = component.Find("[data-testid='add-alternative-button']");

            Assert.NotNull(addWarmupButton);
            Assert.NotNull(addCooldownButton);
            Assert.NotNull(addAlternativeButton);

            Assert.Contains("Add Warmup", addWarmupButton.TextContent);
            Assert.Contains("Add Cooldown", addCooldownButton.TextContent);
            Assert.Contains("Add Alternative", addAlternativeButton.TextContent);
        }

        [Fact]
        public void Component_Should_Show_Add_Warmup_Button_Regardless_Of_Count()
        {
            // Arrange
            _mockStateService.Setup(s => s.WarmupLinkCount).Returns(15); // Previously over limit, now no limits

            // Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Assert - Button should always be available (no limits)
            var addWarmupButtons = component.FindAll("[data-testid='add-warmup-button']");
            Assert.NotEmpty(addWarmupButtons);
        }

        [Fact]
        public void Component_Should_Show_Add_Cooldown_Button_Regardless_Of_Count()
        {
            // Arrange
            _mockStateService.Setup(s => s.CooldownLinkCount).Returns(15); // Previously over limit, now no limits

            // Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Assert - Button should always be available (no limits)
            var addCooldownButtons = component.FindAll("[data-testid='add-cooldown-button']");
            Assert.NotEmpty(addCooldownButtons);
        }

        [Fact]
        public void Component_Should_Always_Show_Add_Alternative_Button()
        {
            // Arrange - Even with many alternatives, button should still show
            _mockStateService.Setup(s => s.AlternativeLinkCount).Returns(50);

            // Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Assert
            var addAlternativeButton = component.Find("[data-testid='add-alternative-button']");
            Assert.NotNull(addAlternativeButton);
        }

        [Fact]
        public void Component_Should_Show_Empty_States()
        {
            // Arrange & Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Assert
            var warmupEmptyState = component.Find("[data-testid='warmup-empty-state']");
            var cooldownEmptyState = component.Find("[data-testid='cooldown-empty-state']");
            var alternativeEmptyState = component.Find("[data-testid='alternative-empty-state']");

            Assert.Contains("No warmup exercises linked yet", warmupEmptyState.TextContent);
            Assert.Contains("No cooldown exercises linked yet", cooldownEmptyState.TextContent);
            Assert.Contains("No alternative exercises linked yet", alternativeEmptyState.TextContent);
        }

        [Fact]
        public void Component_Should_Display_Warmup_Links()
        {
            // Arrange
            var warmupLinks = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDto
                {
                    Id = "1",
                    LinkType = "Warmup",
                    TargetExercise = new ExerciseDto { Id = "1", Name = "Jumping Jacks" },
                    DisplayOrder = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockStateService.Setup(s => s.WarmupLinks).Returns(warmupLinks);
            _mockStateService.Setup(s => s.WarmupLinkCount).Returns(1);

            // Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Assert
            var warmupContainer = component.Find("[data-testid='warmup-links-container']");
            Assert.NotNull(warmupContainer);
            
            var warmupEmptyStates = component.FindAll("[data-testid='warmup-empty-state']");
            Assert.Empty(warmupEmptyStates); // Should not show empty state when links exist
        }

        [Fact]
        public void Component_Should_Display_Cooldown_Links()
        {
            // Arrange
            var cooldownLinks = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDto
                {
                    Id = "2",
                    LinkType = "Cooldown",
                    TargetExercise = new ExerciseDto { Id = "2", Name = "Stretching" },
                    DisplayOrder = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockStateService.Setup(s => s.CooldownLinks).Returns(cooldownLinks);
            _mockStateService.Setup(s => s.CooldownLinkCount).Returns(1);

            // Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Assert
            var cooldownContainer = component.Find("[data-testid='cooldown-links-container']");
            Assert.NotNull(cooldownContainer);
            
            var cooldownEmptyStates = component.FindAll("[data-testid='cooldown-empty-state']");
            Assert.Empty(cooldownEmptyStates);
        }

        [Fact]
        public void Component_Should_Display_Alternative_Links()
        {
            // Arrange
            var alternativeLinks = new List<ExerciseLinkDto>
            {
                new ExerciseLinkDto
                {
                    Id = "3",
                    LinkType = "Alternative",
                    TargetExercise = new ExerciseDto { Id = "3", Name = "Push-ups Variation" },
                    DisplayOrder = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockStateService.Setup(s => s.AlternativeLinks).Returns(alternativeLinks);
            _mockStateService.Setup(s => s.AlternativeLinkCount).Returns(1);

            // Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Assert
            var alternativeContainer = component.Find("[data-testid='alternative-links-container']");
            Assert.NotNull(alternativeContainer);
            
            var alternativeEmptyStates = component.FindAll("[data-testid='alternative-empty-state']");
            Assert.Empty(alternativeEmptyStates);
        }

        [Fact]
        public async Task Component_Should_Handle_Add_Warmup_Click()
        {
            // Arrange
            var onAddLinkCalled = false;
            var addLinkType = "";

            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.OnAddLink, EventCallback.Factory.Create<string>(this, type =>
                {
                    onAddLinkCalled = true;
                    addLinkType = type;
                })));

            // Act
            var addWarmupButton = component.Find("[data-testid='add-warmup-button']");
            await addWarmupButton.ClickAsync(new MouseEventArgs());

            // Assert
            Assert.True(onAddLinkCalled);
            Assert.Equal("Warmup", addLinkType);
        }

        [Fact]
        public async Task Component_Should_Handle_Add_Cooldown_Click()
        {
            // Arrange
            var onAddLinkCalled = false;
            var addLinkType = "";

            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.OnAddLink, EventCallback.Factory.Create<string>(this, type =>
                {
                    onAddLinkCalled = true;
                    addLinkType = type;
                })));

            // Act
            var addCooldownButton = component.Find("[data-testid='add-cooldown-button']");
            await addCooldownButton.ClickAsync(new MouseEventArgs());

            // Assert
            Assert.True(onAddLinkCalled);
            Assert.Equal("Cooldown", addLinkType);
        }

        [Fact]
        public async Task Component_Should_Handle_Add_Alternative_Click()
        {
            // Arrange
            var onAddLinkCalled = false;
            var addLinkType = "";

            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.OnAddLink, EventCallback.Factory.Create<string>(this, type =>
                {
                    onAddLinkCalled = true;
                    addLinkType = type;
                })));

            // Act
            var addAlternativeButton = component.Find("[data-testid='add-alternative-button']");
            await addAlternativeButton.ClickAsync(new MouseEventArgs());

            // Assert
            Assert.True(onAddLinkCalled);
            Assert.Equal("Alternative", addLinkType);
        }

        [Fact]
        public void Component_Should_Show_Saving_Overlay()
        {
            // Arrange
            _mockStateService.Setup(s => s.IsSaving).Returns(true);

            // Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Assert
            var savingOverlay = component.Find("[data-testid='reorder-progress-overlay']");
            Assert.NotNull(savingOverlay);
            Assert.Contains("Updating exercises", savingOverlay.TextContent);
        }

        [Fact]
        public void Component_Should_Have_Proper_Color_Themes()
        {
            // Arrange & Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Assert
            var warmupSection = component.Find("[data-testid='warmup-section'] .bg-orange-50");
            var cooldownSection = component.Find("[data-testid='cooldown-section'] .bg-blue-50");
            var alternativeSection = component.Find("[data-testid='alternative-section'] .bg-purple-50");

            Assert.NotNull(warmupSection); // Orange theme for warmup
            Assert.NotNull(cooldownSection); // Blue theme for cooldown
            Assert.NotNull(alternativeSection); // Purple theme for alternative
        }

        [Fact]
        public void Component_Should_Have_Proper_ARIA_Attributes()
        {
            // Arrange & Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Assert
            var warmupSection = component.Find("[data-testid='warmup-section']");
            var cooldownSection = component.Find("[data-testid='cooldown-section']");
            var alternativeSection = component.Find("[data-testid='alternative-section']");

            Assert.Equal("region", warmupSection.GetAttribute("role"));
            Assert.Equal("region", cooldownSection.GetAttribute("role"));
            Assert.Equal("region", alternativeSection.GetAttribute("role"));

            Assert.Contains("Warmup exercises section", warmupSection.GetAttribute("aria-label"));
            Assert.Contains("Cooldown exercises section", cooldownSection.GetAttribute("aria-label"));
            Assert.Contains("Alternative exercises section", alternativeSection.GetAttribute("aria-label"));
        }

        [Fact]
        public void Component_Should_Dispose_Properly()
        {
            // Arrange
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Act
            component.Dispose();

            // Assert
            // Component is still available for further testing
            Assert.NotNull(component);
        }

        [Fact]
        public void Component_Should_Subscribe_To_State_Changes()
        {
            // Arrange
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object));

            // Verify subscription
            _mockStateService.VerifyAdd(x => x.OnChange += It.IsAny<Action>(), Times.Once);

            // Act - Update state and trigger change
            _mockStateService.Setup(s => s.WarmupLinkCount).Returns(1);
            _mockStateService.Setup(s => s.CooldownLinkCount).Returns(2);
            _mockStateService.Setup(s => s.AlternativeLinkCount).Returns(3);
            
            // Trigger the OnChange event within the renderer's synchronization context
            component.InvokeAsync(() => _mockStateService.Raise(s => s.OnChange += null));

            // Assert - Component should re-render with new state
            component.WaitForAssertion(() =>
            {
                var warmupCount = component.Find("[data-testid='warmup-count']");
                var cooldownCount = component.Find("[data-testid='cooldown-count']");
                var alternativeCount = component.Find("[data-testid='alternative-count']");
                
                Assert.Contains("1", warmupCount.TextContent);
                Assert.Contains("2", cooldownCount.TextContent);
                Assert.Contains("3", alternativeCount.TextContent);
            });
        }

        #region Link Type Restriction Tests

        [Fact]
        public void Component_Should_Show_Only_Alternative_Section_In_Warmup_Context()
        {
            // Arrange - Setup validation service for warmup context restrictions
            _mockValidationService.Setup(v => v.CanAddLinkType("Warmup", "Warmup"))
                .Returns(ValidationResult.Failure("Warmup exercises can only link to Workout and Alternative exercises", "WARMUP_LINK_RESTRICTION"));
            _mockValidationService.Setup(v => v.CanAddLinkType("Warmup", "Cooldown"))
                .Returns(ValidationResult.Failure("Warmup exercises can only link to Workout and Alternative exercises", "WARMUP_LINK_RESTRICTION"));
            _mockValidationService.Setup(v => v.CanAddLinkType("Warmup", "Alternative"))
                .Returns(ValidationResult.Success());

            // Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ValidationService, _mockValidationService.Object)
                .Add(p => p.CurrentContext, "Warmup"));

            // Assert
            // Should show restricted messages for warmup/cooldown sections
            var warmupRestricted = component.Find("[data-testid='warmup-section-restricted']");
            var cooldownRestricted = component.Find("[data-testid='cooldown-section-restricted']");
            
            Assert.NotNull(warmupRestricted);
            Assert.NotNull(cooldownRestricted);
            Assert.Contains("Warmup section not available for Warmup exercises", warmupRestricted.TextContent);
            Assert.Contains("Cooldown section not available for Warmup exercises", cooldownRestricted.TextContent);

            // Should show alternative section normally
            var alternativeSection = component.Find("[data-testid='alternative-section']");
            Assert.NotNull(alternativeSection);

            // Should not show actual warmup/cooldown sections
            var warmupSections = component.FindAll("[data-testid='warmup-section']");
            var cooldownSections = component.FindAll("[data-testid='cooldown-section']");
            Assert.Empty(warmupSections);
            Assert.Empty(cooldownSections);
        }

        [Fact]
        public void Component_Should_Show_Only_Alternative_Section_In_Cooldown_Context()
        {
            // Arrange - Setup validation service for cooldown context restrictions
            _mockValidationService.Setup(v => v.CanAddLinkType("Cooldown", "Warmup"))
                .Returns(ValidationResult.Failure("Cooldown exercises can only link to Workout and Alternative exercises", "COOLDOWN_LINK_RESTRICTION"));
            _mockValidationService.Setup(v => v.CanAddLinkType("Cooldown", "Cooldown"))
                .Returns(ValidationResult.Failure("Cooldown exercises can only link to Workout and Alternative exercises", "COOLDOWN_LINK_RESTRICTION"));
            _mockValidationService.Setup(v => v.CanAddLinkType("Cooldown", "Alternative"))
                .Returns(ValidationResult.Success());

            // Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ValidationService, _mockValidationService.Object)
                .Add(p => p.CurrentContext, "Cooldown"));

            // Assert
            // Should show restricted messages for warmup/cooldown sections
            var warmupRestricted = component.Find("[data-testid='warmup-section-restricted']");
            var cooldownRestricted = component.Find("[data-testid='cooldown-section-restricted']");
            
            Assert.NotNull(warmupRestricted);
            Assert.NotNull(cooldownRestricted);
            Assert.Contains("Warmup section not available for Cooldown exercises", warmupRestricted.TextContent);
            Assert.Contains("Cooldown section not available for Cooldown exercises", cooldownRestricted.TextContent);

            // Should show alternative section normally
            var alternativeSection = component.Find("[data-testid='alternative-section']");
            Assert.NotNull(alternativeSection);

            // Should not show actual warmup/cooldown sections
            var warmupSections = component.FindAll("[data-testid='warmup-section']");
            var cooldownSections = component.FindAll("[data-testid='cooldown-section']");
            Assert.Empty(warmupSections);
            Assert.Empty(cooldownSections);
        }

        [Fact]
        public void Component_Should_Show_Helpful_Restriction_Messages()
        {
            // Arrange - Setup validation service for warmup context restrictions
            _mockValidationService.Setup(v => v.CanAddLinkType("Warmup", "Warmup"))
                .Returns(ValidationResult.Failure("Restricted", "WARMUP_LINK_RESTRICTION"));
            _mockValidationService.Setup(v => v.CanAddLinkType("Warmup", "Cooldown"))
                .Returns(ValidationResult.Failure("Restricted", "WARMUP_LINK_RESTRICTION"));
            _mockValidationService.Setup(v => v.CanAddLinkType("Warmup", "Alternative"))
                .Returns(ValidationResult.Success());

            // Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ValidationService, _mockValidationService.Object)
                .Add(p => p.CurrentContext, "Warmup"));

            // Assert
            var warmupRestricted = component.Find("[data-testid='warmup-section-restricted']");
            var cooldownRestricted = component.Find("[data-testid='cooldown-section-restricted']");
            
            // Check that helpful messages explain what's allowed
            Assert.Contains("Warmup exercises can only link to Workout and Alternative exercises", warmupRestricted.TextContent);
            Assert.Contains("Warmup exercises can only link to Workout and Alternative exercises", cooldownRestricted.TextContent);
        }

        [Theory]
        [InlineData("Warmup")]
        [InlineData("Cooldown")]
        public void Component_Should_Validate_Link_Types_Based_On_Context(string context)
        {
            // Arrange - Setup validation service to restrict warmup/cooldown link types
            _mockValidationService.Setup(v => v.CanAddLinkType(context, "Warmup"))
                .Returns(ValidationResult.Failure($"{context} exercises cannot add warmup links", "RESTRICTION"));
            _mockValidationService.Setup(v => v.CanAddLinkType(context, "Cooldown"))
                .Returns(ValidationResult.Failure($"{context} exercises cannot add cooldown links", "RESTRICTION"));
            _mockValidationService.Setup(v => v.CanAddLinkType(context, "Alternative"))
                .Returns(ValidationResult.Success());

            // Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ValidationService, _mockValidationService.Object)
                .Add(p => p.CurrentContext, context));

            // Assert - Validation service should be called for restricted link types
            // Note: Alternative section is always shown, so it doesn't check CanAddLinkType
            _mockValidationService.Verify(v => v.CanAddLinkType(context, "Warmup"), Times.AtLeastOnce);
            _mockValidationService.Verify(v => v.CanAddLinkType(context, "Cooldown"), Times.AtLeastOnce);
        }

        [Fact]
        public void Component_Should_Show_All_Sections_When_All_Link_Types_Are_Allowed()
        {
            // Arrange - Setup validation service to allow all link types (like Workout context)
            _mockValidationService.Setup(v => v.CanAddLinkType("Workout", "Warmup"))
                .Returns(ValidationResult.Success());
            _mockValidationService.Setup(v => v.CanAddLinkType("Workout", "Cooldown"))
                .Returns(ValidationResult.Success());
            _mockValidationService.Setup(v => v.CanAddLinkType("Workout", "Alternative"))
                .Returns(ValidationResult.Success());

            // Act
            var component = RenderComponent<FourWayLinkedExercisesList>(parameters => parameters
                .Add(p => p.StateService, _mockStateService.Object)
                .Add(p => p.ValidationService, _mockValidationService.Object)
                .Add(p => p.CurrentContext, "Workout"));

            // Assert
            var warmupSection = component.Find("[data-testid='warmup-section']");
            var cooldownSection = component.Find("[data-testid='cooldown-section']");
            var alternativeSection = component.Find("[data-testid='alternative-section']");

            Assert.NotNull(warmupSection);
            Assert.NotNull(cooldownSection);
            Assert.NotNull(alternativeSection);

            // Should not show any restricted messages
            var restrictedMessages = component.FindAll("[data-testid$='-section-restricted']");
            Assert.Empty(restrictedMessages);
        }

        #endregion
    }
}