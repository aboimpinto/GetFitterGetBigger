using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises.ExerciseLinks;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises.ExerciseLinks
{
    public class ExerciseLinkManagerValidationTests : TestContext
    {
        private readonly Mock<IExerciseLinkStateService> _stateServiceMock;
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly Mock<IExerciseLinkValidationService> _validationServiceMock;
        private readonly List<ExerciseTypeDto> _exerciseTypes;
        private readonly ExerciseDto _workoutExercise;

        public ExerciseLinkManagerValidationTests()
        {
            _stateServiceMock = new Mock<IExerciseLinkStateService>();
            _exerciseServiceMock = new Mock<IExerciseService>();
            _validationServiceMock = new Mock<IExerciseLinkValidationService>();
            
            // Register the validation service in the test context
            Services.AddSingleton(_validationServiceMock.Object);
            
            _exerciseTypes = new List<ExerciseTypeDto>
            {
                new() { Id = "1", Value = "Warmup", Description = "Warmup exercises" },
                new() { Id = "2", Value = "Workout", Description = "Main workout" },
                new() { Id = "3", Value = "Cooldown", Description = "Cooldown exercises" }
            };

            _workoutExercise = new ExerciseDtoBuilder()
                .WithId("ex1")
                .WithName("Squats")
                .WithExerciseTypes(("Workout", "Main workout"))
                .Build();
        }

        private void SetupEmptyState()
        {
            _stateServiceMock.SetupGet(x => x.CurrentExerciseId).Returns("ex1");
            _stateServiceMock.SetupGet(x => x.CurrentExerciseName).Returns("Squats");
            _stateServiceMock.SetupGet(x => x.CurrentLinks).Returns(new ExerciseLinksResponseDto
            {
                Links = new List<ExerciseLinkDto>(),
                TotalWarmupCount = 0,
                TotalCooldownCount = 0
            });
            _stateServiceMock.SetupGet(x => x.IsLoading).Returns(false);
            _stateServiceMock.SetupGet(x => x.ErrorMessage).Returns((string?)null);
        }

        [Fact]
        public async Task ExerciseLinkManager_ShowsValidationError_WhenMaxLinksReached()
        {
            // Arrange
            SetupEmptyState();
            _validationServiceMock.Setup(x => x.ValidateCreateLink(
                It.IsAny<ExerciseDto>(),
                It.IsAny<string>(),
                It.IsAny<ExerciseLinkType>(),
                It.IsAny<IEnumerable<ExerciseLinkDto>>()))
                .ReturnsAsync(ValidationResult.Failure("Maximum number of warmup links (10) has been reached", "MAX_LINKS_REACHED"));

            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Open modal and select exercise
            var linkedList = component.FindComponent<LinkedExercisesList>();
            await component.InvokeAsync(() => linkedList.Instance.OnAddLink.InvokeAsync("Warmup"));

            var modal = component.FindComponent<AddExerciseLinkModal>();
            var selectedExercise = new ExerciseListDtoBuilder()
                .WithName("Jumping Jacks")
                .Build();

            // Act
            await component.InvokeAsync(() => modal.Instance.OnAdd.InvokeAsync(selectedExercise));

            // Assert
            _stateServiceMock.Verify(x => x.SetError("Maximum number of warmup links (10) has been reached"), Times.Once);
            _stateServiceMock.Verify(x => x.CreateLinkAsync(It.IsAny<CreateExerciseLinkDto>()), Times.Never);
        }

        [Fact]
        public async Task ExerciseLinkManager_ShowsValidationError_WhenDuplicateLink()
        {
            // Arrange
            SetupEmptyState();
            _validationServiceMock.Setup(x => x.ValidateCreateLink(
                It.IsAny<ExerciseDto>(),
                It.IsAny<string>(),
                It.IsAny<ExerciseLinkType>(),
                It.IsAny<IEnumerable<ExerciseLinkDto>>()))
                .ReturnsAsync(ValidationResult.Failure("This exercise is already linked as a warmup exercise", "DUPLICATE_LINK"));

            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Open modal and select exercise
            var linkedList = component.FindComponent<LinkedExercisesList>();
            await component.InvokeAsync(() => linkedList.Instance.OnAddLink.InvokeAsync("Warmup"));

            var modal = component.FindComponent<AddExerciseLinkModal>();
            var selectedExercise = new ExerciseListDtoBuilder()
                .WithName("Arm Circles")
                .Build();

            // Act
            await component.InvokeAsync(() => modal.Instance.OnAdd.InvokeAsync(selectedExercise));

            // Assert
            _stateServiceMock.Verify(x => x.SetError("This exercise is already linked as a warmup exercise"), Times.Once);
            _stateServiceMock.Verify(x => x.CreateLinkAsync(It.IsAny<CreateExerciseLinkDto>()), Times.Never);
        }

        [Fact]
        public async Task ExerciseLinkManager_ShowsValidationError_WhenCircularReference()
        {
            // Arrange
            SetupEmptyState();
            _validationServiceMock.Setup(x => x.ValidateCreateLink(
                It.IsAny<ExerciseDto>(),
                It.IsAny<string>(),
                It.IsAny<ExerciseLinkType>(),
                It.IsAny<IEnumerable<ExerciseLinkDto>>()))
                .ReturnsAsync(ValidationResult.Failure("This would create a circular reference", "CIRCULAR_REFERENCE"));

            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Open modal and select exercise
            var linkedList = component.FindComponent<LinkedExercisesList>();
            await component.InvokeAsync(() => linkedList.Instance.OnAddLink.InvokeAsync("Cooldown"));

            var modal = component.FindComponent<AddExerciseLinkModal>();
            var selectedExercise = new ExerciseListDtoBuilder()
                .WithId("ex1")
                .WithName("Squats")
                .Build();

            // Act
            await component.InvokeAsync(() => modal.Instance.OnAdd.InvokeAsync(selectedExercise));

            // Assert
            _stateServiceMock.Verify(x => x.SetError("This would create a circular reference"), Times.Once);
            _stateServiceMock.Verify(x => x.CreateLinkAsync(It.IsAny<CreateExerciseLinkDto>()), Times.Never);
        }

        [Fact]
        public async Task ExerciseLinkManager_CreatesLink_WhenValidationSucceeds()
        {
            // Arrange
            SetupEmptyState();
            _stateServiceMock.Setup(x => x.CreateLinkAsync(It.IsAny<CreateExerciseLinkDto>()))
                .Returns(Task.CompletedTask);
            
            _validationServiceMock.Setup(x => x.ValidateCreateLink(
                It.IsAny<ExerciseDto>(),
                It.IsAny<string>(),
                It.IsAny<ExerciseLinkType>(),
                It.IsAny<IEnumerable<ExerciseLinkDto>>()))
                .ReturnsAsync(ValidationResult.Success());

            var component = RenderComponent<ExerciseLinkManager>(parameters => parameters
                .Add(p => p.Exercise, _workoutExercise)
                .Add(p => p.StateService, _stateServiceMock.Object)
                .Add(p => p.ExerciseService, _exerciseServiceMock.Object)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Open modal and select exercise
            var linkedList = component.FindComponent<LinkedExercisesList>();
            await component.InvokeAsync(() => linkedList.Instance.OnAddLink.InvokeAsync("Warmup"));

            var modal = component.FindComponent<AddExerciseLinkModal>();
            var selectedExercise = new ExerciseListDtoBuilder()
                .WithId("ex3")
                .WithName("Arm Circles")
                .Build();

            // Act
            await component.InvokeAsync(() => modal.Instance.OnAdd.InvokeAsync(selectedExercise));

            // Assert
            _stateServiceMock.Verify(x => x.CreateLinkAsync(It.Is<CreateExerciseLinkDto>(dto =>
                dto.SourceExerciseId == "ex1" &&
                dto.TargetExerciseId == "ex3" &&
                dto.LinkType == "Warmup"
            )), Times.Once);
            _stateServiceMock.Verify(x => x.SetError(It.IsAny<string>()), Times.Never);
        }
    }
}