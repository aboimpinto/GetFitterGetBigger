using Bunit;
using GetFitterGetBigger.Admin.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.TestHelpers
{
    public abstract class WorkoutTemplateTestBase : TestContext
    {
        protected Mock<IWorkoutTemplateService> MockWorkoutTemplateService { get; }
        protected Mock<IWorkoutTemplateStateService> MockWorkoutTemplateStateService { get; }
        protected Mock<IToastService> MockToastService { get; }

        protected WorkoutTemplateTestBase()
        {
            MockWorkoutTemplateService = new Mock<IWorkoutTemplateService>();
            MockWorkoutTemplateStateService = new Mock<IWorkoutTemplateStateService>();
            MockToastService = new Mock<IToastService>();

            // Register all required services
            Services.AddSingleton(MockWorkoutTemplateService.Object);
            Services.AddSingleton(MockWorkoutTemplateStateService.Object);
            Services.AddSingleton(MockToastService.Object);
        }
    }
}