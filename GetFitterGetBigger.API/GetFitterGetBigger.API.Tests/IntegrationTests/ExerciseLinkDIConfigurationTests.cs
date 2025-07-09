using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

/// <summary>
/// Tests to verify that exercise link services are properly configured in DI container
/// </summary>
public class ExerciseLinkDIConfigurationTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;

    public ExerciseLinkDIConfigurationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ExerciseLinkRepository_ShouldBeRegisteredInDI()
    {
        // Arrange & Act
        await using var scope = _fixture.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetService<IExerciseLinkRepository>();

        // Assert
        Assert.NotNull(repository);
        Assert.IsAssignableFrom<IExerciseLinkRepository>(repository);
    }

    [Fact]
    public async Task ExerciseLinkService_ShouldBeRegisteredInDI()
    {
        // Arrange & Act
        await using var scope = _fixture.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetService<IExerciseLinkService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsAssignableFrom<IExerciseLinkService>(service);
    }

    [Fact]
    public async Task ExerciseLinkService_ShouldHaveAllDependenciesResolved()
    {
        // Arrange & Act
        await using var scope = _fixture.Services.CreateAsyncScope();
        
        // This will throw if any dependencies are missing
        var service = scope.ServiceProvider.GetRequiredService<IExerciseLinkService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsAssignableFrom<IExerciseLinkService>(service);
    }

    [Fact]
    public async Task ExerciseLinkRepository_ShouldHaveAllDependenciesResolved()
    {
        // Arrange & Act
        await using var scope = _fixture.Services.CreateAsyncScope();
        
        // This will throw if any dependencies are missing
        var repository = scope.ServiceProvider.GetRequiredService<IExerciseLinkRepository>();

        // Assert
        Assert.NotNull(repository);
        Assert.IsAssignableFrom<IExerciseLinkRepository>(repository);
    }

    [Fact]
    public async Task ExerciseLinkService_ShouldBeTransient()
    {
        // Arrange & Act
        await using var scope = _fixture.Services.CreateAsyncScope();
        var service1 = scope.ServiceProvider.GetService<IExerciseLinkService>();
        var service2 = scope.ServiceProvider.GetService<IExerciseLinkService>();

        // Assert - Transient services should return different instances
        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public async Task ExerciseLinkRepository_ShouldBeTransient()
    {
        // Arrange & Act
        await using var scope = _fixture.Services.CreateAsyncScope();
        var repository1 = scope.ServiceProvider.GetService<IExerciseLinkRepository>();
        var repository2 = scope.ServiceProvider.GetService<IExerciseLinkRepository>();

        // Assert - Transient services should return different instances
        Assert.NotNull(repository1);
        Assert.NotNull(repository2);
        Assert.NotSame(repository1, repository2);
    }
}