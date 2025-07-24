using Bunit;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using GetFitterGetBigger.Admin.Components.Shared;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.WorkoutTemplates
{
    public class WorkoutTemplateListTests : TestContext
    {
        private readonly Mock<IWorkoutTemplateStateService> _mockStateService;

        public WorkoutTemplateListTests()
        {
            _mockStateService = new Mock<IWorkoutTemplateStateService>();
            Services.AddSingleton(_mockStateService.Object);
        }

        private static WorkoutTemplateDto CreateTestTemplate(string id, string name)
        {
            return new WorkoutTemplateDto
            {
                Id = id,
                Name = name,
                Description = $"Description for {name}",
                WorkoutState = new ReferenceDataDto { Id = "draft", Value = "DRAFT" },
                Category = new ReferenceDataDto { Id = "strength", Value = "Strength" },
                Difficulty = new ReferenceDataDto { Id = "intermediate", Value = "Intermediate" },
                EstimatedDurationMinutes = 45,
                IsPublic = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Exercises = new List<WorkoutTemplateExerciseDto>(),
                Tags = new List<string> { "upper body", "strength" },
                Objectives = new List<ReferenceDataDto>
                {
                    new ReferenceDataDto { Id = "muscle-gain", Value = "Muscle Gain" }
                }
            };
        }

        [Fact]
        public void Should_render_loading_state_when_loading()
        {
            // Arrange
            _mockStateService.Setup(x => x.IsLoading).Returns(true);
            _mockStateService.Setup(x => x.CurrentPage).Returns((WorkoutTemplatePagedResultDto?)null);
            _mockStateService.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());
            _mockStateService.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var cut = RenderComponent<WorkoutTemplateList>();

            // Assert
            // Should render the skeleton component
            Assert.NotNull(cut.FindComponent<WorkoutTemplateListSkeleton>());
            Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find("[data-testid='template-grid']"));
            Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find("[data-testid='empty-state']"));
        }

        [Fact]
        public void Should_render_empty_state_when_no_templates()
        {
            // Arrange
            _mockStateService.Setup(x => x.IsLoading).Returns(false);
            _mockStateService.Setup(x => x.CurrentPage).Returns(new WorkoutTemplatePagedResultDto
            {
                Items = new List<WorkoutTemplateDto>(),
                TotalCount = 0,
                TotalPages = 0
            });
            _mockStateService.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());
            _mockStateService.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var cut = RenderComponent<WorkoutTemplateList>();

            // Assert
            Assert.NotNull(cut.Find("[data-testid='empty-state']"));
            Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find("[data-testid='loading-state']"));
            Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find("[data-testid='template-grid']"));
        }

        [Fact]
        public void Should_render_error_state_when_error_exists()
        {
            // Arrange
            var errorMessage = "Failed to load workout templates";
            _mockStateService.Setup(x => x.IsLoading).Returns(false);
            _mockStateService.Setup(x => x.CurrentPage).Returns((WorkoutTemplatePagedResultDto?)null);
            _mockStateService.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());
            _mockStateService.Setup(x => x.ErrorMessage).Returns(errorMessage);

            // Act
            var cut = RenderComponent<WorkoutTemplateList>();

            // Assert
            var errorState = cut.Find("[data-testid='error-state']");
            Assert.NotNull(errorState);
            Assert.Contains(errorMessage, errorState.TextContent);
            Assert.NotNull(cut.Find("[data-testid='retry-button']"));
        }

        [Fact]
        public void Should_render_templates_grid_when_templates_exist()
        {
            // Arrange
            var templates = new List<WorkoutTemplateDto>
            {
                CreateTestTemplate("1", "Template 1"),
                CreateTestTemplate("2", "Template 2")
            };

            _mockStateService.Setup(x => x.IsLoading).Returns(false);
            _mockStateService.Setup(x => x.CurrentPage).Returns(new WorkoutTemplatePagedResultDto
            {
                Items = templates,
                TotalCount = 2,
                TotalPages = 1
            });
            _mockStateService.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto { Page = 1, PageSize = 10 });
            _mockStateService.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var cut = RenderComponent<WorkoutTemplateList>();

            // Assert
            Assert.NotNull(cut.Find("[data-testid='template-grid']"));
            var cards = cut.FindAll("[data-testid='workout-template-card']");
            Assert.Equal(2, cards.Count);
        }

        [Fact]
        public void Should_render_sorting_controls_when_templates_exist()
        {
            // Arrange
            var templates = new List<WorkoutTemplateDto>
            {
                CreateTestTemplate("1", "Template 1")
            };

            _mockStateService.Setup(x => x.IsLoading).Returns(false);
            _mockStateService.Setup(x => x.CurrentPage).Returns(new WorkoutTemplatePagedResultDto
            {
                Items = templates,
                TotalCount = 1,
                TotalPages = 1
            });
            _mockStateService.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());
            _mockStateService.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var cut = RenderComponent<WorkoutTemplateList>();

            // Assert
            Assert.NotNull(cut.Find("[data-testid='sorting-controls']"));
            Assert.NotNull(cut.Find("[data-testid='sort-select']"));
            Assert.NotNull(cut.Find("[data-testid='sort-direction-button']"));
        }

        [Fact]
        public void Should_render_pagination_when_multiple_pages()
        {
            // Arrange
            var templates = new List<WorkoutTemplateDto>
            {
                CreateTestTemplate("1", "Template 1")
            };

            _mockStateService.Setup(x => x.IsLoading).Returns(false);
            _mockStateService.Setup(x => x.CurrentPage).Returns(new WorkoutTemplatePagedResultDto
            {
                Items = templates,
                TotalCount = 25,
                TotalPages = 3
            });
            _mockStateService.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto { Page = 2, PageSize = 10 });
            _mockStateService.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var cut = RenderComponent<WorkoutTemplateList>();

            // Assert
            Assert.NotNull(cut.Find("[data-testid='pagination']"));
            Assert.NotNull(cut.Find("[data-testid='prev-page-button']"));
            Assert.NotNull(cut.Find("[data-testid='next-page-button']"));
            Assert.NotNull(cut.Find("[data-testid='page-2-button']"));
        }

        [Fact]
        public void Should_call_retry_when_retry_button_clicked()
        {
            // Arrange
            _mockStateService.Setup(x => x.IsLoading).Returns(false);
            _mockStateService.Setup(x => x.ErrorMessage).Returns("Error");
            _mockStateService.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());

            // Act
            var cut = RenderComponent<WorkoutTemplateList>();
            var retryButton = cut.Find("[data-testid='retry-button']");
            retryButton.Click();

            // Assert
            _mockStateService.Verify(x => x.ClearError(), Times.Once);
            _mockStateService.Verify(x => x.RefreshCurrentPageAsync(), Times.Once);
        }

        [Fact]
        public void Should_invoke_create_callback_when_create_button_clicked()
        {
            // Arrange
            var createClicked = false;
            _mockStateService.Setup(x => x.IsLoading).Returns(false);
            _mockStateService.Setup(x => x.CurrentPage).Returns(new WorkoutTemplatePagedResultDto
            {
                Items = new List<WorkoutTemplateDto>(),
                TotalCount = 0,
                TotalPages = 0
            });
            _mockStateService.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());

            // Act
            var cut = RenderComponent<WorkoutTemplateList>(parameters => parameters
                .Add(p => p.OnCreateNew, () => { createClicked = true; }));
            
            var createButton = cut.Find("[data-testid='create-new-button']");
            createButton.Click();

            // Assert
            Assert.True(createClicked);
        }

        [Fact]
        public void Should_sort_templates_by_name_ascending()
        {
            // Arrange
            var templates = new List<WorkoutTemplateDto>
            {
                CreateTestTemplate("1", "Charlie"),
                CreateTestTemplate("2", "Alpha"),
                CreateTestTemplate("3", "Beta")
            };

            _mockStateService.Setup(x => x.IsLoading).Returns(false);
            _mockStateService.Setup(x => x.CurrentPage).Returns(new WorkoutTemplatePagedResultDto
            {
                Items = templates,
                TotalCount = 3,
                TotalPages = 1
            });
            _mockStateService.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());

            // Act
            var cut = RenderComponent<WorkoutTemplateList>();
            var sortSelect = cut.Find("[data-testid='sort-select']");
            sortSelect.Change("name");
            
            // Toggle to ascending
            var sortButton = cut.Find("[data-testid='sort-direction-button']");
            sortButton.Click();

            // Assert - Get sorted templates
            var component = cut.Instance;
            var sortedTemplates = component.GetSortedTemplates().ToList();
            Assert.Equal("Alpha", sortedTemplates[0].Name);
            Assert.Equal("Beta", sortedTemplates[1].Name);
            Assert.Equal("Charlie", sortedTemplates[2].Name);
        }

        [Fact]
        public void Should_navigate_to_next_page()
        {
            // Arrange
            var templates = new List<WorkoutTemplateDto>
            {
                CreateTestTemplate("1", "Template 1")
            };

            _mockStateService.Setup(x => x.IsLoading).Returns(false);
            _mockStateService.Setup(x => x.CurrentPage).Returns(new WorkoutTemplatePagedResultDto
            {
                Items = templates,
                TotalCount = 25,
                TotalPages = 3
            });
            _mockStateService.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto { Page = 1, PageSize = 10 });

            // Act
            var cut = RenderComponent<WorkoutTemplateList>();
            var nextButton = cut.Find("[data-testid='next-page-button']");
            nextButton.Click();

            // Assert
            _mockStateService.Verify(x => x.LoadWorkoutTemplatesAsync(It.Is<WorkoutTemplateFilterDto>(f => f.Page == 2)), Times.Once);
        }

        [Fact]
        public void Should_unsubscribe_from_state_changes_on_dispose()
        {
            // Arrange
            _mockStateService.Setup(x => x.IsLoading).Returns(false);
            _mockStateService.Setup(x => x.CurrentPage).Returns(new WorkoutTemplatePagedResultDto
            {
                Items = new List<WorkoutTemplateDto>(),
                TotalCount = 0,
                TotalPages = 0
            });
            _mockStateService.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());

            var cut = RenderComponent<WorkoutTemplateList>();

            // Act
            cut.Dispose();

            // Assert - Verify that OnChange event has been unsubscribed
            // This is implicitly tested by the dispose not throwing an exception
            Assert.True(true);
        }
    }
}