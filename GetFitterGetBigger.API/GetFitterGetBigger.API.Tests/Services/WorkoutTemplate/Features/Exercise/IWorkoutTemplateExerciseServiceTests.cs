using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services.WorkoutTemplate.Features.Exercise;

/// <summary>
/// Unit tests for IWorkoutTemplateExerciseService interface contract
/// </summary>
public class IWorkoutTemplateExerciseServiceTests
{
    [Fact]
    public void Interface_Should_HaveCorrectMethodSignatures()
    {
        // Arrange
        var interfaceType = typeof(IWorkoutTemplateExerciseService);
        
        // Act & Assert
        interfaceType.Should().NotBeNull();
        
        // Core methods - need to specify parameter types due to overloads
        var addExerciseMethod = interfaceType.GetMethod(nameof(IWorkoutTemplateExerciseService.AddExerciseAsync), 
            new[] { typeof(WorkoutTemplateId), typeof(AddExerciseDto) });
        addExerciseMethod.Should().NotBeNull()
            .And.Subject.ReturnType.Should().Be(typeof(Task<ServiceResult<AddExerciseResultDto>>));
            
        var removeExerciseMethod = interfaceType.GetMethod(nameof(IWorkoutTemplateExerciseService.RemoveExerciseAsync),
            new[] { typeof(WorkoutTemplateId), typeof(WorkoutTemplateExerciseId) });
        removeExerciseMethod.Should().NotBeNull()
            .And.Subject.ReturnType.Should().Be(typeof(Task<ServiceResult<RemoveExerciseResultDto>>));
            
        interfaceType.GetMethod(nameof(IWorkoutTemplateExerciseService.UpdateExerciseMetadataAsync))
            .Should().NotBeNull()
            .And.Subject.ReturnType.Should().Be(typeof(Task<ServiceResult<UpdateMetadataResultDto>>));
            
        interfaceType.GetMethod(nameof(IWorkoutTemplateExerciseService.ReorderExerciseAsync))
            .Should().NotBeNull()
            .And.Subject.ReturnType.Should().Be(typeof(Task<ServiceResult<ReorderResultDto>>));
            
        interfaceType.GetMethod(nameof(IWorkoutTemplateExerciseService.CopyRoundAsync))
            .Should().NotBeNull()
            .And.Subject.ReturnType.Should().Be(typeof(Task<ServiceResult<CopyRoundResultDto>>));
            
        interfaceType.GetMethod(nameof(IWorkoutTemplateExerciseService.GetTemplateExercisesAsync))
            .Should().NotBeNull()
            .And.Subject.ReturnType.Should().Be(typeof(Task<ServiceResult<WorkoutTemplateExercisesDto>>));
            
        interfaceType.GetMethod(nameof(IWorkoutTemplateExerciseService.GetExerciseByIdAsync))
            .Should().NotBeNull()
            .And.Subject.ReturnType.Should().Be(typeof(Task<ServiceResult<WorkoutTemplateExerciseDto>>));
    }

    [Fact]
    public void Interface_Should_HaveRequiredMethods()
    {
        // Arrange
        var interfaceType = typeof(IWorkoutTemplateExerciseService);
        
        // Act & Assert
        var getTemplateExercises = interfaceType.GetMethod(nameof(IWorkoutTemplateExerciseService.GetTemplateExercisesAsync));
        getTemplateExercises.Should().NotBeNull();
        
        var getExerciseById = interfaceType.GetMethod(nameof(IWorkoutTemplateExerciseService.GetExerciseByIdAsync));
        getExerciseById.Should().NotBeNull();
    }
}