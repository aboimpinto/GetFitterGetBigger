using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Provides a static entry point for creating service validation chains.
/// </summary>
public static class ServiceValidate
{
    /// <summary>
    /// Starts a new validation chain for service operations.
    /// </summary>
    /// <returns>A new ServiceValidation instance for building the validation chain</returns>
    public static ServiceValidation For()
    {
        return new ServiceValidation();
    }
}