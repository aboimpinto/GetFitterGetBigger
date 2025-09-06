using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using AngleSharp.Dom;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises
{
    public class ExerciseDetailWeightTypeTests : TestContext
    {
        private readonly Mock<IExerciseStateService> _exerciseStateServiceMock;
        private readonly Mock<IExerciseLinkStateService> _linkStateServiceMock;
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly Mock<IExerciseWeightTypeStateService> _weightTypeStateServiceMock;
        private readonly Mock<IExerciseLinkValidationService> _exerciseLinkValidationServiceMock;
        private readonly Mock<NavigationManager> _navigationManagerMock;

        public ExerciseDetailWeightTypeTests()
        {
            _exerciseStateServiceMock = new Mock<IExerciseStateService>();
            _linkStateServiceMock = new Mock<IExerciseLinkStateService>();
            _exerciseServiceMock = new Mock<IExerciseService>();
            _weightTypeStateServiceMock = new Mock<IExerciseWeightTypeStateService>();
            _exerciseLinkValidationServiceMock = new Mock<IExerciseLinkValidationService>();
            _navigationManagerMock = new Mock<NavigationManager>();

            Services.AddSingleton(_exerciseStateServiceMock.Object);
            Services.AddSingleton(_linkStateServiceMock.Object);
            Services.AddSingleton(_exerciseServiceMock.Object);
            Services.AddSingleton(_weightTypeStateServiceMock.Object);
            Services.AddSingleton(_exerciseLinkValidationServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);
        }

        [Fact]
        public void ExerciseDetail_WithWeightRequiredExercise_DisplaysWeightTypeBadgeInHeader()
        {
            // Arrange
            var weightType = ExerciseWeightTypeDtoBuilder.WeightRequired().Build();
            var exercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            _exerciseStateServiceMock.Setup(x => x.SelectedExercise).Returns(exercise);
            _exerciseStateServiceMock.Setup(x => x.IsLoadingExercise).Returns(false);
            _exerciseStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            var badges = component.FindAll(".inline-flex.items-center.px-3.py-1.rounded-full");
            badges.Should().NotBeEmpty();
            
            // Should contain weight type badge
            component.Markup.Should().Contain("Weight Required");
        }

        [Fact]
        public void ExerciseDetail_WithWeightType_DisplaysWeightTypeSection()
        {
            // Arrange
            var weightType = ExerciseWeightTypeDtoBuilder.WeightRequired()
                .WithDescription("External weight is required for this exercise")
                .Build();
            
            var exercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .WithDefaultWeight(20.0m)
                .Build();

            _exerciseStateServiceMock.Setup(x => x.SelectedExercise).Returns(exercise);
            _exerciseStateServiceMock.Setup(x => x.IsLoadingExercise).Returns(false);
            _exerciseStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            component.Markup.Should().Contain("Weight Type");
            component.Markup.Should().Contain("Weight Required");
            component.Markup.Should().Contain("External weight is required for this exercise");
            component.Markup.Should().Contain("Weight Requirements");
            component.Markup.Should().Contain("Default Weight:");
        }

        [Fact]
        public void ExerciseDetail_WithBodyweightOnlyExercise_DisplaysCorrectWeightRules()
        {
            // Arrange
            var weightType = ExerciseWeightTypeDtoBuilder.BodyweightOnly().Build();
            var exercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Push-ups")
                .WithWeightType(weightType)
                .Build();

            _exerciseStateServiceMock.Setup(x => x.SelectedExercise).Returns(exercise);
            _exerciseStateServiceMock.Setup(x => x.IsLoadingExercise).Returns(false);
            _exerciseStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            component.Markup.Should().Contain("• No additional weight can be added");
            component.Markup.Should().Contain("• Exercise uses bodyweight resistance only");
            component.Markup.Should().Contain("• Focus on form and repetition quality");
        }

        [Fact]
        public void ExerciseDetail_WithMachineWeightExercise_DisplaysCorrectWeightRules()
        {
            // Arrange
            var weightType = ExerciseWeightTypeDtoBuilder.MachineWeight().Build();
            var exercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Leg Press")
                .WithWeightType(weightType)
                .Build();

            _exerciseStateServiceMock.Setup(x => x.SelectedExercise).Returns(exercise);
            _exerciseStateServiceMock.Setup(x => x.IsLoadingExercise).Returns(false);
            _exerciseStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            component.Markup.Should().Contain("• Uses machine weight stack or pin settings");
            component.Markup.Should().Contain("• Weight refers to machine setting/stack position");
            component.Markup.Should().Contain("• Maximum recommended: 500 (stack limit)");
        }

        [Fact]
        public void ExerciseDetail_WithBodyweightOptionalExercise_DisplaysCorrectWeightRules()
        {
            // Arrange
            var weightType = ExerciseWeightTypeDtoBuilder.BodyweightOptional().Build();
            var exercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Pull-ups")
                .WithWeightType(weightType)
                .Build();

            _exerciseStateServiceMock.Setup(x => x.SelectedExercise).Returns(exercise);
            _exerciseStateServiceMock.Setup(x => x.IsLoadingExercise).Returns(false);
            _exerciseStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            component.Markup.Should().Contain("• Can be performed with bodyweight alone");
            component.Markup.Should().Contain("• Additional weight optional for increased difficulty");
            component.Markup.Should().Contain("• Recommended for progressive overload");
        }

        [Fact]
        public void ExerciseDetail_WithNoWeightExercise_DisplaysCorrectWeightRules()
        {
            // Arrange
            var weightType = ExerciseWeightTypeDtoBuilder.NoWeight().Build();
            var exercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Stretching")
                .WithWeightType(weightType)
                .Build();

            _exerciseStateServiceMock.Setup(x => x.SelectedExercise).Returns(exercise);
            _exerciseStateServiceMock.Setup(x => x.IsLoadingExercise).Returns(false);
            _exerciseStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            component.Markup.Should().Contain("• No weight required or used");
            component.Markup.Should().Contain("• Typically stretches, mobility, or cardio exercises");
            component.Markup.Should().Contain("• Equipment may be used for support or guidance");
        }

        [Fact]
        public void ExerciseDetail_WithoutWeightType_DoesNotDisplayWeightTypeSection()
        {
            // Arrange
            var exercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Generic Exercise")
                .WithWeightType((ExerciseWeightTypeDto?)null)
                .Build();

            _exerciseStateServiceMock.Setup(x => x.SelectedExercise).Returns(exercise);
            _exerciseStateServiceMock.Setup(x => x.IsLoadingExercise).Returns(false);
            _exerciseStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            component.Markup.Should().NotContain("Weight Type");
            component.Markup.Should().NotContain("Weight Requirements");
        }

        [Fact]
        public void ExerciseDetail_WithoutDefaultWeight_DoesNotDisplayDefaultWeightInfo()
        {
            // Arrange
            var weightType = ExerciseWeightTypeDtoBuilder.WeightRequired().Build();
            var exercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .WithDefaultWeight(null)
                .Build();

            _exerciseStateServiceMock.Setup(x => x.SelectedExercise).Returns(exercise);
            _exerciseStateServiceMock.Setup(x => x.IsLoadingExercise).Returns(false);
            _exerciseStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            component.Markup.Should().NotContain("Default Weight:");
        }

        [Fact]
        public void ExerciseDetail_WeightTypeSection_HasCorrectStyling()
        {
            // Arrange
            var weightType = ExerciseWeightTypeDtoBuilder.WeightRequired().Build();
            var exercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            _exerciseStateServiceMock.Setup(x => x.SelectedExercise).Returns(exercise);
            _exerciseStateServiceMock.Setup(x => x.IsLoadingExercise).Returns(false);
            _exerciseStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            component.Markup.Should().Contain("bg-gray-50 p-4 rounded-lg");
            component.Markup.Should().Contain("bg-blue-50 border border-blue-200 rounded-md p-3");
            component.Markup.Should().Contain("whitespace-pre-line");
        }

        [Fact]
        public void ExerciseDetail_WeightTypeSection_ResponsiveDesign()
        {
            // Arrange
            var weightType = ExerciseWeightTypeDtoBuilder.WeightRequired().Build();
            var exercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            _exerciseStateServiceMock.Setup(x => x.SelectedExercise).Returns(exercise);
            _exerciseStateServiceMock.Setup(x => x.IsLoadingExercise).Returns(false);
            _exerciseStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            // Verify the section maintains responsive layout structure
            component.Markup.Should().Contain("space-y-3");
            component.Markup.Should().Contain("flex items-center space-x-3");
        }

        [Fact]
        public void ExerciseDetail_WithWeightTypeDescription_DisplaysDescription()
        {
            // Arrange
            var weightType = ExerciseWeightTypeDtoBuilder.WeightRequired()
                .WithDescription("This exercise requires external weight for effective training")
                .Build();
            
            var exercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            _exerciseStateServiceMock.Setup(x => x.SelectedExercise).Returns(exercise);
            _exerciseStateServiceMock.Setup(x => x.IsLoadingExercise).Returns(false);
            _exerciseStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            component.Markup.Should().Contain("This exercise requires external weight for effective training");
        }

        [Fact]
        public void ExerciseDetail_WithEmptyWeightTypeDescription_DoesNotDisplayEmptyDescription()
        {
            // Arrange
            var weightType = ExerciseWeightTypeDtoBuilder.WeightRequired()
                .WithDescription("")
                .Build();
            
            var exercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            _exerciseStateServiceMock.Setup(x => x.SelectedExercise).Returns(exercise);
            _exerciseStateServiceMock.Setup(x => x.IsLoadingExercise).Returns(false);
            _exerciseStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ExerciseDetail>(parameters => parameters
                .Add(p => p.Id, "ex1"));

            // Assert
            // Should still display the weight type section but not the description paragraph
            component.Markup.Should().Contain("Weight Type");
            component.Markup.Should().Contain("Weight Requirements");
            // The description paragraph should not be rendered for empty descriptions
            var descriptionParagraphs = component.FindAll("p.text-gray-600");
            descriptionParagraphs.Should().BeEmpty();
        }
    }
}