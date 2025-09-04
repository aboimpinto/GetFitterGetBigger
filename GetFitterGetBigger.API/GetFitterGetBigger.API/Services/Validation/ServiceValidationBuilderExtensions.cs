using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Common validation extensions for ServiceValidationBuilder to provide 
/// natural language validation methods that replace symbolic expressions
/// </summary>
public static class ServiceValidationBuilderExtensions
{
    #region ID Comparison Extensions
    
    /// <summary>
    /// Ensures two IDs are different (not equal)
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureIdsAreDifferent<T, TId>(
        this ServiceValidationBuilder<T> builder,
        TId sourceId,
        TId targetId,
        string errorMessage)
        where TId : ISpecializedIdBase
    {
        return builder.Ensure(() => AreDifferentIds(sourceId, targetId), errorMessage);
    }
    
    private static bool AreDifferentIds<TId>(TId sourceId, TId targetId) where TId : ISpecializedIdBase
        => !sourceId.Equals(targetId);
    
    /// <summary>
    /// Ensures two Exercise IDs are different
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureExercisesAreDifferent<T>(
        this ServiceValidationBuilder<T> builder,
        ExerciseId sourceId,
        ExerciseId targetId,
        string errorMessage)
    {
        return builder.Ensure(() => sourceId != targetId, errorMessage);
    }
    
    #endregion
    
    #region Numeric Comparison Extensions
    
    /// <summary>
    /// Ensures a value is not negative (>= 0)
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureNotNegative<T>(
        this ServiceValidationBuilder<T> builder,
        int value,
        string errorMessage)
    {
        return builder.Ensure(() => value >= 0, errorMessage);
    }
    
    /// <summary>
    /// Ensures display order is not negative
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureDisplayOrderIsNotNegative<T>(
        this ServiceValidationBuilder<T> builder,
        int displayOrder,
        string errorMessage)
    {
        return builder.Ensure(() => displayOrder >= 0, errorMessage);
    }
    
    /// <summary>
    /// Ensures a value is within a specified range (inclusive)
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureInRange<T>(
        this ServiceValidationBuilder<T> builder,
        int value,
        int min,
        int max,
        string errorMessage)
    {
        return builder.Ensure(
            () => value >= min && value <= max,
            errorMessage);
    }
    
    /// <summary>
    /// Ensures count is within a specified range
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureCountIsInRange<T>(
        this ServiceValidationBuilder<T> builder,
        int count,
        int min,
        int max,
        string errorMessage)
    {
        return builder.Ensure(
            () => count >= min && count <= max,
            errorMessage);
    }
    
    /// <summary>
    /// Ensures a value is greater than a minimum
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureGreaterThan<T>(
        this ServiceValidationBuilder<T> builder,
        int value,
        int minimum,
        string errorMessage)
    {
        return builder.Ensure(() => value > minimum, errorMessage);
    }
    
    /// <summary>
    /// Ensures a value is less than a maximum
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureLessThan<T>(
        this ServiceValidationBuilder<T> builder,
        int value,
        int maximum,
        string errorMessage)
    {
        return builder.Ensure(() => value < maximum, errorMessage);
    }
    
    #endregion
    
    #region Enum Validation Extensions
    
    /// <summary>
    /// Ensures an enum value is valid (defined in the enum)
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureValidEnum<T, TEnum>(
        this ServiceValidationBuilder<T> builder,
        TEnum value,
        string errorMessage)
        where TEnum : struct, Enum
    {
        return builder.Ensure(
            () => Enum.IsDefined(typeof(TEnum), value),
            errorMessage);
    }
    
    /// <summary>
    /// Ensures an enum value is NOT a specific value
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureEnumIsNot<T, TEnum>(
        this ServiceValidationBuilder<T> builder,
        TEnum value,
        TEnum notAllowedValue,
        string errorMessage)
        where TEnum : struct, Enum
    {
        return builder.Ensure(
            () => IsEnumValueDifferent(value, notAllowedValue),
            errorMessage);
    }
    
    private static bool IsEnumValueDifferent<TEnum>(TEnum value, TEnum notAllowedValue) where TEnum : struct, Enum
        => !value.Equals(notAllowedValue);
    
    #endregion
    
    #region Exercise Link Specific Extensions
    
    /// <summary>
    /// Ensures ExerciseLinkType is valid
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureValidLinkType<T>(
        this ServiceValidationBuilder<T> builder,
        ExerciseLinkType linkType,
        string errorMessage)
    {
        return builder.Ensure(
            () => Enum.IsDefined(typeof(ExerciseLinkType), linkType),
            errorMessage);
    }
    
    /// <summary>
    /// Ensures string link type is valid (for backward compatibility)
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureValidLinkType<T>(
        this ServiceValidationBuilder<T> builder,
        string linkType,
        string errorMessage)
    {
        return builder.Ensure(
            () => linkType == "Warmup" || linkType == "Cooldown" || 
                  Enum.TryParse<ExerciseLinkType>(linkType, out _),
            errorMessage);
    }
    
