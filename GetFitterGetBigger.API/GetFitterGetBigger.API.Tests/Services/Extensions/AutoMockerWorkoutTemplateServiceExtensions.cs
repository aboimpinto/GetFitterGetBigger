using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.SetConfiguration;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services.Extensions;

/// <summary>
/// AutoMocker extension methods for WorkoutTemplate controller testing.
/// Provides setup and verification methods for WorkoutTemplate service dependencies.
/// </summary>
public static class AutoMockerWorkoutTemplateServiceExtensions
{
    #region Setup Methods - Success Scenarios
    
    public static AutoMocker SetupWorkoutTemplateService_UpdateAsync_Success(
        this AutoMocker mocker, 
        WorkoutTemplateDto returnDto)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.UpdateAsync(
                It.IsAny<WorkoutTemplateId>(), 
                It.IsAny<UpdateWorkoutTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(returnDto));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_CreateAsync_Success(
        this AutoMocker mocker, 
        WorkoutTemplateDto returnDto)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.CreateAsync(It.IsAny<CreateWorkoutTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(returnDto));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_GetByIdAsync_Success(
        this AutoMocker mocker, 
        WorkoutTemplateDto returnDto)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.GetByIdAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(returnDto));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_DeleteAsync_Success(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.DeleteAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_ChangeStateAsync_Success(
        this AutoMocker mocker, 
        WorkoutTemplateDto returnDto)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.ChangeStateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<WorkoutStateId>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(returnDto));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_DuplicateAsync_Success(
        this AutoMocker mocker, 
        WorkoutTemplateDto returnDto)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.DuplicateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(returnDto));
        
        return mocker;
    }
    
    #endregion
    
    #region Setup Methods - Failure Scenarios
    
    public static AutoMocker SetupWorkoutTemplateService_UpdateAsync_NotFound(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.UpdateAsync(
                It.IsAny<WorkoutTemplateId>(), 
                It.IsAny<UpdateWorkoutTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.NotFound("WorkoutTemplate")));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_UpdateAsync_AlreadyExists(
        this AutoMocker mocker, 
        string duplicateName)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.UpdateAsync(
                It.IsAny<WorkoutTemplateId>(), 
                It.IsAny<UpdateWorkoutTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.AlreadyExists("WorkoutTemplate", duplicateName)));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_UpdateAsync_ValidationFailed(
        this AutoMocker mocker, 
        string errorMessage)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.UpdateAsync(
                It.IsAny<WorkoutTemplateId>(), 
                It.IsAny<UpdateWorkoutTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed(errorMessage)));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_CreateAsync_AlreadyExists(
        this AutoMocker mocker, 
        string duplicateName)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.CreateAsync(It.IsAny<CreateWorkoutTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.AlreadyExists("WorkoutTemplate", duplicateName)));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_CreateAsync_ValidationFailed(
        this AutoMocker mocker, 
        string errorMessage)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.CreateAsync(It.IsAny<CreateWorkoutTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed(errorMessage)));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_GetByIdAsync_NotFound(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.GetByIdAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.NotFound("WorkoutTemplate")));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_DeleteAsync_NotFound(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.DeleteAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Create(false),
                ServiceError.NotFound("WorkoutTemplate")));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_ChangeStateAsync_NotFound(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.ChangeStateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<WorkoutStateId>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.NotFound("WorkoutTemplate")));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_ChangeStateAsync_DependencyExists(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.ChangeStateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<WorkoutStateId>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.DependencyExists("WorkoutTemplate", "dependencies")));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_ChangeStateAsync_ValidationFailed(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.ChangeStateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<WorkoutStateId>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed("Invalid state transition")));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_ChangeStateAsync_InvalidFormat(
        this AutoMocker mocker, 
        string errorMessage)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.ChangeStateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<WorkoutStateId>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.InvalidFormat("WorkoutStateId", "workoutstate-{guid}")));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_ChangeStateAsync_Unauthorized(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.ChangeStateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<WorkoutStateId>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.Unauthorized("User is not authorized")));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_ChangeStateAsync_InternalError(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.ChangeStateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<WorkoutStateId>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.InternalError("An internal error occurred")));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_DuplicateAsync_NotFound(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.DuplicateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.NotFound("WorkoutTemplate")));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_DuplicateAsync_AlreadyExists(
        this AutoMocker mocker, 
        string duplicateName)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.DuplicateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.AlreadyExists("WorkoutTemplate", duplicateName)));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateService_DuplicateAsync_ValidationFailed(
        this AutoMocker mocker, 
        string errorMessage)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Setup(x => x.DuplicateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed(errorMessage)));
        
        return mocker;
    }
    
    #endregion
    
    #region Verification Methods - Positive Cases
    
    public static AutoMocker VerifyWorkoutTemplateService_UpdateAsync_CalledOnceWith(
        this AutoMocker mocker,
        string templateId,
        UpdateWorkoutTemplateDto dto)
    {
        // Parse the IDs the same way the controller does to get the actual values that will be passed
        var expectedTemplateId = WorkoutTemplateId.ParseOrEmpty(templateId);
        var expectedCategoryId = WorkoutCategoryId.ParseOrEmpty(dto.CategoryId);
        var expectedDifficultyId = DifficultyLevelId.ParseOrEmpty(dto.DifficultyId);
        var expectedObjectiveIds = dto.ObjectiveIds.Select(WorkoutObjectiveId.ParseOrEmpty).ToList();
        
        mocker.GetMock<IWorkoutTemplateService>()
            .Verify(x => x.UpdateAsync(
                It.Is<WorkoutTemplateId>(id => id.ToString() == expectedTemplateId.ToString()),
                It.Is<UpdateWorkoutTemplateCommand>(cmd => 
                    cmd.Name == dto.Name && 
                    cmd.Description == dto.Description &&
                    cmd.CategoryId != null && cmd.CategoryId.ToString() == expectedCategoryId.ToString() &&
                    cmd.DifficultyId != null && cmd.DifficultyId.ToString() == expectedDifficultyId.ToString() &&
                    cmd.EstimatedDurationMinutes == dto.EstimatedDurationMinutes &&
                    cmd.IsPublic == dto.IsPublic &&
                    (cmd.Tags == null ? dto.Tags.Count == 0 : cmd.Tags.SequenceEqual(dto.Tags)) &&
                    (cmd.ObjectiveIds == null ? expectedObjectiveIds.Count == 0 : 
                        cmd.ObjectiveIds.Select(oid => oid.ToString()).SequenceEqual(expectedObjectiveIds.Select(oid => oid.ToString()))))), Times.Once);
        
        return mocker;
    }
    
    public static AutoMocker VerifyWorkoutTemplateService_CreateAsync_CalledOnceWith(
        this AutoMocker mocker,
        CreateWorkoutTemplateDto dto)
    {
        // Parse the IDs the same way the controller does to get the actual values that will be passed
        var expectedCategoryId = WorkoutCategoryId.ParseOrEmpty(dto.CategoryId);
        var expectedDifficultyId = DifficultyLevelId.ParseOrEmpty(dto.DifficultyId);
        var expectedObjectiveIds = dto.ObjectiveIds.Select(WorkoutObjectiveId.ParseOrEmpty).ToList();
        
        mocker.GetMock<IWorkoutTemplateService>()
            .Verify(x => x.CreateAsync(
                It.Is<CreateWorkoutTemplateCommand>(cmd => 
                    cmd.Name == dto.Name && 
                    cmd.Description == dto.Description &&
                    cmd.CategoryId.ToString() == expectedCategoryId.ToString() &&
                    cmd.DifficultyId.ToString() == expectedDifficultyId.ToString() &&
                    cmd.EstimatedDurationMinutes == dto.EstimatedDurationMinutes &&
                    cmd.IsPublic == dto.IsPublic &&
                    cmd.Tags.SequenceEqual(dto.Tags) &&
                    cmd.ObjectiveIds.Select(oid => oid.ToString()).SequenceEqual(expectedObjectiveIds.Select(oid => oid.ToString())))), Times.Once);
        
        return mocker;
    }
    
    public static AutoMocker VerifyWorkoutTemplateService_GetByIdAsync_CalledOnceWith(
        this AutoMocker mocker,
        string templateId)
    {
        // Parse the ID the same way the controller does to get the actual value that will be passed
        var expectedTemplateId = WorkoutTemplateId.ParseOrEmpty(templateId);
        
        mocker.GetMock<IWorkoutTemplateService>()
            .Verify(x => x.GetByIdAsync(
                It.Is<WorkoutTemplateId>(id => id.ToString() == expectedTemplateId.ToString())), Times.Once);
        
        return mocker;
    }
    
    public static AutoMocker VerifyWorkoutTemplateService_DeleteAsync_CalledOnceWith(
        this AutoMocker mocker,
        string templateId)
    {
        // Parse the ID the same way the controller does to get the actual value that will be passed
        var expectedTemplateId = WorkoutTemplateId.ParseOrEmpty(templateId);
        
        mocker.GetMock<IWorkoutTemplateService>()
            .Verify(x => x.DeleteAsync(
                It.Is<WorkoutTemplateId>(id => id.ToString() == expectedTemplateId.ToString())), Times.Once);
        
        return mocker;
    }
    
    public static AutoMocker VerifyWorkoutTemplateService_ChangeStateAsync_CalledOnceWith(
        this AutoMocker mocker,
        string templateId,
        string workoutStateId)
    {
        // Parse the IDs the same way the controller does to get the actual values that will be passed
        var expectedTemplateId = WorkoutTemplateId.ParseOrEmpty(templateId);
        var expectedStateId = WorkoutStateId.ParseOrEmpty(workoutStateId);
        
        mocker.GetMock<IWorkoutTemplateService>()
            .Verify(x => x.ChangeStateAsync(
                It.Is<WorkoutTemplateId>(id => id.ToString() == expectedTemplateId.ToString()),
                It.Is<WorkoutStateId>(id => id.ToString() == expectedStateId.ToString())), Times.Once);
        
        return mocker;
    }
    
    public static AutoMocker VerifyWorkoutTemplateService_DuplicateAsync_CalledOnceWith(
        this AutoMocker mocker,
        string templateId,
        string newName)
    {
        // Parse the ID the same way the controller does to get the actual value that will be passed
        var expectedTemplateId = WorkoutTemplateId.ParseOrEmpty(templateId);
        
        mocker.GetMock<IWorkoutTemplateService>()
            .Verify(x => x.DuplicateAsync(
                It.Is<WorkoutTemplateId>(id => id.ToString() == expectedTemplateId.ToString()),
                It.Is<string>(name => name == newName)), Times.Once);
        
        return mocker;
    }
    
    // Legacy verification methods - kept for backward compatibility but marked as obsolete
    [Obsolete("Use VerifyWorkoutTemplateService_UpdateAsync_CalledOnceWith for exact parameter verification")]
    public static AutoMocker VerifyWorkoutTemplateService_UpdateCalledOnce(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Verify(x => x.UpdateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<UpdateWorkoutTemplateCommand>()), Times.Once);
        
        return mocker;
    }
    
    [Obsolete("Use VerifyWorkoutTemplateService_CreateAsync_CalledOnceWith for exact parameter verification")]
    public static AutoMocker VerifyWorkoutTemplateService_CreateCalledOnce(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Verify(x => x.CreateAsync(
                It.IsAny<CreateWorkoutTemplateCommand>()), Times.Once);
        
        return mocker;
    }
    
    [Obsolete("Use VerifyWorkoutTemplateService_GetByIdAsync_CalledOnceWith for exact parameter verification")]
    public static AutoMocker VerifyWorkoutTemplateService_GetByIdCalledOnce(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Verify(x => x.GetByIdAsync(
                It.IsAny<WorkoutTemplateId>()), Times.Once);
        
        return mocker;
    }
    
    [Obsolete("Use VerifyWorkoutTemplateService_DeleteAsync_CalledOnceWith for exact parameter verification")]
    public static AutoMocker VerifyWorkoutTemplateService_DeleteCalledOnce(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Verify(x => x.DeleteAsync(
                It.IsAny<WorkoutTemplateId>()), Times.Once);
        
        return mocker;
    }
    
    [Obsolete("Use VerifyWorkoutTemplateService_ChangeStateAsync_CalledOnceWith for exact parameter verification")]
    public static AutoMocker VerifyWorkoutTemplateService_ChangeStateCalledOnce(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .Verify(x => x.ChangeStateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<WorkoutStateId>()), Times.Once);
        
        return mocker;
    }
    
    #endregion
    
    #region Verification Methods - Negative Cases (Never Called)
    
    public static AutoMocker VerifyWorkoutTemplateService_NeverCalled(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateService>()
            .VerifyNoOtherCalls();
        
        return mocker;
    }
    
    public static AutoMocker VerifyWorkoutTemplateExerciseService_NeverCalled(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateExerciseService>()
            .VerifyNoOtherCalls();
        
        return mocker;
    }
    
    public static AutoMocker VerifySetConfigurationService_NeverCalled(
        this AutoMocker mocker)
    {
        mocker.GetMock<ISetConfigurationService>()
            .VerifyNoOtherCalls();
        
        return mocker;
    }
    
    #endregion
}