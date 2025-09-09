using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;

public interface ICopyRoundHandler
{
    Task<ServiceResult<CopyRoundResultDto>> CopyRoundAsync(
        WorkoutTemplateId templateId,
        CopyRoundDto dto);
}