using Bunit;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Services.Stores;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.TestHelpers
{
    public abstract class WorkoutTemplateTestBase : TestContext
    {
        protected Mock<IWorkoutTemplateService> MockWorkoutTemplateService { get; }
        protected Mock<IWorkoutTemplateListStore> MockWorkoutTemplateListStore { get; }
        protected Mock<IWorkoutTemplateFormStore> MockWorkoutTemplateFormStore { get; }
        protected Mock<IWorkoutReferenceDataStore> MockWorkoutReferenceDataStore { get; }
        protected Mock<IToastService> MockToastService { get; }

        protected WorkoutTemplateTestBase()
        {
            MockWorkoutTemplateService = new Mock<IWorkoutTemplateService>();
            MockWorkoutTemplateListStore = new Mock<IWorkoutTemplateListStore>();
            // Setup default to avoid null reference exceptions
            MockWorkoutTemplateListStore.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());
            MockWorkoutTemplateFormStore = new Mock<IWorkoutTemplateFormStore>();
            MockWorkoutReferenceDataStore = new Mock<IWorkoutReferenceDataStore>();
            MockToastService = new Mock<IToastService>();

            // Register all required services
            Services.AddSingleton(MockWorkoutTemplateService.Object);
            Services.AddSingleton(MockWorkoutTemplateListStore.Object);
            Services.AddSingleton(MockWorkoutTemplateFormStore.Object);
            Services.AddSingleton(MockWorkoutReferenceDataStore.Object);
            Services.AddSingleton(MockToastService.Object);
        }
    }
}