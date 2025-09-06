using System.Collections.Generic;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Exercise.DataServices;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.ExerciseType;
using GetFitterGetBigger.API.Services.Results;
using Moq;
using Moq.AutoMock;
using ExerciseEntity = GetFitterGetBigger.API.Models.Entities.Exercise;

namespace GetFitterGetBigger.API.Tests.Services.Extensions;

/// <summary>
/// AutoMocker extension methods for setting up Exercise DataServices
/// </summary>
public static class AutoMockerExerciseDataServiceExtensions
{
    public static AutoMocker SetupExerciseQueryDataServiceGetPaged(
        this AutoMocker mocker,
        PagedResponse<ExerciseDto> returnValue)
    {
        mocker.GetMock<IExerciseQueryDataService>()
            .Setup(x => x.GetPagedAsync(It.IsAny<GetExercisesCommand>()))
            .ReturnsAsync(ServiceResult<PagedResponse<ExerciseDto>>.Success(returnValue));

        return mocker;
    }

    public static AutoMocker SetupExerciseQueryDataServiceGetById(
        this AutoMocker mocker,
        ExerciseDto returnValue)
    {
        mocker.GetMock<IExerciseQueryDataService>()
            .Setup(x => x.GetByIdAsync(It.IsAny<ExerciseId>()))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(returnValue));

        return mocker;
    }

    public static AutoMocker SetupExerciseQueryDataServiceExists(
        this AutoMocker mocker,
        ExerciseId id,
        bool exists)
    {
        mocker.GetMock<IExerciseQueryDataService>()
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists)));

        return mocker;
    }

    public static AutoMocker SetupExerciseQueryDataServiceExistsByName(
        this AutoMocker mocker,
        string name,
        bool exists,
        ExerciseId? excludeId = null)
    {
        mocker.GetMock<IExerciseQueryDataService>()
            .Setup(x => x.ExistsByNameAsync(name, excludeId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists)));

        return mocker;
    }

    public static AutoMocker SetupExerciseCommandDataServiceCreate(
        this AutoMocker mocker,
        ExerciseDto returnValue)
    {
        mocker.GetMock<IExerciseCommandDataService>()
            .Setup(x => x.CreateAsync(It.IsAny<ExerciseEntity>(), null))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(returnValue));

        return mocker;
    }

    public static AutoMocker SetupExerciseCommandDataServiceUpdate(
        this AutoMocker mocker,
        ExerciseDto returnValue)
    {
        mocker.GetMock<IExerciseCommandDataService>()
            .Setup(x => x.UpdateAsync(
                It.IsAny<ExerciseId>(),
                It.IsAny<Func<ExerciseEntity, ExerciseEntity>>(),
                null))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(returnValue));

        return mocker;
    }

    public static AutoMocker SetupExerciseCommandDataServiceSoftDelete(
        this AutoMocker mocker,
        bool success = true)
    {
        mocker.GetMock<IExerciseCommandDataService>()
            .Setup(x => x.SoftDeleteAsync(It.IsAny<ExerciseId>(), null))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(success)));

        return mocker;
    }

    public static AutoMocker SetupExerciseCommandDataServiceHardDelete(
        this AutoMocker mocker,
        bool success = true)
    {
        mocker.GetMock<IExerciseCommandDataService>()
            .Setup(x => x.HardDeleteAsync(It.IsAny<ExerciseId>(), null))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(success)));

        return mocker;
    }
    
    // ExerciseTypeService setup methods for DataService pattern
    public static AutoMocker SetupExerciseTypeService(
        this AutoMocker mocker,
        bool allExist = true,
        bool isRestType = false)
    {
        mocker.GetMock<IExerciseTypeService>()
            .Setup(s => s.AllExistAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(allExist);
            
        mocker.GetMock<IExerciseTypeService>()
            .Setup(s => s.AnyIsRestTypeAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(isRestType);

        return mocker;
    }
}