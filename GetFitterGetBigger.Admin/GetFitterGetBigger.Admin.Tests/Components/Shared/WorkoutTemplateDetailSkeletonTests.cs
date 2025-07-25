using Bunit;
using GetFitterGetBigger.Admin.Components.Shared;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace GetFitterGetBigger.Admin.Tests.Components.Shared
{
    public class WorkoutTemplateDetailSkeletonTests : TestContext
    {
        [Fact]
        public void Should_render_skeleton_with_animation()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateDetailSkeleton>();
            
            // Assert
            cut.Find(".animate-pulse").Should().NotBeNull();
        }

        [Fact]
        public void Should_render_header_skeleton()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateDetailSkeleton>();
            
            // Assert
            var header = cut.Find(".bg-white.rounded-lg.shadow-md.p-6.mb-6");
            header.Should().NotBeNull();
            
            // Should have title skeleton
            header.QuerySelector(".h-8.bg-gray-300.rounded.w-64").Should().NotBeNull();
            
            // Should have description skeleton
            header.QuerySelector(".h-4.bg-gray-200.rounded.w-96").Should().NotBeNull();
            
            // Should have state badge skeleton
            header.QuerySelector(".h-8.bg-gray-200.rounded-full.w-24").Should().NotBeNull();
            
            // Should have action buttons skeleton
            var actionButtons = header.QuerySelectorAll(".flex.gap-4.mt-4 .h-10.bg-gray-200.rounded.w-24");
            actionButtons.Should().HaveCount(3);
        }

        [Fact]
        public void Should_render_metadata_card_skeleton()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateDetailSkeleton>();
            
            // Assert
            var cards = cut.FindAll(".bg-white.rounded-lg.shadow-md.p-6");
            cards.Should().HaveCountGreaterThan(1);
            
            var metadataCard = cards[1];
            
            // Should have section title
            metadataCard.QuerySelector(".h-6.bg-gray-300.rounded.w-48.mb-4").Should().NotBeNull();
            
            // Should have 6 metadata items in grid
            var metadataItems = metadataCard.QuerySelectorAll(".grid > div");
            metadataItems.Should().HaveCount(6);
            
            // Each metadata item should have label and value skeleton
            foreach (var item in metadataItems)
            {
                item.QuerySelector(".h-4.bg-gray-200.rounded.w-24.mb-2").Should().NotBeNull();
                item.QuerySelector(".h-6.bg-gray-100.rounded.w-32").Should().NotBeNull();
            }
        }

        [Fact]
        public void Should_render_exercises_card_skeleton()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateDetailSkeleton>();
            
            // Assert
            var exercisesCard = cut.FindAll(".bg-white.rounded-lg.shadow-md.p-6")[2];
            
            // Should have section title
            exercisesCard.QuerySelector(".h-6.bg-gray-300.rounded.w-48.mb-4").Should().NotBeNull();
            
            // Should have 3 exercise skeletons
            var exercises = exercisesCard.QuerySelectorAll(".border.rounded-lg.p-4");
            exercises.Should().HaveCount(3);
            
            // Each exercise should have proper skeleton structure
            foreach (var exercise in exercises)
            {
                exercise.QuerySelector(".h-5.bg-gray-200.rounded.w-48").Should().NotBeNull();
                exercise.QuerySelector(".h-4.bg-gray-100.rounded.w-12").Should().NotBeNull();
                exercise.QuerySelectorAll(".space-y-2 .h-4.bg-gray-100.rounded").Should().HaveCount(2);
            }
        }

        [Fact]
        public void Should_render_equipment_card_skeleton()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateDetailSkeleton>();
            
            // Assert
            var equipmentCard = cut.FindAll(".bg-white.rounded-lg.shadow-md.p-6").Last();
            
            // Should have section title
            equipmentCard.QuerySelector(".h-6.bg-gray-300.rounded.w-48.mb-4").Should().NotBeNull();
            
            // Should have equipment placeholder skeleton
            equipmentCard.QuerySelector(".h-16.bg-gray-100.rounded.w-full").Should().NotBeNull();
        }

        [Fact]
        public void Should_use_proper_spacing_between_cards()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateDetailSkeleton>();
            
            // Assert
            var cards = cut.FindAll(".bg-white.rounded-lg.shadow-md.p-6");
            
            // First three cards should have mb-6 class
            for (int i = 0; i < cards.Count - 1; i++)
            {
                cards[i].GetAttribute("class")?.Should().Contain("mb-6");
            }
            
            // Last card should not have mb-6
            cards.Last().GetAttribute("class")?.Should().NotContain("mb-6");
        }
    }
}