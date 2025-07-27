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