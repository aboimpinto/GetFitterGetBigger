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
using System.Linq;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises.ExerciseLinks
{
    public class SmartExerciseSelectorTests : TestContext
    {
        private readonly Mock<IExerciseService> _mockExerciseService;
        private readonly List<ExerciseTypeDto> _exerciseTypes;
        private readonly List<ExerciseLinkDto> _existingLinks;

        public SmartExerciseSelectorTests()
        {
            _mockExerciseService = new Mock<IExerciseService>();
            
            _exerciseTypes = new List<ExerciseTypeDto>
            {
                new ExerciseTypeDto { Id = "1", Value = "Workout" },
                new ExerciseTypeDto { Id = "2", Value = "Warmup" },
                new ExerciseTypeDto { Id = "3", Value = "Cooldown" },
                new ExerciseTypeDto { Id = "4", Value = "Alternative" }
            };

            _existingLinks = new List<ExerciseLinkDto>();

            Services.AddSingleton(_mockExerciseService.Object);
        }

        [Fact]
        public void AddExerciseLinkModal_ShowsAlternativeInfo_WhenLinkTypeIsAlternative()
        {
            // Arrange
            var sourceExercise = new ExerciseDto
            {
                Id = "source-1",
                Name = "Push-ups",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new ExerciseTypeDto { Id = "1", Value = "Workout" },
                    new ExerciseTypeDto { Id = "2", Value = "Warmup" }
                }
            };

            // Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Alternative")
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExistingLinks, _existingLinks)
                .Add(p => p.ExerciseTypes, _exerciseTypes)
                .Add(p => p.SourceExercise, sourceExercise));

            // Assert
            component.Markup.Should().Contain("Showing exercises compatible with Push-ups");
            component.Markup.Should().Contain("bg-purple-50");
            component.Markup.Should().Contain("text-purple-800");
            component.Find(".bg-purple-100.text-purple-700").Should().NotBeNull();
            component.Markup.Should().Contain("Workout");
            component.Markup.Should().Contain("Warmup");
        }

        [Fact]
        public void AddExerciseLinkModal_ShowsNormalInfo_WhenLinkTypeIsNotAlternative()
        {
            // Act
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExistingLinks, _existingLinks)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Assert
            component.Markup.Should().Contain("bg-blue-50");
            component.Markup.Should().Contain("text-blue-700");
            component.Markup.Should().Contain("Showing exercises that can be used as warmup exercises");
        }

        [Fact]
        public async Task AddExerciseLinkModal_FiltersAlternativeExercises_ByCompatibleTypes()
        {
            // Arrange
            var sourceExercise = new ExerciseDto
            {
                Id = "source-1",
                Name = "Push-ups",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new ExerciseTypeDto { Id = "1", Value = "Workout" }
                }
            };

            var searchResults = new List<ExerciseListDto>
            {
                // Compatible: Has Workout type
                new ExerciseListDtoBuilder()
                    .WithId("exercise-1")
                    .WithName("Incline Push-ups")
                    .AddExerciseType("Workout", "Workout exercise")
                    .Build(),
                // Not compatible: Only Warmup type
                new ExerciseListDtoBuilder()
                    .WithId("exercise-2")
                    .WithName("Arm Circles")
                    .AddExerciseType("Warmup", "Warmup exercise")
                    .Build(),
                // Compatible: Has both Workout and Warmup
                new ExerciseListDtoBuilder()
                    .WithId("exercise-3")
                    .WithName("Wall Push-ups")
                    .AddExerciseType("Workout", "Workout exercise")
                    .AddExerciseType("Warmup", "Warmup exercise")
                    .Build(),
                // Self-reference (should be excluded)
                new ExerciseListDtoBuilder()
                    .WithId("source-1")
                    .WithName("Push-ups")
                    .AddExerciseType("Workout", "Workout exercise")
                    .Build()
            };

            var mockResult = new ExercisePagedResultDto
            {
                Items = searchResults,
                TotalCount = searchResults.Count
            };

            _mockExerciseService.Setup(s => s.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                              .ReturnsAsync(mockResult);

            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Alternative")
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExistingLinks, _existingLinks)
                .Add(p => p.ExerciseTypes, _exerciseTypes)
                .Add(p => p.SourceExercise, sourceExercise));

            // Act
            var searchButton = component.Find("[data-testid='search-button']");
            await component.InvokeAsync(() => searchButton.Click());

            // Assert
            // Should show compatible exercises
            component.Markup.Should().Contain("Incline Push-ups");
            component.Markup.Should().Contain("Wall Push-ups");
            
            // Should NOT show incompatible or self-reference
            component.Markup.Should().NotContain("Arm Circles");
            component.FindAll("[data-testid='exercise-source-1']").Should().BeEmpty(); // Self-reference excluded
        }

        [Fact]
        public async Task AddExerciseLinkModal_ShowsCompatibilityScores_ForAlternativeExercises()
        {
            // Arrange
            var sourceExercise = new ExerciseDto
            {
                Id = "source-1",
                Name = "Push-ups",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new ExerciseTypeDto { Id = "1", Value = "Workout" }
                },
                MuscleGroups = new List<MuscleGroupWithRoleDto>
                {
                    new MuscleGroupWithRoleDto 
                    { 
                        MuscleGroup = new ReferenceDataDto { Id = "1", Value = "Chest" },
                        Role = new ReferenceDataDto { Id = "1", Value = "Primary" }
                    }
                },
                Difficulty = new ReferenceDataDto { Id = "1", Value = "Beginner" }
            };

            var searchResults = new List<ExerciseListDto>
            {
                // High compatibility: Same type, muscle, and difficulty
                new ExerciseListDtoBuilder()
                    .WithId("exercise-1")
                    .WithName("Incline Push-ups")
                    .WithDifficulty("Beginner", "1")
                    .WithMuscleGroups(new MuscleGroupListItemDto 
                    { 
                        MuscleGroup = new ReferenceDataDto { Id = "1", Value = "Chest" },
                        Role = new ReferenceDataDto { Id = "1", Value = "Primary" }
                    })
                    .AddExerciseType("Workout", "Workout exercise")
                    .Build(),
                // Lower compatibility: Same type but different muscle
                new ExerciseListDtoBuilder()
                    .WithId("exercise-2")
                    .WithName("Squats")
                    .WithDifficulty("Intermediate", "2")
                    .WithMuscleGroups(new MuscleGroupListItemDto 
                    { 
                        MuscleGroup = new ReferenceDataDto { Id = "2", Value = "Quadriceps" },
                        Role = new ReferenceDataDto { Id = "1", Value = "Primary" }
                    })
                    .AddExerciseType("Workout", "Workout exercise")
                    .Build()
            };

            var mockResult = new ExercisePagedResultDto
            {
                Items = searchResults,
                TotalCount = searchResults.Count
            };

            _mockExerciseService.Setup(s => s.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                              .ReturnsAsync(mockResult);

            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Alternative")
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExistingLinks, _existingLinks)
                .Add(p => p.ExerciseTypes, _exerciseTypes)
                .Add(p => p.SourceExercise, sourceExercise));

            // Act
            var searchButton = component.Find("[data-testid='search-button']");
            await component.InvokeAsync(() => searchButton.Click());

            // Assert
            component.Markup.Should().Contain("Compatibility:");
            component.Markup.Should().Contain("% match");
            
            // High compatibility exercise should have better score
            var inclinePushupSection = component.Markup.Substring(
                component.Markup.IndexOf("Incline Push-ups"),
                component.Markup.IndexOf("Squats") - component.Markup.IndexOf("Incline Push-ups"));
            inclinePushupSection.Should().Contain("Compatibility:");
        }

        [Fact]
        public async Task AddExerciseLinkModal_UsesPurpleTheme_ForAlternativeExerciseCards()
        {
            // Arrange
            var sourceExercise = new ExerciseDto
            {
                Id = "source-1",
                Name = "Push-ups",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new ExerciseTypeDto { Id = "1", Value = "Workout" }
                }
            };

            var searchResults = new List<ExerciseListDto>
            {
                new ExerciseListDtoBuilder()
                    .WithId("exercise-1")
                    .WithName("Incline Push-ups")
                    .AddExerciseType("Workout", "Workout exercise")
                    .Build()
            };

            var mockResult = new ExercisePagedResultDto
            {
                Items = searchResults,
                TotalCount = searchResults.Count
            };

            _mockExerciseService.Setup(s => s.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                              .ReturnsAsync(mockResult);

            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Alternative")
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExistingLinks, _existingLinks)
                .Add(p => p.ExerciseTypes, _exerciseTypes)
                .Add(p => p.SourceExercise, sourceExercise));

            // Act
            var searchButton = component.Find("[data-testid='search-button']");
            await component.InvokeAsync(() => searchButton.Click());

            // Select the exercise
            var exerciseCard = component.Find("[data-testid='exercise-exercise-1']");
            await component.InvokeAsync(() => exerciseCard.Click());

            // Assert
            exerciseCard.GetAttribute("class").Should().Contain("border-purple-500");
            exerciseCard.GetAttribute("class").Should().Contain("bg-purple-50");
        }

        [Fact]
        public async Task AddExerciseLinkModal_UsesBlueTheme_ForNonAlternativeExerciseCards()
        {
            // Arrange
            var searchResults = new List<ExerciseListDto>
            {
                new ExerciseListDtoBuilder()
                    .WithId("exercise-1")
                    .WithName("Arm Circles")
                    .AddExerciseType("Warmup", "Warmup exercise")
                    .Build()
            };

            var mockResult = new ExercisePagedResultDto
            {
                Items = searchResults,
                TotalCount = searchResults.Count
            };

            _mockExerciseService.Setup(s => s.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                              .ReturnsAsync(mockResult);

            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Warmup")
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExistingLinks, _existingLinks)
                .Add(p => p.ExerciseTypes, _exerciseTypes));

            // Act
            var searchButton = component.Find("[data-testid='search-button']");
            await component.InvokeAsync(() => searchButton.Click());

            // Select the exercise
            var exerciseCard = component.Find("[data-testid='exercise-exercise-1']");
            await component.InvokeAsync(() => exerciseCard.Click());

            // Assert
            exerciseCard.GetAttribute("class").Should().Contain("border-blue-500");
            exerciseCard.GetAttribute("class").Should().Contain("bg-blue-50");
        }

        [Fact]
        public async Task AddExerciseLinkModal_OrdersByCompatibilityScore_ForAlternativeExercises()
        {
            // Arrange
            var sourceExercise = new ExerciseDto
            {
                Id = "source-1",
                Name = "Push-ups",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new ExerciseTypeDto { Id = "1", Value = "Workout" }
                },
                MuscleGroups = new List<MuscleGroupWithRoleDto>
                {
                    new MuscleGroupWithRoleDto 
                    { 
                        MuscleGroup = new ReferenceDataDto { Id = "1", Value = "Chest" },
                        Role = new ReferenceDataDto { Id = "1", Value = "Primary" }
                    }
                },
                Difficulty = new ReferenceDataDto { Id = "1", Value = "Beginner" }
            };

            // Return results in reverse compatibility order
            var searchResults = new List<ExerciseListDto>
            {
                // Lower compatibility: Different muscle group
                new ExerciseListDtoBuilder()
                    .WithId("exercise-1")
                    .WithName("Squats")
                    .WithDifficulty("Intermediate", "2")
                    .WithMuscleGroups(new MuscleGroupListItemDto 
                    { 
                        MuscleGroup = new ReferenceDataDto { Id = "2", Value = "Quadriceps" },
                        Role = new ReferenceDataDto { Id = "1", Value = "Primary" }
                    })
                    .AddExerciseType("Workout", "Workout exercise")
                    .Build(),
                // Higher compatibility: Same muscle group and difficulty
                new ExerciseListDtoBuilder()
                    .WithId("exercise-2")
                    .WithName("Incline Push-ups")
                    .WithDifficulty("Beginner", "1")
                    .WithMuscleGroups(new MuscleGroupListItemDto 
                    { 
                        MuscleGroup = new ReferenceDataDto { Id = "1", Value = "Chest" },
                        Role = new ReferenceDataDto { Id = "1", Value = "Primary" }
                    })
                    .AddExerciseType("Workout", "Workout exercise")
                    .Build()
            };

            var mockResult = new ExercisePagedResultDto
            {
                Items = searchResults,
                TotalCount = searchResults.Count
            };

            _mockExerciseService.Setup(s => s.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                              .ReturnsAsync(mockResult);

            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Alternative")
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExistingLinks, _existingLinks)
                .Add(p => p.ExerciseTypes, _exerciseTypes)
                .Add(p => p.SourceExercise, sourceExercise));

            // Act
            var searchButton = component.Find("[data-testid='search-button']");
            await component.InvokeAsync(() => searchButton.Click());

            // Assert
            var resultsSection = component.Find("[data-testid='search-results']");
            var inclinePushupIndex = resultsSection.InnerHtml.IndexOf("Incline Push-ups");
            var squatsIndex = resultsSection.InnerHtml.IndexOf("Squats");
            
            // Incline Push-ups should appear before Squats (higher compatibility score)
            inclinePushupIndex.Should().BeLessThan(squatsIndex);
        }

        [Fact]
        public async Task AddExerciseLinkModal_ExcludesSourceExercise_FromAlternativeResults()
        {
            // Arrange
            var sourceExercise = new ExerciseDto
            {
                Id = "source-1",
                Name = "Push-ups",
                ExerciseTypes = new List<ExerciseTypeDto>
                {
                    new ExerciseTypeDto { Id = "1", Value = "Workout" }
                }
            };

            var searchResults = new List<ExerciseListDto>
            {
                // This is the source exercise - should be excluded
                new ExerciseListDtoBuilder()
                    .WithId("source-1")
                    .WithName("Push-ups")
                    .AddExerciseType("Workout", "Workout exercise")
                    .Build(),
                // This should be included
                new ExerciseListDtoBuilder()
                    .WithId("exercise-2")
                    .WithName("Incline Push-ups")
                    .AddExerciseType("Workout", "Workout exercise")
                    .Build()
            };

            var mockResult = new ExercisePagedResultDto
            {
                Items = searchResults,
                TotalCount = searchResults.Count
            };

            _mockExerciseService.Setup(s => s.GetExercisesAsync(It.IsAny<ExerciseFilterDto>()))
                              .ReturnsAsync(mockResult);

            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Alternative")
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExistingLinks, _existingLinks)
                .Add(p => p.ExerciseTypes, _exerciseTypes)
                .Add(p => p.SourceExercise, sourceExercise));

            // Act
            var searchButton = component.Find("[data-testid='search-button']");
            await component.InvokeAsync(() => searchButton.Click());

            // Assert
            component.Markup.Should().Contain("Incline Push-ups");
            component.FindAll("[data-testid='exercise-source-1']").Should().BeEmpty();
        }

        [Fact]
        public void AddExerciseLinkModal_ShowsCompatibilityIndicators_WithCorrectColors()
        {
            // This test verifies the compatibility score display with proper color coding
            var component = RenderComponent<AddExerciseLinkModal>(parameters => parameters
                .Add(p => p.IsOpen, true)
                .Add(p => p.LinkType, "Alternative")
                .Add(p => p.ExerciseService, _mockExerciseService.Object)
                .Add(p => p.ExistingLinks, _existingLinks)
                .Add(p => p.ExerciseTypes, _exerciseTypes)
                .Add(p => p.SourceExercise, new ExerciseDto 
                { 
                    Id = "test", 
                    Name = "Test",
                    ExerciseTypes = new List<ExerciseTypeDto>
                    {
                        new ExerciseTypeDto { Id = "1", Value = "Workout" }
                    }
                }));

            // The purple heart icon should be displayed in the info section
            var infoSection = component.Find(".bg-purple-50");
            infoSection.Should().NotBeNull();
            
            var purpleIcon = component.Find("svg.text-purple-400");
            purpleIcon.Should().NotBeNull();
        }
    }
}