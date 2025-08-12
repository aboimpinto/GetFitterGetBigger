using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Tests.TestHelpers;
using Moq;
using Moq.AutoMock;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Tests.Services.Extensions;

public static class AutoMockerWorkoutTemplateServiceExtensions
{
    // Setup methods for WorkoutTemplate-specific operations
    public static AutoMocker SetupWorkoutTemplateUnitOfWork(this AutoMocker mocker)
    {
        var workoutTemplateRepositoryMock = mocker.GetMock<IWorkoutTemplateRepository>();

        var readOnlyUnitOfWorkMock = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        readOnlyUnitOfWorkMock
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(workoutTemplateRepositoryMock.Object);

        var writableUnitOfWorkMock = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        writableUnitOfWorkMock
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(workoutTemplateRepositoryMock.Object);
        writableUnitOfWorkMock
            .Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);

        mocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(readOnlyUnitOfWorkMock.Object);

        mocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateWritable())
            .Returns(writableUnitOfWorkMock.Object);

        return mocker;
    }

    public static AutoMocker SetupWorkoutTemplateGetByIdWithDetails(this AutoMocker mocker, WorkoutTemplate returnValue)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(returnValue);

        return mocker;
    }

    public static AutoMocker SetupWorkoutTemplateGetById(this AutoMocker mocker, WorkoutTemplate returnValue)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(x => x.GetByIdAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(returnValue);

        return mocker;
    }

    public static AutoMocker SetupWorkoutTemplateExists(this AutoMocker mocker, string name, bool exists)
    {
        // Setup for ExistsByNameAsync with exclude ID parameter
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(r => r.ExistsByNameAsync(It.Is<string>(n => n == name), It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(exists);

        // Also setup GetWorkoutTemplatesQueryable for the name uniqueness check
        if (exists)
        {
            // If name exists, return an empty queryable (we're not testing the actual uniqueness logic here)
            mocker.GetMock<IWorkoutTemplateRepository>()
                .Setup(r => r.GetWorkoutTemplatesQueryable())
                .Returns(new List<WorkoutTemplate>().AsQueryable());
        }

        return mocker;
    }

    public static AutoMocker SetupWorkoutTemplateExistsById(this AutoMocker mocker, WorkoutTemplateId id, bool exists)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(exists);

        return mocker;
    }

    public static AutoMocker SetupWorkoutTemplateAdd(this AutoMocker mocker, WorkoutTemplate returnValue)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(r => r.AddAsync(It.IsAny<WorkoutTemplate>()))
            .ReturnsAsync(returnValue);

        return mocker;
    }

    public static AutoMocker SetupWorkoutTemplateUpdate(this AutoMocker mocker, WorkoutTemplate returnValue)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(r => r.UpdateAsync(It.IsAny<WorkoutTemplate>()))
            .ReturnsAsync(returnValue);

        return mocker;
    }

    public static AutoMocker SetupWorkoutTemplateSoftDelete(this AutoMocker mocker, bool success = true)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(r => r.SoftDeleteAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(success);

        return mocker;
    }

    public static AutoMocker SetupWorkoutTemplateDelete(this AutoMocker mocker, bool success = true)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(r => r.DeleteAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(success);

        return mocker;
    }

    public static AutoMocker SetupWorkoutTemplateQueryable(this AutoMocker mocker, IEnumerable<WorkoutTemplate> templates)
    {
        var asyncQueryable = templates.ToList().BuildAsyncQueryable();
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(r => r.GetWorkoutTemplatesQueryable())
            .Returns(asyncQueryable);

        return mocker;
    }

    // Setup methods for dependent services
    public static AutoMocker SetupWorkoutStateService(this AutoMocker mocker, WorkoutStateDto workoutStateDto)
    {
        mocker.GetMock<IWorkoutStateService>()
            .Setup(s => s.GetByValueAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<WorkoutStateDto>.Success(workoutStateDto));

        return mocker;
    }

    public static AutoMocker SetupExerciseService(this AutoMocker mocker)
    {
        // Setup basic exercise service if needed
        return mocker;
    }

    public static AutoMocker SetupWorkoutTemplateExerciseService(this AutoMocker mocker)
    {
        // Setup basic workout template exercise service if needed
        return mocker;
    }

    // Verification methods - Positive cases
    public static AutoMocker VerifyWorkoutTemplateGetByIdWithDetailsOnce(this AutoMocker mocker, WorkoutTemplateId templateId)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.GetByIdWithDetailsAsync(templateId), Times.Once());

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateGetByIdOnce(this AutoMocker mocker, WorkoutTemplateId templateId)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.GetByIdAsync(templateId), Times.Once());

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateExistsByNameOnce(this AutoMocker mocker, string name)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.ExistsByNameAsync(name, It.IsAny<WorkoutTemplateId>()), Times.Once());

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateExistsByIdOnce(this AutoMocker mocker, WorkoutTemplateId id)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.ExistsAsync(id), Times.Once());

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateAddOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.AddAsync(It.IsAny<WorkoutTemplate>()), Times.Once());

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateUpdateOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.UpdateAsync(It.IsAny<WorkoutTemplate>()), Times.Once());

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateSoftDeleteOnce(this AutoMocker mocker, WorkoutTemplateId templateId)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.SoftDeleteAsync(templateId), Times.Once());

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateDeleteOnce(this AutoMocker mocker, WorkoutTemplateId templateId)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.DeleteAsync(templateId), Times.Once());

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateQueryableOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.GetWorkoutTemplatesQueryable(), Times.Once());

        return mocker;
    }

    public static AutoMocker VerifyWritableUnitOfWorkCreatedOnce(this AutoMocker mocker)
    {
        mocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Verify(x => x.CreateWritable(), Times.Once());

        return mocker;
    }


    // Verification methods - Negative cases (explicit intent)
    public static AutoMocker VerifyWorkoutTemplateGetByIdWithDetailsNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()), Times.Never);

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateGetByIdNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.GetByIdAsync(It.IsAny<WorkoutTemplateId>()), Times.Never);

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateAddNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.AddAsync(It.IsAny<WorkoutTemplate>()), Times.Never);

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateUpdateNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.UpdateAsync(It.IsAny<WorkoutTemplate>()), Times.Never);

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateSoftDeleteNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.SoftDeleteAsync(It.IsAny<WorkoutTemplateId>()), Times.Never);

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateDeleteNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IWorkoutTemplateRepository>()
            .Verify(x => x.DeleteAsync(It.IsAny<WorkoutTemplateId>()), Times.Never);

        return mocker;
    }

    public static AutoMocker VerifyWorkoutTemplateRepositoryNotAccessedForValidationFailure(this AutoMocker mocker)
    {
        // Explicit intent - validation failed so repository should not be accessed
        mocker.GetMock<IWorkoutTemplateRepository>()
            .VerifyNoOtherCalls();

        return mocker;
    }
}