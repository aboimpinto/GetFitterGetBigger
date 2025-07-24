using Bunit;
using GetFitterGetBigger.Admin.Components.Shared;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace GetFitterGetBigger.Admin.Tests.Components.Shared
{
    public class WorkoutTemplateListSkeletonTests : TestContext
    {
        [Fact]
        public void Should_render_skeleton_grid_with_animation()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateListSkeleton>();
            
            // Assert
            cut.Find(".animate-pulse").Should().NotBeNull();
        }

        [Fact]
        public void Should_render_six_skeleton_cards()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateListSkeleton>();
            
            // Assert
            var cards = cut.FindAll(".border.rounded-lg.p-4.bg-white");
            cards.Should().HaveCount(6);
        }

        [Fact]
        public void Should_render_skeleton_elements_for_each_card()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateListSkeleton>();
            
            // Assert
            var firstCard = cut.Find(".border.rounded-lg.p-4.bg-white");
            
            // Should have header with state badge
            firstCard.QuerySelector(".flex.justify-between.items-start.mb-3").Should().NotBeNull();
            
            // Should have description skeleton lines
            firstCard.QuerySelectorAll(".space-y-2.mb-4 .h-4.bg-gray-200").Should().HaveCount(2);
            
            // Should have metadata badges
            firstCard.QuerySelectorAll(".flex.flex-wrap.gap-2.mb-4 .bg-gray-200.rounded-full").Should().HaveCount(3);
            
            // Should have action buttons area
            firstCard.QuerySelector(".flex.justify-between.items-center").Should().NotBeNull();
        }

        [Fact]
        public void Should_render_pagination_skeleton()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateListSkeleton>();
            
            // Assert
            var pagination = cut.Find(".mt-6.flex.items-center.justify-between");
            pagination.Should().NotBeNull();
            
            // Should have info text skeleton
            pagination.QuerySelector(".h-4.bg-gray-200.rounded.w-48").Should().NotBeNull();
            
            // Should have pagination buttons skeleton
            var buttons = pagination.QuerySelectorAll(".flex.space-x-2 .bg-gray-200.rounded");
            buttons.Should().HaveCount(5); // Previous, 3 pages, Next
        }

        [Fact]
        public void Should_use_responsive_grid_classes()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateListSkeleton>();
            
            // Assert
            var grid = cut.Find(".grid");
            var gridClasses = grid.GetAttribute("class") ?? "";
            gridClasses.Should().Contain("grid-cols-1");
            gridClasses.Should().Contain("md:grid-cols-2");
            gridClasses.Should().Contain("lg:grid-cols-3");
        }
    }
}