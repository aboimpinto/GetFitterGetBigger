using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.DifficultyLevel.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.ExerciseType.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.KineticChainType.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.MuscleRole.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.MetricType.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.MovementPattern.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.ExerciseWeightType.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.MuscleGroup.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.Equipment.DataServices;
using GetFitterGetBigger.API.Services.Results;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services.Extensions;

/// <summary>
/// AutoMocker extension methods for reference table services testing
/// Provides generic setup and verification methods for all reference table DataServices
/// </summary>
public static class AutoMockerReferenceTableExtensions
{
    // DifficultyLevel DataService Extensions
    public static AutoMocker SetupDifficultyLevelDataServiceGetAllActive(this AutoMocker mocker, IEnumerable<ReferenceDataDto> items)
    {
        mocker.GetMock<IDifficultyLevelDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ReferenceDataDto>>.Success(items));
        return mocker;
    }

    public static AutoMocker SetupDifficultyLevelDataServiceGetById(this AutoMocker mocker, DifficultyLevelId id, ReferenceDataDto item)
    {
        mocker.GetMock<IDifficultyLevelDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(item));
        return mocker;
    }

    public static AutoMocker SetupDifficultyLevelDataServiceGetByIdNotFound(this AutoMocker mocker, DifficultyLevelId id)
    {
        mocker.GetMock<IDifficultyLevelDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty));
        return mocker;
    }

    public static AutoMocker SetupDifficultyLevelDataServiceGetByValue(this AutoMocker mocker, string value, ReferenceDataDto item)
    {
        mocker.GetMock<IDifficultyLevelDataService>()
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(item));
        return mocker;
    }

    public static AutoMocker SetupDifficultyLevelDataServiceGetByValueNotFound(this AutoMocker mocker, string value)
    {
        mocker.GetMock<IDifficultyLevelDataService>()
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty));
        return mocker;
    }

    // Verification methods for DifficultyLevel
    public static AutoMocker VerifyDifficultyLevelDataServiceGetAllActiveOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IDifficultyLevelDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Once);
        return mocker;
    }

    public static AutoMocker VerifyDifficultyLevelDataServiceGetByIdOnce(this AutoMocker mocker, DifficultyLevelId id)
    {
        mocker.GetMock<IDifficultyLevelDataService>()
            .Verify(x => x.GetByIdAsync(id), Times.Once);
        return mocker;
    }

    public static AutoMocker VerifyDifficultyLevelDataServiceGetByValueOnce(this AutoMocker mocker, string value)
    {
        mocker.GetMock<IDifficultyLevelDataService>()
            .Verify(x => x.GetByValueAsync(value), Times.Once);
        return mocker;
    }

    public static AutoMocker VerifyDifficultyLevelDataServiceGetByIdNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IDifficultyLevelDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<DifficultyLevelId>()), Times.Never);
        return mocker;
    }

    public static AutoMocker VerifyDifficultyLevelDataServiceGetByValueNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IDifficultyLevelDataService>()
            .Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
        return mocker;
    }

    public static AutoMocker VerifyDifficultyLevelDataServiceGetAllActiveNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IDifficultyLevelDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Never);
        return mocker;
    }

    // ExerciseType DataService Extensions
    public static AutoMocker SetupExerciseTypeDataServiceGetAllActive(this AutoMocker mocker, IEnumerable<ExerciseTypeDto> items)
    {
        mocker.GetMock<IExerciseTypeDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ExerciseTypeDto>>.Success(items));
        return mocker;
    }

    public static AutoMocker SetupExerciseTypeDataServiceGetById(this AutoMocker mocker, ExerciseTypeId id, ExerciseTypeDto item)
    {
        mocker.GetMock<IExerciseTypeDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ExerciseTypeDto>.Success(item));
        return mocker;
    }

    public static AutoMocker SetupExerciseTypeDataServiceGetByIdNotFound(this AutoMocker mocker, ExerciseTypeId id)
    {
        mocker.GetMock<IExerciseTypeDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ExerciseTypeDto>.Success(ExerciseTypeDto.Empty));
        return mocker;
    }

    public static AutoMocker SetupExerciseTypeDataServiceGetByValue(this AutoMocker mocker, string value, ExerciseTypeDto item)
    {
        mocker.GetMock<IExerciseTypeDataService>()
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(ServiceResult<ExerciseTypeDto>.Success(item));
        return mocker;
    }

    public static AutoMocker SetupExerciseTypeDataServiceGetByValueNotFound(this AutoMocker mocker, string value)
    {
        mocker.GetMock<IExerciseTypeDataService>()
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(ServiceResult<ExerciseTypeDto>.Success(ExerciseTypeDto.Empty));
        return mocker;
    }

    public static AutoMocker VerifyExerciseTypeDataServiceGetByIdNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IExerciseTypeDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<ExerciseTypeId>()), Times.Never);
        return mocker;
    }

    // KineticChainType DataService Extensions  
    public static AutoMocker SetupKineticChainTypeDataServiceGetAllActive(this AutoMocker mocker, IEnumerable<ReferenceDataDto> items)
    {
        mocker.GetMock<IKineticChainTypeDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ReferenceDataDto>>.Success(items));
        return mocker;
    }

    public static AutoMocker SetupKineticChainTypeDataServiceGetById(this AutoMocker mocker, KineticChainTypeId id, ReferenceDataDto item)
    {
        mocker.GetMock<IKineticChainTypeDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(item));
        return mocker;
    }

    public static AutoMocker SetupKineticChainTypeDataServiceGetByIdNotFound(this AutoMocker mocker, KineticChainTypeId id)
    {
        mocker.GetMock<IKineticChainTypeDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty));
        return mocker;
    }

    public static AutoMocker VerifyKineticChainTypeDataServiceGetByIdNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IKineticChainTypeDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<KineticChainTypeId>()), Times.Never);
        return mocker;
    }

    // MuscleRole DataService Extensions
    public static AutoMocker SetupMuscleRoleDataServiceGetAllActive(this AutoMocker mocker, IEnumerable<ReferenceDataDto> items)
    {
        mocker.GetMock<IMuscleRoleDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ReferenceDataDto>>.Success(items));
        return mocker;
    }

    public static AutoMocker SetupMuscleRoleDataServiceGetById(this AutoMocker mocker, MuscleRoleId id, ReferenceDataDto item)
    {
        mocker.GetMock<IMuscleRoleDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(item));
        return mocker;
    }

    public static AutoMocker SetupMuscleRoleDataServiceGetByIdNotFound(this AutoMocker mocker, MuscleRoleId id)
    {
        mocker.GetMock<IMuscleRoleDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty));
        return mocker;
    }

    public static AutoMocker VerifyMuscleRoleDataServiceGetByIdNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IMuscleRoleDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<MuscleRoleId>()), Times.Never);
        return mocker;
    }

    // MetricType DataService Extensions
    public static AutoMocker SetupMetricTypeDataServiceGetAllActive(this AutoMocker mocker, IEnumerable<ReferenceDataDto> items)
    {
        mocker.GetMock<IMetricTypeDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ReferenceDataDto>>.Success(items));
        return mocker;
    }

    public static AutoMocker SetupMetricTypeDataServiceGetById(this AutoMocker mocker, MetricTypeId id, ReferenceDataDto item)
    {
        mocker.GetMock<IMetricTypeDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(item));
        return mocker;
    }

    public static AutoMocker SetupMetricTypeDataServiceGetByIdNotFound(this AutoMocker mocker, MetricTypeId id)
    {
        mocker.GetMock<IMetricTypeDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty));
        return mocker;
    }

    public static AutoMocker VerifyMetricTypeDataServiceGetByIdNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IMetricTypeDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<MetricTypeId>()), Times.Never);
        return mocker;
    }

    public static AutoMocker SetupMetricTypeDataServiceExistsAsync(this AutoMocker mocker, MetricTypeId id, BooleanResultDto result)
    {
        mocker.GetMock<IMetricTypeDataService>()
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(result));
        return mocker;
    }

    public static AutoMocker VerifyMetricTypeDataServiceExistsAsyncOnce(this AutoMocker mocker, MetricTypeId id)
    {
        mocker.GetMock<IMetricTypeDataService>()
            .Verify(x => x.ExistsAsync(id), Times.Once);
        return mocker;
    }

    public static AutoMocker VerifyMetricTypeDataServiceExistsAsyncNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IMetricTypeDataService>()
            .Verify(x => x.ExistsAsync(It.IsAny<MetricTypeId>()), Times.Never);
        return mocker;
    }

    // MovementPattern DataService Extensions
    public static AutoMocker SetupMovementPatternDataServiceGetAllActive(this AutoMocker mocker, IEnumerable<ReferenceDataDto> items)
    {
        mocker.GetMock<IMovementPatternDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ReferenceDataDto>>.Success(items));
        return mocker;
    }

    public static AutoMocker SetupMovementPatternDataServiceGetById(this AutoMocker mocker, MovementPatternId id, ReferenceDataDto item)
    {
        mocker.GetMock<IMovementPatternDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(item));
        return mocker;
    }

    public static AutoMocker SetupMovementPatternDataServiceGetByIdNotFound(this AutoMocker mocker, MovementPatternId id)
    {
        mocker.GetMock<IMovementPatternDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty));
        return mocker;
    }

    public static AutoMocker VerifyMovementPatternDataServiceGetByIdNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IMovementPatternDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<MovementPatternId>()), Times.Never);
        return mocker;
    }

    public static AutoMocker SetupMovementPatternDataServiceExistsAsync(this AutoMocker mocker, MovementPatternId id, BooleanResultDto result)
    {
        mocker.GetMock<IMovementPatternDataService>()
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(result));
        return mocker;
    }

    public static AutoMocker VerifyMovementPatternDataServiceExistsAsyncOnce(this AutoMocker mocker, MovementPatternId id)
    {
        mocker.GetMock<IMovementPatternDataService>()
            .Verify(x => x.ExistsAsync(id), Times.Once);
        return mocker;
    }

    public static AutoMocker VerifyMovementPatternDataServiceExistsAsyncNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IMovementPatternDataService>()
            .Verify(x => x.ExistsAsync(It.IsAny<MovementPatternId>()), Times.Never);
        return mocker;
    }

    // ExerciseWeightType DataService Extensions
    public static AutoMocker SetupExerciseWeightTypeDataServiceGetAllActive(this AutoMocker mocker, IEnumerable<ReferenceDataDto> items)
    {
        mocker.GetMock<IExerciseWeightTypeDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ReferenceDataDto>>.Success(items));
        return mocker;
    }

    public static AutoMocker SetupExerciseWeightTypeDataServiceGetById(this AutoMocker mocker, ExerciseWeightTypeId id, ReferenceDataDto item)
    {
        mocker.GetMock<IExerciseWeightTypeDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(item));
        return mocker;
    }

    public static AutoMocker SetupExerciseWeightTypeDataServiceGetByIdNotFound(this AutoMocker mocker, ExerciseWeightTypeId id)
    {
        mocker.GetMock<IExerciseWeightTypeDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty));
        return mocker;
    }

    public static AutoMocker VerifyExerciseWeightTypeDataServiceGetByIdNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IExerciseWeightTypeDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<ExerciseWeightTypeId>()), Times.Never);
        return mocker;
    }

    public static AutoMocker SetupExerciseWeightTypeDataServiceExistsAsync(this AutoMocker mocker, ExerciseWeightTypeId id, BooleanResultDto result)
    {
        mocker.GetMock<IExerciseWeightTypeDataService>()
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(result));
        return mocker;
    }

    public static AutoMocker VerifyExerciseWeightTypeDataServiceExistsAsyncOnce(this AutoMocker mocker, ExerciseWeightTypeId id)
    {
        mocker.GetMock<IExerciseWeightTypeDataService>()
            .Verify(x => x.ExistsAsync(id), Times.Once);
        return mocker;
    }

    public static AutoMocker VerifyExerciseWeightTypeDataServiceExistsAsyncNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IExerciseWeightTypeDataService>()
            .Verify(x => x.ExistsAsync(It.IsAny<ExerciseWeightTypeId>()), Times.Never);
        return mocker;
    }

    // Equipment DataService Extensions
    public static AutoMocker SetupEquipmentDataServiceGetAllActive(this AutoMocker mocker, IEnumerable<EquipmentDto> items)
    {
        mocker.GetMock<IEquipmentDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<EquipmentDto>>.Success(items));
        return mocker;
    }

    public static AutoMocker SetupEquipmentDataServiceGetById(this AutoMocker mocker, EquipmentId id, EquipmentDto item)
    {
        mocker.GetMock<IEquipmentDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Success(item));
        return mocker;
    }

    public static AutoMocker SetupEquipmentDataServiceGetByIdNotFound(this AutoMocker mocker, EquipmentId id)
    {
        mocker.GetMock<IEquipmentDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Success(EquipmentDto.Empty));
        return mocker;
    }

    public static AutoMocker VerifyEquipmentDataServiceGetByIdNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IEquipmentDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<EquipmentId>()), Times.Never);
        return mocker;
    }

    // MuscleGroup DataService Extensions  
    public static AutoMocker SetupMuscleGroupDataServiceGetAllActive(this AutoMocker mocker, IEnumerable<MuscleGroupDto> items)
    {
        mocker.GetMock<IMuscleGroupDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<MuscleGroupDto>>.Success(items));
        return mocker;
    }

    public static AutoMocker SetupMuscleGroupDataServiceGetById(this AutoMocker mocker, MuscleGroupId id, MuscleGroupDto item)
    {
        mocker.GetMock<IMuscleGroupDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Success(item));
        return mocker;
    }

    public static AutoMocker SetupMuscleGroupDataServiceGetByIdNotFound(this AutoMocker mocker, MuscleGroupId id)
    {
        mocker.GetMock<IMuscleGroupDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Success(MuscleGroupDto.Empty));
        return mocker;
    }

    public static AutoMocker VerifyMuscleGroupDataServiceGetByIdNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IMuscleGroupDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<MuscleGroupId>()), Times.Never);
        return mocker;
    }

    // Generic Cache Extensions for Reference Data
    public static AutoMocker SetupReferenceDataCacheMiss(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());
        return mocker;
    }

    public static AutoMocker SetupReferenceDataCacheMissList(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<ReferenceDataDto>>.Miss());
        return mocker;
    }

    public static AutoMocker SetupReferenceDataCacheHit(this AutoMocker mocker, ReferenceDataDto item)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Hit(item));
        return mocker;
    }

    public static AutoMocker SetupReferenceDataCacheHitList(this AutoMocker mocker, IEnumerable<ReferenceDataDto> items)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<ReferenceDataDto>>.Hit(items));
        return mocker;
    }

    public static AutoMocker VerifyReferenceDataCacheGetOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()), Times.Once);
        return mocker;
    }

    public static AutoMocker VerifyReferenceDataCacheGetListOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()), Times.Once);
        return mocker;
    }

    public static AutoMocker VerifyReferenceDataCacheSetOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<ReferenceDataDto>()), Times.Once);
        return mocker;
    }

    public static AutoMocker VerifyReferenceDataCacheSetListOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ReferenceDataDto>>()), Times.Once);
        return mocker;
    }

    // ExerciseType Cache Extensions
    public static AutoMocker SetupExerciseTypeCacheMiss(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<ExerciseTypeDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<ExerciseTypeDto>>.Miss());

        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<ExerciseTypeDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ExerciseTypeDto>.Miss());

        return mocker;
    }

    public static AutoMocker SetupExerciseTypeCacheHit(this AutoMocker mocker, IEnumerable<ExerciseTypeDto> items)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<ExerciseTypeDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<ExerciseTypeDto>>.Hit(items));
        return mocker;
    }

    public static AutoMocker SetupExerciseTypeCacheHit(this AutoMocker mocker, ExerciseTypeDto item)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<ExerciseTypeDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ExerciseTypeDto>.Hit(item));
        return mocker;
    }

    public static AutoMocker VerifyExerciseTypeCacheGetOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.GetAsync<ExerciseTypeDto>(It.IsAny<string>()), Times.AtLeastOnce);
        return mocker;
    }

    public static AutoMocker VerifyExerciseTypeCacheSetOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<ExerciseTypeDto>()), Times.AtLeastOnce);
        return mocker;
    }

    // Equipment Cache Extensions
    public static AutoMocker SetupEquipmentCacheMiss(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<EquipmentDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<EquipmentDto>>.Miss());

        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<EquipmentDto>.Miss());

        return mocker;
    }

    public static AutoMocker SetupEquipmentCacheHit(this AutoMocker mocker, IEnumerable<EquipmentDto> items)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<EquipmentDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<EquipmentDto>>.Hit(items));
        return mocker;
    }

    public static AutoMocker SetupEquipmentCacheHit(this AutoMocker mocker, EquipmentDto item)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<EquipmentDto>.Hit(item));
        return mocker;
    }

    public static AutoMocker VerifyEquipmentCacheGetOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()), Times.AtLeastOnce);
        return mocker;
    }

    public static AutoMocker VerifyEquipmentCacheGetNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()), Times.Never);
        return mocker;
    }

    public static AutoMocker VerifyEquipmentCacheSetOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<EquipmentDto>()), Times.AtLeastOnce);
        return mocker;
    }

    // Equipment DataService Verification Extensions
    public static AutoMocker VerifyEquipmentDataServiceGetAllActiveOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEquipmentDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Once);
        return mocker;
    }

    public static AutoMocker VerifyEquipmentDataServiceGetAllActiveNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IEquipmentDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Never);
        return mocker;
    }

    public static AutoMocker VerifyEquipmentDataServiceGetByIdOnce(this AutoMocker mocker, EquipmentId id)
    {
        mocker.GetMock<IEquipmentDataService>()
            .Verify(x => x.GetByIdAsync(id), Times.Once);
        return mocker;
    }

    public static AutoMocker SetupEquipmentDataServiceExistsAsync(this AutoMocker mocker, EquipmentId id, BooleanResultDto result)
    {
        mocker.GetMock<IEquipmentDataService>()
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(result));
        return mocker;
    }

    public static AutoMocker VerifyEquipmentDataServiceExistsAsyncOnce(this AutoMocker mocker, EquipmentId id)
    {
        mocker.GetMock<IEquipmentDataService>()
            .Verify(x => x.ExistsAsync(id), Times.Once);
        return mocker;
    }

    public static AutoMocker VerifyEquipmentDataServiceExistsAsyncNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IEquipmentDataService>()
            .Verify(x => x.ExistsAsync(It.IsAny<EquipmentId>()), Times.Never);
        return mocker;
    }


    // MuscleGroup Cache Extensions
    public static AutoMocker SetupMuscleGroupCacheMiss(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<MuscleGroupDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<MuscleGroupDto>>.Miss());

        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<MuscleGroupDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<MuscleGroupDto>.Miss());

        return mocker;
    }

    public static AutoMocker SetupMuscleGroupCacheHit(this AutoMocker mocker, IEnumerable<MuscleGroupDto> items)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<MuscleGroupDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<MuscleGroupDto>>.Hit(items));
        return mocker;
    }

    public static AutoMocker SetupMuscleGroupCacheHit(this AutoMocker mocker, MuscleGroupDto item)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<MuscleGroupDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<MuscleGroupDto>.Hit(item));
        return mocker;
    }

    public static AutoMocker VerifyMuscleGroupCacheGetOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.GetAsync<MuscleGroupDto>(It.IsAny<string>()), Times.AtLeastOnce);
        return mocker;
    }

    public static AutoMocker VerifyMuscleGroupCacheGetNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.GetAsync<MuscleGroupDto>(It.IsAny<string>()), Times.Never);
        return mocker;
    }

    public static AutoMocker VerifyMuscleGroupCacheSetOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<MuscleGroupDto>()), Times.AtLeastOnce);
        return mocker;
    }

    // MuscleGroup DataService Verification Extensions
    public static AutoMocker VerifyMuscleGroupDataServiceGetAllActiveOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IMuscleGroupDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Once);
        return mocker;
    }

    public static AutoMocker VerifyMuscleGroupDataServiceGetAllActiveNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IMuscleGroupDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Never);
        return mocker;
    }

    public static AutoMocker VerifyMuscleGroupDataServiceGetByIdOnce(this AutoMocker mocker, MuscleGroupId id)
    {
        mocker.GetMock<IMuscleGroupDataService>()
            .Verify(x => x.GetByIdAsync(id), Times.Once);
        return mocker;
    }

    public static AutoMocker SetupMuscleGroupDataServiceExistsAsync(this AutoMocker mocker, MuscleGroupId id, BooleanResultDto result)
    {
        mocker.GetMock<IMuscleGroupDataService>()
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(result));
        return mocker;
    }

    public static AutoMocker VerifyMuscleGroupDataServiceExistsAsyncOnce(this AutoMocker mocker, MuscleGroupId id)
    {
        mocker.GetMock<IMuscleGroupDataService>()
            .Verify(x => x.ExistsAsync(id), Times.Once);
        return mocker;
    }

    public static AutoMocker VerifyMuscleGroupDataServiceExistsAsyncNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IMuscleGroupDataService>()
            .Verify(x => x.ExistsAsync(It.IsAny<MuscleGroupId>()), Times.Never);
        return mocker;
    }

}