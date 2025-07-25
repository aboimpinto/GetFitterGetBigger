using Bunit;
using GetFitterGetBigger.Admin.Components.Shared;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace GetFitterGetBigger.Admin.Tests.Components.Shared
{
    public class WorkoutTemplateFormSkeletonTests : TestContext
    {
        [Fact]
        public void Should_render_skeleton_with_animation()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateFormSkeleton>();
            
            // Assert
            cut.Find(".animate-pulse").Should().NotBeNull();
        }

        [Fact]
        public void Should_render_form_title_skeleton()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateFormSkeleton>();
            
            // Assert
            var formContainer = cut.Find(".bg-white.rounded-lg.shadow-md.p-6");
            var titleSkeleton = formContainer.QuerySelector(".h-8.bg-gray-300.rounded.w-64.mb-6");
            titleSkeleton.Should().NotBeNull();
        }

        [Fact]
        public void Should_render_basic_information_section()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateFormSkeleton>();
            
            // Assert
            var sections = cut.FindAll(".mb-8");
            sections.Should().HaveCountGreaterThan(0);
            
            var basicInfoSection = sections[0];
            
            // Should have section title
            basicInfoSection.QuerySelector(".h-6.bg-gray-200.rounded.w-48.mb-4").Should().NotBeNull();
            
            // Should have form fields grid
            var grid = basicInfoSection.QuerySelector(".grid.grid-cols-1.md\\:grid-cols-2.gap-6");
            grid.Should().NotBeNull();
            
            // Should have name and category fields
            var fields = grid.QuerySelectorAll(":scope > div");
            fields.Should().HaveCountGreaterThanOrEqualTo(2);
            
            // Should have description field spanning two columns
            var descriptionField = basicInfoSection.QuerySelector(".md\\:col-span-2");
            descriptionField.Should().NotBeNull();
            descriptionField.QuerySelector(".h-24.bg-gray-100.rounded.w-full").Should().NotBeNull();
        }

        [Fact]
        public void Should_render_training_parameters_section()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateFormSkeleton>();
            
            // Assert
            var sections = cut.FindAll(".mb-8");
            var trainingSection = sections[1];
            
            // Should have section title
            trainingSection.QuerySelector(".h-6.bg-gray-200.rounded.w-48.mb-4").Should().NotBeNull();
            
            // Should have 3 parameter fields
            var parameterFields = trainingSection.QuerySelectorAll(".grid.grid-cols-1.md\\:grid-cols-3.gap-6 > div");
            parameterFields.Should().HaveCount(3);
            
            foreach (var field in parameterFields)
            {
                field.QuerySelector(".h-4.bg-gray-200.rounded.w-32.mb-2").Should().NotBeNull();
                field.QuerySelector(".h-10.bg-gray-100.rounded.w-full").Should().NotBeNull();
            }
        }

        [Fact]
        public void Should_render_settings_section()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateFormSkeleton>();
            
            // Assert
            var sections = cut.FindAll(".mb-8");
            var settingsSection = sections[2];
            
            // Should have section title
            settingsSection.QuerySelector(".h-6.bg-gray-200.rounded.w-32.mb-4").Should().NotBeNull();
            
            // Should have 2 checkbox settings
            var checkboxSettings = settingsSection.QuerySelectorAll(".flex.items-center.gap-3");
            checkboxSettings.Should().HaveCount(2);
            
            foreach (var setting in checkboxSettings)
            {
                setting.QuerySelector(".h-5.w-5.bg-gray-200.rounded").Should().NotBeNull();
                setting.QuerySelector(".h-4.bg-gray-200.rounded.w-48").Should().NotBeNull();
            }
        }

        [Fact]
        public void Should_render_auto_save_indicator_skeleton()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateFormSkeleton>();
            
            // Assert
            var autoSaveIndicator = cut.Find(".h-4.bg-gray-100.rounded.w-32.mb-6");
            autoSaveIndicator.Should().NotBeNull();
        }

        [Fact]
        public void Should_render_form_buttons_skeleton()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateFormSkeleton>();
            
            // Assert
            var buttonsContainer = cut.Find(".flex.gap-4");
            buttonsContainer.Should().NotBeNull();
            
            var buttons = buttonsContainer.QuerySelectorAll(".h-10.bg-gray-200.rounded.w-24");
            buttons.Should().HaveCount(2); // Save and Cancel buttons
        }

        [Fact]
        public void Should_render_all_form_field_labels()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateFormSkeleton>();
            
            // Assert
            // Count all field label skeletons
            var fieldLabels = cut.FindAll(".h-4.bg-gray-200.rounded.mb-2");
            fieldLabels.Should().HaveCountGreaterThanOrEqualTo(5); // Name, Category, Description, 3 training parameters
        }

        [Fact]
        public void Should_use_proper_responsive_classes()
        {
            // Act
            var cut = RenderComponent<WorkoutTemplateFormSkeleton>();
            
            // Assert
            // Check basic info grid
            var basicInfoGrid = cut.Find(".grid.grid-cols-1.md\\:grid-cols-2.gap-6");
            basicInfoGrid.Should().NotBeNull();
            
            // Check training parameters grid
            var trainingGrid = cut.Find(".grid.grid-cols-1.md\\:grid-cols-3.gap-6");
            trainingGrid.Should().NotBeNull();
        }
    }
}