    /// <summary>
    /// Ensures link type is not WORKOUT (since WORKOUT links are auto-created)
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureLinkTypeIsNotWorkout<T>(
        this ServiceValidationBuilder<T> builder,
        ExerciseLinkType linkType,
        string errorMessage)
    {
        return builder.Ensure(
            () => linkType != ExerciseLinkType.WORKOUT,
            errorMessage);
    }
    
    #endregion
    
    #region Collection Extensions
    
    /// <summary>
    /// Ensures a collection is not empty
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureCollectionNotEmpty<T, TItem>(
        this ServiceValidationBuilder<T> builder,
        IEnumerable<TItem>? collection,
        string errorMessage)
    {
        return builder.Ensure(
            () => collection != null && collection.Any(),
            errorMessage);
    }
    
    /// <summary>
    /// Ensures a collection has a minimum number of items
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureMinimumCount<T, TItem>(
        this ServiceValidationBuilder<T> builder,
        IEnumerable<TItem>? collection,
        int minimum,
        string errorMessage)
    {
        return builder.Ensure(
            () => collection != null && collection.Count() >= minimum,
            errorMessage);
    }
    
    /// <summary>
    /// Ensures a collection has a maximum number of items
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureMaximumCount<T, TItem>(
        this ServiceValidationBuilder<T> builder,
        IEnumerable<TItem>? collection,
        int maximum,
        string errorMessage)
    {
        return builder.Ensure(
            () => collection != null && collection.Count() <= maximum,
            errorMessage);
    }
    
    #endregion
    
    #region Date/Time Extensions
    
    /// <summary>
    /// Ensures a date is in the future
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureFutureDate<T>(
        this ServiceValidationBuilder<T> builder,
        DateTime date,
        string errorMessage)
    {
        return builder.Ensure(() => date > DateTime.UtcNow, errorMessage);
    }
    
    /// <summary>
    /// Ensures a date is in the past
    /// </summary>
    public static ServiceValidationBuilder<T> EnsurePastDate<T>(
        this ServiceValidationBuilder<T> builder,
        DateTime date,
        string errorMessage)
    {
        return builder.Ensure(() => date < DateTime.UtcNow, errorMessage);
    }
    
    /// <summary>
    /// Ensures a date is within a range
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureDateInRange<T>(
        this ServiceValidationBuilder<T> builder,
        DateTime date,
        DateTime start,
        DateTime end,
        string errorMessage)
    {
        return builder.Ensure(
            () => date >= start && date <= end,
            errorMessage);
    }
    
    #endregion
    
    #region Boolean State Extensions
    
    /// <summary>
    /// Ensures a boolean value is true
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureIsTrue<T>(
        this ServiceValidationBuilder<T> builder,
        bool value,
        string errorMessage)
    {
        return builder.Ensure(() => value, errorMessage);
    }
    
    /// <summary>
    /// Ensures a boolean value is false
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureIsFalse<T>(
        this ServiceValidationBuilder<T> builder,
        bool value,
        string errorMessage)
    {
        return builder.Ensure(() => !value, errorMessage);
    }
    
    /// <summary>
    /// Ensures an entity is active
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureIsActive<T>(
        this ServiceValidationBuilder<T> builder,
        bool isActive,
        string errorMessage)
    {
        return builder.Ensure(() => isActive, errorMessage);
    }
    
    #endregion
    
    #region Percentage/Ratio Extensions
    
    /// <summary>
    /// Ensures a percentage is valid (0-100)
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureValidPercentage<T>(
        this ServiceValidationBuilder<T> builder,
        decimal percentage,
        string errorMessage)
    {
        return builder.Ensure(
            () => percentage >= 0 && percentage <= 100,
            errorMessage);
    }
    
    /// <summary>
    /// Ensures a ratio is valid (0-1)
    /// </summary>
    public static ServiceValidationBuilder<T> EnsureValidRatio<T>(
        this ServiceValidationBuilder<T> builder,
        decimal ratio,
        string errorMessage)
    {
        return builder.Ensure(
            () => ratio >= 0 && ratio <= 1,
            errorMessage);
    }
    
    #endregion
    
    #region Execution Extensions
    
    /// <summary>
    /// Executes the provided function when all validations pass.
    /// This is a convenience method that simplifies the common pattern of using MatchAsync 
    /// with only a whenValid function, automatically handling the failure case.
    /// </summary>
    /// <typeparam name="T">The type of data the service operation will return</typeparam>
    /// <param name="builder">The validation builder instance</param>
    /// <param name="whenValid">Function to execute when all validations pass</param>
    /// <returns>The result from the whenValid function or a failure result</returns>
    public static async Task<ServiceResult<T>> WhenValidAsync<T>(
        this ServiceValidationBuilder<T> builder,
        Func<Task<ServiceResult<T>>> whenValid)
    {
        return await builder.MatchAsync(
            whenValid: whenValid,
            whenInvalid: (IReadOnlyList<ServiceError> errors) => ServiceResult<T>.Failure(default(T)!, errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error")));
    }
    
    #endregion
}