using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Results;
using Microsoft.Extensions.Logging;

namespace GetFitterGetBigger.Admin.Extensions
{
    internal static class WorkoutTemplateListStoreExtensions
    {
        /// <summary>
        /// Extracts data from a successful ServiceResult, returning Empty if null
        /// </summary>
        internal static WorkoutTemplatePagedResultDto ExtractData(this ServiceResult<WorkoutTemplatePagedResultDto> result)
        {
            return result.Data ?? WorkoutTemplatePagedResultDto.Empty();
        }

        /// <summary>
        /// Extracts a user-friendly error message from a failed ServiceResult
        /// </summary>
        internal static string ExtractErrorMessage(this ServiceResult<WorkoutTemplatePagedResultDto> result)
        {
            return result.Errors.FirstOrDefault()?.Message ?? "Failed to load workout templates";
        }

        /// <summary>
        /// Logs the operation result with appropriate log level and details
        /// </summary>
        internal static void LogOperation<T>(this ServiceResult<T> result, string operation, ILogger logger)
        {
            switch (result.IsSuccess)
            {
                case true:
                    var count = (result.Data as WorkoutTemplatePagedResultDto)?.TotalCount;
                    if (count.HasValue)
                    {
                        logger.LogDebug("Successfully {Operation}: {Count} items", operation, count.Value);
                    }
                    else
                    {
                        logger.LogDebug("Successfully {Operation}", operation);
                    }
                    break;
                    
                case false:
                    var error = result.Errors.FirstOrDefault()?.Message ?? "Unknown error";
                    logger.LogWarning("Failed to {Operation}: {Error}", operation, error);
                    break;
            }
        }
    }
}