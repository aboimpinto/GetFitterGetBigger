using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Equipment;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Handlers;
using Moq;
using Moq.AutoMock;
using WorkoutTemplateEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplate;

namespace GetFitterGetBigger.API.Tests.Services.Extensions;

public static class AutoMockerWorkoutTemplateDataServiceExtensions
{
    #region Query DataService Setup Methods
    
    public static AutoMocker SetupWorkoutTemplateQueryDataService_GetByIdWithDetails(
        this AutoMocker mocker, 
        WorkoutTemplateDto dto)
    {
        mocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(dto));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateQueryDataService_GetByIdWithDetails_NotFound(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty, 
                ServiceError.NotFound("WorkoutTemplate")));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateQueryDataService_Exists(
        this AutoMocker mocker,
        WorkoutTemplateId id,
        bool exists)
    {
        mocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(
                new BooleanResultDto { Value = exists }));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateQueryDataService_ExistsByName(
        this AutoMocker mocker,
        string name,
        bool exists)
    {
        mocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.ExistsByNameAsync(name))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(
                new BooleanResultDto { Value = exists }));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateQueryDataService_HasExecutionLogs(
        this AutoMocker mocker,
        WorkoutTemplateId id,
        bool hasLogs)
    {
        mocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.HasExecutionLogsAsync(id))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(
                new BooleanResultDto { Value = hasLogs }));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateQueryDataService_Search(
        this AutoMocker mocker,
        PagedResponse<WorkoutTemplateDto> response)
    {
        mocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.SearchAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<WorkoutCategoryId>(),
                It.IsAny<WorkoutObjectiveId>(),
                It.IsAny<DifficultyLevelId>(),
                It.IsAny<WorkoutStateId>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<PagedResponse<WorkoutTemplateDto>>.Success(response));
        
        return mocker;
    }
    
    #endregion
    
    #region Command DataService Setup Methods
    
    public static AutoMocker SetupWorkoutTemplateCommandDataService_Create(
        this AutoMocker mocker,
        WorkoutTemplateDto dto)
    {
        mocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Setup(x => x.CreateAsync(It.IsAny<WorkoutTemplateEntity>(), null))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(dto));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateCommandDataService_Update(
        this AutoMocker mocker,
        WorkoutTemplateDto dto)
    {
        mocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Setup(x => x.UpdateAsync(
                It.IsAny<WorkoutTemplateId>(), 
                It.IsAny<Func<WorkoutTemplateEntity, WorkoutTemplateEntity>>(),
                null))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(dto));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateCommandDataService_ChangeState(
        this AutoMocker mocker,
        WorkoutTemplateDto dto)
    {
        mocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Setup(x => x.ChangeStateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<WorkoutStateId>(),
                null))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(dto));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateCommandDataService_Duplicate(
        this AutoMocker mocker,
        WorkoutTemplateDto dto)
    {
        mocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Setup(x => x.DuplicateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<string>(),
                It.IsAny<UserId>(),
                null))
            .ReturnsAsync(ServiceResult<WorkoutTemplateDto>.Success(dto));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateCommandDataService_SoftDelete(
        this AutoMocker mocker,
        bool success = true)
    {
        mocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Setup(x => x.SoftDeleteAsync(It.IsAny<WorkoutTemplateId>(), null))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(
                new BooleanResultDto { Value = success }));
        
        return mocker;
    }
    
    public static AutoMocker SetupWorkoutTemplateCommandDataService_Delete(
        this AutoMocker mocker,
        bool success = true)
    {
        mocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Setup(x => x.DeleteAsync(It.IsAny<WorkoutTemplateId>(), null))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(
                new BooleanResultDto { Value = success }));
        
        return mocker;
    }
    
    #endregion
    
    #region Additional Service Setup Methods
    
    public static AutoMocker SetupSuggestionHandler_GetSuggestedExercises(
        this AutoMocker mocker,
        IEnumerable<ExerciseDto> exercises)
    {
        mocker.GetMock<SuggestionHandler>()
            .Setup(x => x.GetSuggestedExercisesAsync(
                It.IsAny<WorkoutCategoryId>(),
                It.IsAny<IEnumerable<ExerciseId>>(),
                It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<IEnumerable<ExerciseDto>>.Success(exercises));
        
        return mocker;
    }
    
    public static AutoMocker SetupEquipmentRequirementsService_GetRequiredEquipment(
        this AutoMocker mocker,
        IEnumerable<EquipmentDto> equipment)
    {
        mocker.GetMock<IEquipmentRequirementsService>()
            .Setup(x => x.GetRequiredEquipmentAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(ServiceResult<IEnumerable<EquipmentDto>>.Success(equipment));
        
        return mocker;
    }
    
    #endregion
    
    #region Verification Methods
    
    public static void VerifyWorkoutTemplateQueryDataService_GetByIdWithDetailsOnce(
        this AutoMocker mocker,
        WorkoutTemplateId id)
    {
        mocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Verify(x => x.GetByIdWithDetailsAsync(id), Times.Once());
    }
    
    public static void VerifyWorkoutTemplateQueryDataService_ExistsOnce(
        this AutoMocker mocker,
        WorkoutTemplateId id)
    {
        mocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Verify(x => x.ExistsAsync(id), Times.Once());
    }
    
    public static void VerifyWorkoutTemplateQueryDataService_ExistsByNameOnce(
        this AutoMocker mocker,
        string name)
    {
        mocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Verify(x => x.ExistsByNameAsync(name), Times.Once());
    }
    
    public static void VerifyWorkoutTemplateCommandDataService_CreateOnce(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Verify(x => x.CreateAsync(It.IsAny<WorkoutTemplateEntity>(), null), Times.Once());
    }
    
    public static void VerifyWorkoutTemplateCommandDataService_UpdateOnce(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Verify(x => x.UpdateAsync(
                It.IsAny<WorkoutTemplateId>(),
                It.IsAny<Func<WorkoutTemplateEntity, WorkoutTemplateEntity>>(),
                null), Times.Once());
    }
    
    public static void VerifyWorkoutTemplateCommandDataService_ChangeStateOnce(
        this AutoMocker mocker,
        WorkoutTemplateId id,
        WorkoutStateId stateId)
    {
        mocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Verify(x => x.ChangeStateAsync(id, stateId, null), Times.Once());
    }
    
    public static void VerifyWorkoutTemplateCommandDataService_SoftDeleteOnce(
        this AutoMocker mocker,
        WorkoutTemplateId id)
    {
        mocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Verify(x => x.SoftDeleteAsync(id, null), Times.Once());
    }
    
    public static void VerifyWorkoutTemplateCommandDataService_DeleteOnce(
        this AutoMocker mocker,
        WorkoutTemplateId id)
    {
        mocker.GetMock<IWorkoutTemplateCommandDataService>()
            .Verify(x => x.DeleteAsync(id, null), Times.Once());
    }
    
    #endregion
    
    #region Negative Verification Methods (Never Called)
    
    public static void VerifyWorkoutTemplateQueryDataService_NeverCalled(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateQueryDataService>()
            .VerifyNoOtherCalls();
    }
    
    public static void VerifyWorkoutTemplateCommandDataService_NeverCalled(
        this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateCommandDataService>()
            .VerifyNoOtherCalls();
    }
    
    #endregion
}