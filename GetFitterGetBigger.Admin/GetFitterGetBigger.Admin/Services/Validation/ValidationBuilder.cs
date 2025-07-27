using System.Linq.Expressions;
using GetFitterGetBigger.Admin.Models.Errors;
using GetFitterGetBigger.Admin.Models.Results;

namespace GetFitterGetBigger.Admin.Services.Validation;

public class ValidationBuilder<T>
{
    private readonly T _value;
    private readonly List<ServiceError> _errors = new();

    private ValidationBuilder(T value) => _value = value;

    public static ValidationBuilder<T> For(T value) => new(value);

    public ValidationBuilder<T> Ensure(
        Func<T, bool> predicate, 
        ServiceErrorCode errorCode, 
        string errorMessage)
    {
        if (!predicate(_value))
            _errors.Add(new ServiceError(errorCode, errorMessage));
        return this;
    }

    public ValidationBuilder<T> EnsureNotEmpty(
        Func<T, string?> selector, 
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(selector(_value)))
            _errors.Add(ServiceError.ValidationRequired(fieldName));
        return this;
    }

    public ValidationBuilder<T> EnsureRange<TValue>(
        Func<T, TValue> selector,
        TValue min,
        TValue max,
        string fieldName) where TValue : IComparable<TValue>
    {
        var value = selector(_value);
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            _errors.Add(ServiceError.ValidationOutOfRange(fieldName, $"{min} - {max}"));
        return this;
    }

    public ValidationBuilder<T> EnsureMaxLength(
        Func<T, string?> selector,
        int maxLength,
        string fieldName)
    {
        var value = selector(_value);
        if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            _errors.Add(ServiceError.ValidationOutOfRange(fieldName, $"maximum {maxLength} characters"));
        return this;
    }

    public ValidationBuilder<T> EnsureMinLength(
        Func<T, string?> selector,
        int minLength,
        string fieldName)
    {
        var value = selector(_value);
        if (!string.IsNullOrEmpty(value) && value.Length < minLength)
            _errors.Add(ServiceError.ValidationOutOfRange(fieldName, $"minimum {minLength} characters"));
        return this;
    }

    public ValidationBuilder<T> EnsureValid<TValue>(
        Func<T, TValue> selector,
        Func<TValue, bool> validator,
        string fieldName,
        string reason)
    {
        var value = selector(_value);
        if (!validator(value))
            _errors.Add(ServiceError.ValidationInvalid(fieldName, reason));
        return this;
    }

    public ValidationBuilder<T> EnsureCollection<TItem>(
        Func<T, IEnumerable<TItem>?> selector,
        string fieldName,
        int? minCount = null,
        int? maxCount = null)
    {
        var collection = selector(_value)?.ToList();
        
        if (collection == null || !collection.Any())
        {
            if (minCount > 0)
                _errors.Add(ServiceError.ValidationRequired(fieldName));
        }
        else
        {
            if (minCount.HasValue && collection.Count < minCount.Value)
                _errors.Add(ServiceError.ValidationOutOfRange(fieldName, $"minimum {minCount.Value} items"));
                
            if (maxCount.HasValue && collection.Count > maxCount.Value)
                _errors.Add(ServiceError.ValidationOutOfRange(fieldName, $"maximum {maxCount.Value} items"));
        }
        
        return this;
    }

    public ValidationBuilder<T> EnsureCappedRange(
        Expression<Func<T, int>> propertyExpression,
        int min,
        int max,
        ILogger? logger = null)
    {
        // Get the property
        var memberExpression = propertyExpression.Body as MemberExpression;
        if (memberExpression == null)
        {
            throw new ArgumentException("Expression must be a property expression");
        }
        
        var property = memberExpression.Member as System.Reflection.PropertyInfo;
        if (property == null || !property.CanWrite)
        {
            throw new ArgumentException("Expression must refer to a writable property");
        }
        
        // Extract property name for logging
        var propertyName = property.Name;
        
        // Get current value
        var originalValue = propertyExpression.Compile()(_value);
        var newValue = originalValue;
        
        // Cap the value if needed
        if (originalValue < min)
        {
            newValue = min;
            property.SetValue(_value, newValue);
            logger?.LogWarning(
                "{PropertyName} value {OriginalValue} exceeds limits, capped to {CappedValue}", 
                propertyName, originalValue, newValue);
        }
        else if (originalValue > max)
        {
            newValue = max;
            property.SetValue(_value, newValue);
            logger?.LogWarning(
                "{PropertyName} value {OriginalValue} exceeds limits, capped to {CappedValue}", 
                propertyName, originalValue, newValue);
        }
        
        // No errors since we're capping instead of failing
        return this;
    }

    public async Task<ServiceResult<TReturn>> OnSuccessAsync<TReturn>(
        Func<T, Task<ServiceResult<TReturn>>> successFunc)
    {
        return _errors.Any() 
            ? ServiceResult<TReturn>.Failure(_errors.ToArray()) 
            : await successFunc(_value);
    }

    public ServiceResult<TReturn> OnSuccess<TReturn>(
        Func<T, ServiceResult<TReturn>> successFunc)
    {
        return _errors.Any() 
            ? ServiceResult<TReturn>.Failure(_errors.ToArray()) 
            : successFunc(_value);
    }

    public Task<ServiceResult<T>> ValidateAsync()
    {
        var result = _errors.Any() 
            ? ServiceResult<T>.Failure(_errors.ToArray()) 
            : ServiceResult<T>.Success(_value);
        return Task.FromResult(result);
    }

    public ServiceResult<T> Validate()
    {
        return _errors.Any() 
            ? ServiceResult<T>.Failure(_errors.ToArray()) 
            : ServiceResult<T>.Success(_value);
    }

    public bool HasErrors => _errors.Any();
    public IReadOnlyList<ServiceError> Errors => _errors.AsReadOnly();
}