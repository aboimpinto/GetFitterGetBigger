using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Moq;

namespace GetFitterGetBigger.API.Tests.Extensions;

/// <summary>
/// Extension methods for fluent mock setup to make tests more readable and maintainable.
/// </summary>
public static class MockExtensions
{
    /// <summary>
    /// Sets up the ExerciseService mock to return a specific ExerciseDto for a given ID.
    /// </summary>
    public static Mock<GetFitterGetBigger.API.Services.Exercise.IExerciseService> SetupExerciseById(
        this Mock<GetFitterGetBigger.API.Services.Exercise.IExerciseService> mock,
        ExerciseId exerciseId,
        ExerciseDto exerciseDto)
    {
        mock.Setup(x => x.GetByIdAsync(exerciseId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(exerciseDto));
        return mock;
    }
    
    /// <summary>
    /// Sets up the ExerciseService mock to return a failed result for a given ID.
    /// </summary>
    public static Mock<GetFitterGetBigger.API.Services.Exercise.IExerciseService> SetupExerciseNotFound(
        this Mock<GetFitterGetBigger.API.Services.Exercise.IExerciseService> mock,
        ExerciseId exerciseId)
    {
        mock.Setup(x => x.GetByIdAsync(exerciseId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Failure(
                ExerciseDto.Empty, 
                ServiceError.NotFound("Exercise", exerciseId.ToString())));
        return mock;
    }
    
    /// <summary>
    /// Sets up the QueryDataService mock to return a link count for a specific exercise and link type.
    /// </summary>
    public static Mock<IExerciseLinkQueryDataService> SetupLinkCount(
        this Mock<IExerciseLinkQueryDataService> mock,
        ExerciseId exerciseId,
        string linkType,
        int count = 0)
    {
        mock.Setup(x => x.GetLinkCountAsync(exerciseId, linkType))
            .ReturnsAsync(ServiceResult<int>.Success(count));
        return mock;
    }
    
    /// <summary>
    /// Sets up the QueryDataService mock to return link counts for any exercise and link type.
    /// </summary>
    public static Mock<IExerciseLinkQueryDataService> SetupAnyLinkCount(
        this Mock<IExerciseLinkQueryDataService> mock,
        int count = 0)
    {
        mock.Setup(x => x.GetLinkCountAsync(It.IsAny<ExerciseId>(), It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<int>.Success(count));
        return mock;
    }
    
    /// <summary>
    /// Sets up the CommandDataService mock to return a successful bidirectional link creation.
    /// </summary>
    public static Mock<IExerciseLinkCommandDataService> SetupSuccessfulBidirectionalCreation(
        this Mock<IExerciseLinkCommandDataService> mock,
        ExerciseLinkDto expectedResult)
    {
        mock.Setup(x => x.CreateBidirectionalAsync(
                It.IsAny<ExerciseLinkDto>(),
                It.IsAny<ExerciseLinkDto>()))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(expectedResult));
        return mock;
    }
    
    /// <summary>
    /// Sets up the CommandDataService mock to fail bidirectional link creation.
    /// </summary>
    public static Mock<IExerciseLinkCommandDataService> SetupFailedBidirectionalCreation(
        this Mock<IExerciseLinkCommandDataService> mock,
        ServiceError error)
    {
        mock.Setup(x => x.CreateBidirectionalAsync(
                It.IsAny<ExerciseLinkDto>(),
                It.IsAny<ExerciseLinkDto>()))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Failure(ExerciseLinkDto.Empty, error));
        return mock;
    }

    // ===== BIDIRECTIONAL DELETION MOCK EXTENSIONS =====

    /// <summary>
    /// Sets up the QueryDataService mock to confirm that a link exists.
    /// </summary>
    public static Mock<IExerciseLinkQueryDataService> SetupExerciseLinkExists(
        this Mock<IExerciseLinkQueryDataService> mock,
        ExerciseLinkId linkId,
        ExerciseLinkDto existingLink)
    {
        mock.Setup(x => x.GetByIdAsync(linkId))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(existingLink));
        return mock;
    }

    /// <summary>
    /// Sets up the QueryDataService mock to confirm that a link does not exist.
    /// </summary>
    public static Mock<IExerciseLinkQueryDataService> SetupExerciseLinkNotExists(
        this Mock<IExerciseLinkQueryDataService> mock,
        ExerciseLinkId linkId)
    {
        mock.Setup(x => x.GetByIdAsync(linkId))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(ExerciseLinkDto.Empty));
        return mock;
    }

    /// <summary>
    /// Sets up the QueryDataService mock to confirm that a link belongs to an exercise.
    /// </summary>
    public static Mock<IExerciseLinkQueryDataService> SetupExerciseLinkBelongsToExercise(
        this Mock<IExerciseLinkQueryDataService> mock,
        string exerciseId,
        ExerciseLinkId linkId,
        bool belongs)
    {
        if (belongs)
        {
            var linkDto = ExerciseLinkDtoTestBuilder.Default()
                .WithId(linkId)
                .WithSourceExercise(exerciseId)
                .Build();
            
            mock.Setup(x => x.GetByIdAsync(linkId))
                .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(linkDto));
        }
        else
        {
            mock.Setup(x => x.GetByIdAsync(linkId))
                .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(ExerciseLinkDto.Empty));
        }
        return mock;
    }

    /// <summary>
    /// Sets up the QueryDataService mock to return links by source exercise with enum filtering.
    /// </summary>
    public static Mock<IExerciseLinkQueryDataService> SetupGetBySourceExerciseWithEnum(
        this Mock<IExerciseLinkQueryDataService> mock,
        ExerciseId sourceId,
        ExerciseLinkType linkType,
        List<ExerciseLinkDto> links)
    {
        mock.Setup(x => x.GetBySourceExerciseWithEnumAsync(sourceId, linkType))
            .ReturnsAsync(ServiceResult<List<ExerciseLinkDto>>.Success(links));
        return mock;
    }

    /// <summary>
    /// Sets up the CommandDataService mock to successfully delete a link.
    /// </summary>
    public static Mock<IExerciseLinkCommandDataService> SetupSuccessfulDelete(
        this Mock<IExerciseLinkCommandDataService> mock,
        ExerciseLinkId linkId)
    {
        mock.Setup(x => x.DeleteAsync(linkId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        return mock;
    }

    /// <summary>
    /// Sets up the CommandDataService mock to fail to delete a link.
    /// </summary>
    public static Mock<IExerciseLinkCommandDataService> SetupFailedDelete(
        this Mock<IExerciseLinkCommandDataService> mock,
        ExerciseLinkId linkId)
    {
        mock.Setup(x => x.DeleteAsync(linkId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Create(false),
                ServiceError.InternalError("Failed to delete exercise link")));
        return mock;
    }
}