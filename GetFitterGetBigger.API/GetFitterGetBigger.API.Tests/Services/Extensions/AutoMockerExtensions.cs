using System.Collections.Generic;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState;
using GetFitterGetBigger.API.Services.Results;
using Moq;
using Moq.AutoMock;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Tests.Services.Extensions;

public static class AutoMockerExtensions
{
    public static AutoMocker SetupBodyPartUnitOfWork(this AutoMocker mocker)
    {
        // IMPORTANT: Use the mock from AutoMocker, don't create a new one!
        var bodyPartRepositoryMock = mocker.GetMock<IBodyPartRepository>();

        var readOnlyUnitOfWorkMock = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        readOnlyUnitOfWorkMock
            .Setup(x => x.GetRepository<IBodyPartRepository>())
            .Returns(bodyPartRepositoryMock.Object);

        mocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(readOnlyUnitOfWorkMock.Object);

        return mocker;
    }

    public static AutoMocker SetupCacheMiss<T>(this AutoMocker mocker)
        where T : class  // Add the class constraint here!
    {
        mocker.GetMock<IEternalCacheService>()
                .Setup(x => x.GetAsync<T>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<T>.Miss());

        return mocker;
    }

    public static AutoMocker SetupCacheMissForList<T>(this AutoMocker mocker)
        where T : class
    {
        mocker.GetMock<IEternalCacheService>()
                .Setup(x => x.GetAsync<IEnumerable<T>>(It.IsAny<string>()))
                .ReturnsAsync(CacheResult<IEnumerable<T>>.Miss());

        return mocker;
    }

    public static AutoMocker SetupBodyPartGetById(this AutoMocker mocker, BodyPart returnValue)
    {
        mocker.GetMock<IBodyPartRepository>()
            .Setup(x => x.GetByIdAsync(It.IsAny<BodyPartId>()))
            .ReturnsAsync(returnValue);

        return mocker;
    }

    public static AutoMocker SetupBodyPartGetByValue(this AutoMocker mocker, string value, BodyPart returnValue)
    {
        mocker.GetMock<IBodyPartRepository>()
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(returnValue);

        return mocker;
    }

    public static AutoMocker SetupBodyPartGetAllActive(this AutoMocker mocker, IEnumerable<BodyPart> returnValue)
    {
        mocker.GetMock<IBodyPartRepository>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(returnValue);

        return mocker;
    }

    // Verification extension methods
    public static AutoMocker VerifyReadOnlyUnitOfWorkCreated(this AutoMocker mocker, Times times)
    {
        mocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Verify(x => x.CreateReadOnly(), times);

        return mocker;
    }

    public static AutoMocker VerifyReadOnlyUnitOfWorkCreatedOnce(this AutoMocker mocker)
    {
        return mocker.VerifyReadOnlyUnitOfWorkCreated(Times.Once());
    }

    public static AutoMocker VerifyBodyPartGetById(this AutoMocker mocker, BodyPartId bodyPartId, Times times)
    {
        mocker.GetMock<IBodyPartRepository>()
            .Verify(x => x.GetByIdAsync(bodyPartId), times);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartGetByIdOnce(this AutoMocker mocker, BodyPartId bodyPartId)
    {
        return mocker.VerifyBodyPartGetById(bodyPartId, Times.Once());
    }

    public static AutoMocker VerifyBodyPartGetByIdAny(this AutoMocker mocker, Times times)
    {
        mocker.GetMock<IBodyPartRepository>()
            .Verify(x => x.GetByIdAsync(It.IsAny<BodyPartId>()), times);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartGetByIdAnyOnce(this AutoMocker mocker)
    {
        return mocker.VerifyBodyPartGetByIdAny(Times.Once());
    }

    public static AutoMocker VerifyCacheSet<T>(this AutoMocker mocker, Times times)
        where T : class
    {
        mocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<T>()), times);

        return mocker;
    }

    public static AutoMocker VerifyCacheSetOnce<T>(this AutoMocker mocker)
        where T : class
    {
        return mocker.VerifyCacheSet<T>(Times.Once());
    }

    public static AutoMocker VerifyBodyPartGetByValue(this AutoMocker mocker, string value, Times times)
    {
        mocker.GetMock<IBodyPartRepository>()
            .Verify(x => x.GetByValueAsync(value), times);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartGetByValueOnce(this AutoMocker mocker, string value)
    {
        return mocker.VerifyBodyPartGetByValue(value, Times.Once());
    }

    public static AutoMocker VerifyBodyPartGetAllActive(this AutoMocker mocker, Times times)
    {
        mocker.GetMock<IBodyPartRepository>()
            .Verify(x => x.GetAllActiveAsync(), times);

        return mocker;
    }

    public static AutoMocker VerifyBodyPartGetAllActiveOnce(this AutoMocker mocker)
    {
        return mocker.VerifyBodyPartGetAllActive(Times.Once());
    }

    // Verification methods for "Never" scenarios - makes intent explicit
    public static AutoMocker VerifyBodyPartGetByIdNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IBodyPartRepository>()
            .Verify(x => x.GetByIdAsync(It.IsAny<BodyPartId>()), Times.Never);

        return mocker;
    }

    public static AutoMocker VerifyRepositoryNeverCalled<TRepository>(this AutoMocker mocker)
        where TRepository : class
    {
        // This verifies that no methods on the repository were called at all
        mocker.GetMock<TRepository>().VerifyNoOtherCalls();
        return mocker;
    }

    public static AutoMocker VerifyReadOnlyUnitOfWorkNeverCreated(this AutoMocker mocker)
    {
        mocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Verify(x => x.CreateReadOnly(), Times.Never);

        return mocker;
    }

    public static AutoMocker VerifyWritableUnitOfWorkNeverCreated(this AutoMocker mocker)
    {
        mocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Verify(x => x.CreateWritable(), Times.Never);

        return mocker;
    }

    // WorkoutStateService setup extensions
    public static AutoMocker SetupWorkoutStateService(this AutoMocker mocker, WorkoutStateDto archivedStateDto)
    {
        mocker.GetMock<IWorkoutStateService>()
            .Setup(x => x.GetByValueAsync("ARCHIVED"))
            .ReturnsAsync(ServiceResult<WorkoutStateDto>.Success(archivedStateDto));

        return mocker;
    }

    public static AutoMocker SetupWorkoutStateServiceGetByValue(this AutoMocker mocker, string value, WorkoutStateDto stateDto)
    {
        mocker.GetMock<IWorkoutStateService>()
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(ServiceResult<WorkoutStateDto>.Success(stateDto));

        return mocker;
    }
}