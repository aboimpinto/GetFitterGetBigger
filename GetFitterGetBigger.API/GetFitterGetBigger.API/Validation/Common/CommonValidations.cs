using System.Text.RegularExpressions;
using GetFitterGetBigger.API.Models.Interfaces;

namespace GetFitterGetBigger.API.Validation.Common;

/// <summary>
/// Provides common validation methods that can be used across all layers of the application.
/// This class contains reusable validation logic to avoid duplication.
/// </summary>
public static class CommonValidations
{
    /// <summary>
    /// Email validation regex pattern
    /// </summary>
    private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    /// <summary>
    /// URL validation regex pattern
    /// </summary>
    private const string UrlPattern = @"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$";

    /// <summary>
    /// Validates if a string is a valid email address.
    /// </summary>
    /// <param name="email">The email to validate</param>
    /// <returns>True if valid email, false otherwise</returns>
    public static bool IsValidEmail(string? email)
    {
        return !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, EmailPattern);
    }

    /// <summary>
    /// Validates if a string is a valid URL.
    /// </summary>
    /// <param name="url">The URL to validate</param>
    /// <returns>True if valid URL, false otherwise</returns>
    public static bool IsValidUrl(string? url)
    {
        return !string.IsNullOrWhiteSpace(url) && Regex.IsMatch(url, UrlPattern);
    }

    /// <summary>
    /// Validates if a specialized ID is valid (not null and not empty).
    /// </summary>
    /// <typeparam name="TId">The type of specialized ID</typeparam>
    /// <param name="id">The ID to validate</param>
    /// <returns>True if valid ID, false otherwise</returns>
    public static bool IsValidId<TId>(TId? id) where TId : ISpecializedIdBase
    {
        return id != null && !id.IsEmpty;
    }

    /// <summary>
    /// Validates if a string represents a valid specialized ID format.
    /// </summary>
    /// <param name="idString">The ID string to validate</param>
    /// <param name="expectedPrefix">The expected prefix for the ID type</param>
    /// <returns>True if valid ID format, false otherwise</returns>
    public static bool IsValidIdFormat(string? idString, string expectedPrefix)
    {
        if (string.IsNullOrWhiteSpace(idString))
            return false;

        var parts = idString.Split('-');
        if (parts.Length != 2)
            return false;

        return parts[0] == expectedPrefix && Guid.TryParse(parts[1], out _);
    }

    /// <summary>
    /// Validates if a value is within a specified range.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <returns>True if within range, false otherwise</returns>
    public static bool IsWithinRange(int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Validates if a value is within a specified range.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <returns>True if within range, false otherwise</returns>
    public static bool IsWithinRange(decimal value, decimal min, decimal max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Validates if a string contains only alphanumeric characters and spaces.
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsAlphanumericWithSpaces(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, @"^[a-zA-Z0-9\s]+$");
    }

    /// <summary>
    /// Validates if a string contains only letters and spaces.
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsAlphabeticWithSpaces(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, @"^[a-zA-Z\s]+$");
    }

    /// <summary>
    /// Validates if a collection has a specific count of elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="count">The expected count</param>
    /// <returns>True if count matches, false otherwise</returns>
    public static bool HasExactCount<T>(System.Collections.Generic.IEnumerable<T>? collection, int count)
    {
        return collection?.Count() == count;
    }

    /// <summary>
    /// Validates if a collection has at least a minimum count of elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="minCount">The minimum count</param>
    /// <returns>True if count is at least minimum, false otherwise</returns>
    public static bool HasMinCount<T>(System.Collections.Generic.IEnumerable<T>? collection, int minCount)
    {
        return collection?.Count() >= minCount;
    }

    /// <summary>
    /// Validates if a collection has at most a maximum count of elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="maxCount">The maximum count</param>
    /// <returns>True if count is at most maximum, false otherwise</returns>
    public static bool HasMaxCount<T>(System.Collections.Generic.IEnumerable<T>? collection, int maxCount)
    {
        return collection?.Count() <= maxCount;
    }

    /// <summary>
    /// Validates if a date is in the past.
    /// </summary>
    /// <param name="date">The date to validate</param>
    /// <returns>True if date is in the past, false otherwise</returns>
    public static bool IsInPast(DateTime date)
    {
        return date < DateTime.UtcNow;
    }

    /// <summary>
    /// Validates if a date is in the future.
    /// </summary>
    /// <param name="date">The date to validate</param>
    /// <returns>True if date is in the future, false otherwise</returns>
    public static bool IsInFuture(DateTime date)
    {
        return date > DateTime.UtcNow;
    }

    /// <summary>
    /// Validates if a date is within a specific range.
    /// </summary>
    /// <param name="date">The date to validate</param>
    /// <param name="start">The start date (inclusive)</param>
    /// <param name="end">The end date (inclusive)</param>
    /// <returns>True if date is within range, false otherwise</returns>
    public static bool IsDateInRange(DateTime date, DateTime start, DateTime end)
    {
        return date >= start && date <= end;
    }

    /// <summary>
    /// Validates if a string is a valid GUID.
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <returns>True if valid GUID, false otherwise</returns>
    public static bool IsValidGuid(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out _);
    }

    /// <summary>
    /// Validates if a decimal value is a positive number.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>True if positive, false otherwise</returns>
    public static bool IsPositive(decimal value)
    {
        return value > 0;
    }

    /// <summary>
    /// Validates if a decimal value is non-negative.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>True if non-negative, false otherwise</returns>
    public static bool IsNonNegative(decimal value)
    {
        return value >= 0;
    }

    /// <summary>
    /// Validates if an integer value is a positive number.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>True if positive, false otherwise</returns>
    public static bool IsPositive(int value)
    {
        return value > 0;
    }

    /// <summary>
    /// Validates if an integer value is non-negative.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>True if non-negative, false otherwise</returns>
    public static bool IsNonNegative(int value)
    {
        return value >= 0;
    }
}