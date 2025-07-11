using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Shared;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using AngleSharp.Dom;

namespace GetFitterGetBigger.Admin.Tests.Components.Shared
{
    public class ExerciseWeightTypeBadgeTests : TestContext
    {
        private readonly Mock<IExerciseWeightTypeStateService> _stateServiceMock;
        private readonly List<ExerciseWeightTypeDto> _testWeightTypes;

        public ExerciseWeightTypeBadgeTests()
        {
            _stateServiceMock = new Mock<IExerciseWeightTypeStateService>();
            _testWeightTypes = new List<ExerciseWeightTypeDto>
            {
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Code = "BODYWEIGHT_ONLY", Name = "Bodyweight Only", Description = "Uses bodyweight only", IsActive = true, DisplayOrder = 1 },
                new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Code = "NO_WEIGHT", Name = "No Weight", Description = "No weight used", IsActive = true, DisplayOrder = 2 },
                new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Code = "BODYWEIGHT_OPTIONAL", Name = "Bodyweight Optional", Description = "Weight optional", IsActive = true, DisplayOrder = 3 },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Code = "WEIGHT_REQUIRED", Name = "Weight Required", Description = "Weight required", IsActive = true, DisplayOrder = 4 },
                new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Code = "MACHINE_WEIGHT", Name = "Machine Weight", Description = "Machine weight", IsActive = true, DisplayOrder = 5 }
            };

            Services.AddSingleton(_stateServiceMock.Object);
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
        }

        [Fact]
        public void Component_RendersWithWeightTypeId()
        {
            // Act
            var component = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                .Add(p => p.WeightTypeId, "11111111-1111-1111-1111-111111111111"));

            // Assert
            var badge = component.Find("[data-testid=\"weight-type-badge\"]");
            badge.Should().NotBeNull();
            badge.TextContent.Should().Contain("Bodyweight Only");
        }

        [Fact]
        public void Component_RendersWithWeightTypeDto()
        {
            // Act
            var component = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                .Add(p => p.WeightType, _testWeightTypes[0]));

            // Assert
            var badge = component.Find("[data-testid=\"weight-type-badge\"]");
            badge.TextContent.Should().Contain("Bodyweight Only");
        }

        [Fact]
        public void Component_ShowsCorrectColorsForEachWeightType()
        {
            // Arrange
            var testCases = new[]
            {
                ("11111111-1111-1111-1111-111111111111", "bg-blue-100", "text-blue-800", "border-blue-200"),
                ("22222222-2222-2222-2222-222222222222", "bg-gray-100", "text-gray-800", "border-gray-200"),
                ("33333333-3333-3333-3333-333333333333", "bg-green-100", "text-green-800", "border-green-200"),
                ("44444444-4444-4444-4444-444444444444", "bg-orange-100", "text-orange-800", "border-orange-200"),
                ("55555555-5555-5555-5555-555555555555", "bg-purple-100", "text-purple-800", "border-purple-200")
            };

            foreach (var (id, bgColor, textColor, borderColor) in testCases)
            {
                // Act
                var component = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                    .Add(p => p.WeightTypeId, id));

                // Assert
                var badge = component.Find("[data-testid=\"weight-type-badge\"]");
                var classes = badge.GetAttribute("class") ?? "";
                classes.Should().Contain(bgColor);
                classes.Should().Contain(textColor);
                classes.Should().Contain(borderColor);
            }
        }

        [Fact]
        public void Component_ShowsCorrectIconsForEachWeightType()
        {
            // Arrange
            var testCases = new[]
            {
                ("11111111-1111-1111-1111-111111111111", "💪"),
                ("22222222-2222-2222-2222-222222222222", "🚫"),
                ("33333333-3333-3333-3333-333333333333", "⚖️"),
                ("44444444-4444-4444-4444-444444444444", "🏋️"),
                ("55555555-5555-5555-5555-555555555555", "🎯")
            };

            foreach (var (id, expectedIcon) in testCases)
            {
                // Act
                var component = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                    .Add(p => p.WeightTypeId, id));

                // Assert
                var badge = component.Find("[data-testid=\"weight-type-badge\"]");
                badge.TextContent.Should().Contain(expectedIcon);
            }
        }

        [Fact]
        public void Component_HidesIcon_WhenShowIconIsFalse()
        {
            // Act
            var component = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                .Add(p => p.WeightTypeId, "11111111-1111-1111-1111-111111111111")
                .Add(p => p.ShowIcon, false));

            // Assert
            var badge = component.Find("[data-testid=\"weight-type-badge\"]");
            badge.TextContent.Should().NotContain("💪");
            badge.TextContent.Should().Contain("Bodyweight Only");
        }

        [Fact]
        public void Component_RendersDifferentSizes()
        {
            // Test Small
            var smallComponent = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                .Add(p => p.WeightTypeId, "11111111-1111-1111-1111-111111111111")
                .Add(p => p.Size, ExerciseWeightTypeBadge.BadgeSize.Small));

            var smallBadge = smallComponent.Find("[data-testid=\"weight-type-badge\"]");
            var smallClasses = smallBadge.GetAttribute("class") ?? "";
            smallClasses.Should().Contain("px-2");
            smallClasses.Should().Contain("py-0.5");

            // Test Medium (default)
            var mediumComponent = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                .Add(p => p.WeightTypeId, "11111111-1111-1111-1111-111111111111"));

            var mediumBadge = mediumComponent.Find("[data-testid=\"weight-type-badge\"]");
            var mediumClasses = mediumBadge.GetAttribute("class") ?? "";
            mediumClasses.Should().Contain("px-3");
            mediumClasses.Should().Contain("py-1");

            // Test Large
            var largeComponent = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                .Add(p => p.WeightTypeId, "11111111-1111-1111-1111-111111111111")
                .Add(p => p.Size, ExerciseWeightTypeBadge.BadgeSize.Large));

            var largeBadge = largeComponent.Find("[data-testid=\"weight-type-badge\"]");
            var largeClasses = largeBadge.GetAttribute("class") ?? "";
            largeClasses.Should().Contain("px-4");
            largeClasses.Should().Contain("py-2");
        }

        [Fact]
        public void Component_ShowsTooltip_WhenShowTooltipIsTrue()
        {
            // Act
            var component = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                .Add(p => p.WeightTypeId, "11111111-1111-1111-1111-111111111111"));

            // Assert
            var badge = component.Find("[data-testid=\"weight-type-badge\"]");
            badge.GetAttribute("title").Should().Be("Uses bodyweight only");
        }

        [Fact]
        public void Component_HidesTooltip_WhenShowTooltipIsFalse()
        {
            // Act
            var component = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                .Add(p => p.WeightTypeId, "11111111-1111-1111-1111-111111111111")
                .Add(p => p.ShowTooltip, false));

            // Assert
            var badge = component.Find("[data-testid=\"weight-type-badge\"]");
            badge.GetAttribute("title").Should().BeNullOrEmpty();
        }

        [Fact]
        public void Component_HasProperAriaLabel()
        {
            // Act
            var component = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                .Add(p => p.WeightTypeId, "44444444-4444-4444-4444-444444444444"));

            // Assert
            var badge = component.Find("[data-testid=\"weight-type-badge\"]");
            badge.GetAttribute("aria-label").Should().Be("Weight type: Weight Required");
        }

        [Fact]
        public void Component_RendersNothing_WhenNoWeightTypeProvided()
        {
            // Act
            var component = RenderComponent<ExerciseWeightTypeBadge>();

            // Assert
            component.FindAll("[data-testid=\"weight-type-badge\"]").Should().BeEmpty();
        }

        [Fact]
        public void Component_RendersNothing_WhenInvalidWeightTypeId()
        {
            // Act
            var component = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                .Add(p => p.WeightTypeId, "99999999-9999-9999-9999-999999999999"));

            // Assert
            component.FindAll("[data-testid=\"weight-type-badge\"]").Should().BeEmpty();
        }

        [Fact]
        public void Component_PrefersWeightTypeDto_OverWeightTypeId()
        {
            // Arrange
            var customWeightType = new ExerciseWeightTypeDto
            {
                Id = Guid.NewGuid(),
                Code = "CUSTOM",
                Name = "Custom Type",
                Description = "Custom description"
            };

            // Act - provide both WeightType and WeightTypeId
            var component = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                .Add(p => p.WeightType, customWeightType)
                .Add(p => p.WeightTypeId, "11111111-1111-1111-1111-111111111111"));

            // Assert - should use the WeightType parameter
            var badge = component.Find("[data-testid=\"weight-type-badge\"]");
            badge.TextContent.Should().Contain("Custom Type");
            badge.TextContent.Should().NotContain("Bodyweight Only");
        }

        [Fact]
        public void Component_HandlesUnknownWeightTypeCode()
        {
            // Arrange
            var unknownWeightType = new ExerciseWeightTypeDto
            {
                Id = Guid.NewGuid(),
                Code = "UNKNOWN_CODE",
                Name = "Unknown Type",
                Description = "Unknown description"
            };

            // Act
            var component = RenderComponent<ExerciseWeightTypeBadge>(parameters => parameters
                .Add(p => p.WeightType, unknownWeightType));

            // Assert
            var badge = component.Find("[data-testid=\"weight-type-badge\"]");
            badge.TextContent.Should().Contain("Unknown Type");
            badge.TextContent.Should().Contain("❓"); // Default icon
            var classes = badge.GetAttribute("class") ?? "";
            classes.Should().Contain("bg-gray-100"); // Default colors
            classes.Should().Contain("text-gray-800");
        }
    }
}