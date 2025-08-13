using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.BodyPart.DataServices;
using GetFitterGetBigger.API.Services.Results;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services.Extensions;

/// <summary>
/// AutoMocker extension methods specifically for BodyPartService testing
/// These methods help set up and verify BodyPart DataService interactions
/// </summary>
public static class AutoMockerBodyPartServiceExtensions
{
    // DataService setup methods
    public static AutoMocker SetupBodyPartDataServiceGetAllActive(this AutoMocker mocker, IEnumerable<BodyPartDto> bodyParts)
    {
        mocker.GetMock<IBodyPartDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<BodyPartDto>>.Success(bodyParts));

        return mocker;
    }

    public static AutoMocker SetupBodyPartDataServiceGetById(this AutoMocker mocker, BodyPartId id, BodyPartDto bodyPart)
    {
        mocker.GetMock<IBodyPartDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<BodyPartDto>.Success(bodyPart));

        return mocker;
    }

    public static AutoMocker SetupBodyPartDataServiceGetByIdNotFound(this AutoMocker mocker, BodyPartId id)
    {
        mocker.GetMock<IBodyPartDataService>()
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<BodyPartDto>.Success(BodyPartDto.Empty));

        return mocker;
    }

    public static AutoMocker SetupBodyPartDataServiceGetByValue(this AutoMocker mocker, string value, BodyPartDto bodyPart)
    {
        mocker.GetMock<IBodyPartDataService>()
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(ServiceResult<BodyPartDto>.Success(bodyPart));

        return mocker;
    }

    public static AutoMocker SetupBodyPartDataServiceGetByValueNotFound(this AutoMocker mocker, string value)
    {
        mocker.GetMock<IBodyPartDataService>()
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(ServiceResult<BodyPartDto>.Success(BodyPartDto.Empty));

        return mocker;
    }

    public static AutoMocker SetupBodyPartDataServiceExists(this AutoMocker mocker, BodyPartId id, bool exists)
    {
        var result = BooleanResultDto.Create(exists);
        mocker.GetMock<IBodyPartDataService>()
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(result));

        return mocker;
    }

    // Cache setup methods
    public static AutoMocker SetupBodyPartCacheMiss(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<BodyPartDto>.Miss());

        return mocker;
    }

    public static AutoMocker SetupBodyPartCacheMissList(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<BodyPartDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<BodyPartDto>>.Miss());

        return mocker;
    }

    public static AutoMocker SetupBodyPartCacheHit(this AutoMocker mocker, BodyPartDto bodyPart)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<BodyPartDto>.Hit(bodyPart));

        return mocker;
    }

    public static AutoMocker SetupBodyPartCacheHitList(this AutoMocker mocker, IEnumerable<BodyPartDto> bodyParts)
    {
        mocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<BodyPartDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<BodyPartDto>>.Hit(bodyParts));

        return mocker;
    }

    // Verification methods - DataService calls
    public static AutoMocker VerifyBodyPartDataServiceGetAllActiveOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IBodyPartDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Once);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartDataServiceGetByIdOnce(this AutoMocker mocker, BodyPartId id)
    {
        mocker.GetMock<IBodyPartDataService>()
            .Verify(x => x.GetByIdAsync(id), Times.Once);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartDataServiceGetByValueOnce(this AutoMocker mocker, string value)
    {
        mocker.GetMock<IBodyPartDataService>()
            .Verify(x => x.GetByValueAsync(value), Times.Once);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartDataServiceExistsOnce(this AutoMocker mocker, BodyPartId id)
    {
        mocker.GetMock<IBodyPartDataService>()
            .Verify(x => x.ExistsAsync(id), Times.Once);

        return mocker;
    }

    // Verification methods - Never called (explicit intent)
    public static AutoMocker VerifyBodyPartDataServiceGetByIdNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IBodyPartDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<BodyPartId>()), Times.Never);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartDataServiceGetByValueNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IBodyPartDataService>()
            .Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartDataServiceGetAllActiveNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IBodyPartDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Never);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartDataServiceExistsNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IBodyPartDataService>()
            .Verify(x => x.ExistsAsync(It.IsAny<BodyPartId>()), Times.Never);

        return mocker;
    }

    // Cache verification methods
    public static AutoMocker VerifyBodyPartCacheGetOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()), Times.Once);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartCacheGetListOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.GetAsync<IEnumerable<BodyPartDto>>(It.IsAny<string>()), Times.Once);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartCacheSetOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<BodyPartDto>()), Times.Once);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartCacheSetListOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<BodyPartDto>>()), Times.Once);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartCacheNeverAccessed(this AutoMocker mocker)
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.GetAsync<BodyPartDto>(It.IsAny<string>()), Times.Never);
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<BodyPartDto>()), Times.Never);

        return mocker;
    }
}