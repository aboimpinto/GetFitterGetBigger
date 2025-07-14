namespace GetFitterGetBigger.API.Services.Results;

/// <summary>
/// Represents the result of a cache lookup operation
/// Eliminates null returns by explicitly representing cache hit/miss states
/// </summary>
/// <typeparam name="T">The type of the cached value</typeparam>
public readonly struct CacheResult<T> where T : class
{
    private readonly T? _value;
    
    /// <summary>
    /// Indicates whether the cache lookup was successful (true = hit, false = miss)
    /// </summary>
    public bool IsHit { get; }
    
    /// <summary>
    /// Indicates whether the cache lookup was a miss (convenience property)
    /// </summary>
    public bool IsMiss => !IsHit;
    
    /// <summary>
    /// The cached value. Only valid when IsHit is true.
    /// Throws InvalidOperationException if accessed when IsMiss is true.
    /// </summary>
    public T Value 
    { 
        get 
        {
            if (IsMiss)
                throw new InvalidOperationException("Cannot access Value on a cache miss. Check IsHit before accessing Value.");
                
            return _value!;
        }
    }
    
    private CacheResult(bool isHit, T? value)
    {
        IsHit = isHit;
        _value = value;
    }
    
    /// <summary>
    /// Creates a cache hit result with the specified value
    /// </summary>
    public static CacheResult<T> Hit(T value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        return new CacheResult<T>(true, value);
    }
    
    /// <summary>
    /// Creates a cache miss result
    /// </summary>
    public static CacheResult<T> Miss() => new(false, null);
    
    /// <summary>
    /// Pattern matching support
    /// </summary>
    public void Deconstruct(out bool isHit, out T? value)
    {
        isHit = IsHit;
        value = isHit ? _value : null;
    }
    
    /// <summary>
    /// Executes the appropriate function based on cache hit/miss
    /// </summary>
    public TResult Match<TResult>(
        Func<T, TResult> onHit,
        Func<TResult> onMiss)
    {
        return IsHit ? onHit(_value!) : onMiss();
    }
    
    /// <summary>
    /// Executes the appropriate action based on cache hit/miss
    /// </summary>
    public async Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onHit,
        Func<Task<TResult>> onMiss)
    {
        return IsHit ? await onHit(_value!) : await onMiss();
    }
}