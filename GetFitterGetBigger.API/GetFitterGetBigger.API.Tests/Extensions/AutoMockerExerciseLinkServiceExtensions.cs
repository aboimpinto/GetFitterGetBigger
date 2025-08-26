using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Extensions;

/// <summary>
/// Simplified AutoMocker extensions for ExerciseLink service testing
/// </summary>
public static class AutoMockerExerciseLinkServiceExtensions
{
    /// <summary>
    /// Sets up the ExerciseLink service to return a successful result with the provided link
    /// </summary>
    public static AutoMocker SetupExerciseLinkService(this AutoMocker mocker, ExerciseLink exerciseLink)
    {
        var linkDto = new ExerciseLinkDto
        {
            Id = exerciseLink.Id.ToString(),
            SourceExerciseId = exerciseLink.SourceExerciseId.ToString(),
            TargetExerciseId = exerciseLink.TargetExerciseId.ToString(),
            LinkType = exerciseLink.LinkType,
            DisplayOrder = exerciseLink.DisplayOrder,
            IsActive = exerciseLink.IsActive
        };

        // Setup command data service for creation
        mocker.GetMock<IExerciseLinkCommandDataService>()
            .Setup(x => x.CreateAsync(It.IsAny<ExerciseLinkDto>()))
            .ReturnsAsync(ServiceResult<ExerciseLinkDto>.Success(linkDto));

        // Setup exercise service for source exercise validation
        var sourceExerciseDto = new ExerciseDto
        {
            Id = exerciseLink.SourceExerciseId.ToString(),
            Name = $"Source Exercise {exerciseLink.SourceExerciseId}",
            ExerciseTypes = new List<ReferenceDataDto>
            {
                new ReferenceDataDto { Id = "2", Value = "WORKOUT", Description = "Workout exercise type" }
            }
        };
            
        mocker.GetMock<IExerciseService>()
            .Setup(x => x.GetByIdAsync(exerciseLink.SourceExerciseId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(sourceExerciseDto));

        // Setup exercise service for target exercise validation
        var targetExerciseDto = new ExerciseDto
        {
            Id = exerciseLink.TargetExerciseId.ToString(),
            Name = $"Target Exercise {exerciseLink.TargetExerciseId}",
            ExerciseTypes = new List<ReferenceDataDto>
            {
                new ReferenceDataDto { Id = "2", Value = "WORKOUT", Description = "Workout exercise type" }
            }
        };
            
        mocker.GetMock<IExerciseService>()
            .Setup(x => x.GetByIdAsync(exerciseLink.TargetExerciseId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(targetExerciseDto));

        return mocker;
    }

    /// <summary>
    /// Sets up the ExerciseLink query service to return a list of links
    /// </summary>
    public static AutoMocker SetupExerciseLinkQueryService(this AutoMocker mocker, List<ExerciseLink> exerciseLinks)
    {
        var linkDtos = exerciseLinks.Select(link => new ExerciseLinkDto
        {
            Id = link.Id.ToString(),
            SourceExerciseId = link.SourceExerciseId.ToString(),
            TargetExerciseId = link.TargetExerciseId.ToString(),
            LinkType = link.LinkType,
            DisplayOrder = link.DisplayOrder,
            IsActive = link.IsActive
        }).ToList();

        var responseDto = new ExerciseLinksResponseDto
        {
            Links = linkDtos,
            ExerciseId = exerciseLinks.FirstOrDefault()?.SourceExerciseId.ToString() ?? "exercise-test-123"
        };

        mocker.GetMock<IExerciseLinkQueryDataService>()
            .Setup(x => x.GetLinksAsync(It.IsAny<GetExerciseLinksCommand>()))
            .ReturnsAsync(ServiceResult<ExerciseLinksResponseDto>.Success(responseDto));

        return mocker;
    }

    /// <summary>
    /// Sets up the ExerciseLink service to return a deletion success result
    /// </summary>
    public static AutoMocker SetupExerciseLinkDeletion(this AutoMocker mocker, bool success = true)
    {
        mocker.GetMock<IExerciseLinkCommandDataService>()
            .Setup(x => x.DeleteAsync(It.IsAny<ExerciseLinkId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = success }));

        return mocker;
    }
}