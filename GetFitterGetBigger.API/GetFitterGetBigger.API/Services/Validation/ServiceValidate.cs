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

    /// <summary>
    /// Starts a new validation chain for service operations with a specific return type.
    /// This enables the OnSuccess/OnFailure pattern for service validations.
    /// </summary>
    /// <typeparam name="T">The type of data the service operation will return</typeparam>
    /// <returns>A new ServiceValidation&lt;T&gt; instance for building the validation chain</returns>
    public static ServiceValidation<T> For<T>()
    {
        return new ServiceValidation<T>();
    }

    /// <summary>
    /// Starts a new validation chain builder for service operations with async validations.
    /// Use this when you need to perform async validations like database existence checks.
    /// </summary>
    /// <typeparam name="T">The type of data the service operation will return</typeparam>
    /// <returns>A new ServiceValidationBuilder&lt;T&gt; instance for building the validation chain</returns>
    public static ServiceValidationBuilder<T> Build<T>()
    {
        return new ServiceValidationBuilder<T>(new ServiceValidation<T>());
    }

    /// <summary>
    /// Starts a new validation chain builder for service operations with async validations that return ValidationResult.
    /// Use this when you need to perform async validations in methods that return ValidationResult.
    /// </summary>
    /// <returns>A new NonGenericServiceValidationBuilder instance for building the validation chain</returns>
    public static NonGenericServiceValidationBuilder Build()
    {
        return new NonGenericServiceValidationBuilder(new ServiceValidation());
    }
}