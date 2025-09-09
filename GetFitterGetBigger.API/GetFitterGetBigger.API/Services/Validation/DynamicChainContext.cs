using System;
using System.Collections.Generic;

namespace GetFitterGetBigger.API.Services.Validation;

/// <summary>
/// Provides a type-safe context for storing and retrieving data throughout a ServiceValidate chain.
/// Manages repositories, data, and disposable resources with automatic cleanup.
/// </summary>
public class DynamicChainContext : IDisposable
{
    private readonly Dictionary<string, object> _storage = new();
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed;

    /// <summary>
    /// Stores a value with an explicit key.
    /// </summary>
    /// <typeparam name="T">The type of value to store</typeparam>
    /// <param name="key">The unique key to identify this value</param>
    /// <param name="value">The value to store</param>
    public void Store<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Storage key cannot be null or empty", nameof(key));

        _storage[key] = value!;

        // Track disposables for cleanup
        if (value is IDisposable disposable && !_disposables.Contains(disposable))
            _disposables.Add(disposable);
    }

    /// <summary>
    /// Retrieves a value by key with type safety.
    /// </summary>
    /// <typeparam name="T">The expected type of the value</typeparam>
    /// <param name="key">The key used to store the value</param>
    /// <returns>The stored value</returns>
    /// <exception cref="InvalidOperationException">If key not found or type mismatch</exception>
    public T Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Storage key cannot be null or empty", nameof(key));

        if (!_storage.TryGetValue(key, out var value))
            throw new InvalidOperationException(
                $"No item with key '{key}' found in context. This is a development error. " +
                $"Ensure the item was stored before attempting to retrieve it.");

        if (value is not T typedValue)
            throw new InvalidOperationException(
                $"Item with key '{key}' is of type {value?.GetType().Name ?? "null"}, " +
                $"not {typeof(T).Name}. This is a development error.");

        return typedValue;
    }

    /// <summary>
    /// Attempts to retrieve a value by key, returning success status.
    /// </summary>
    /// <typeparam name="T">The expected type of the value</typeparam>
    /// <param name="key">The key used to store the value</param>
    /// <param name="value">The retrieved value if successful</param>
    /// <returns>True if the value was found and is of the correct type</returns>
    public bool TryGet<T>(string key, out T? value)
    {
        value = default;

        if (string.IsNullOrWhiteSpace(key))
            return false;

        if (!_storage.TryGetValue(key, out var storedValue))
            return false;

        if (storedValue is not T typedValue)
            return false;

        value = typedValue;
        return true;
    }

    /// <summary>
    /// Checks if a key exists in the context.
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if the key exists</returns>
    public bool Contains(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return _storage.ContainsKey(key);
    }

    /// <summary>
    /// Stores a repository with automatic naming based on type and access mode.
    /// </summary>
    /// <typeparam name="T">The repository interface type</typeparam>
    /// <param name="repository">The repository instance</param>
    /// <param name="isReadOnly">Whether this is a read-only repository</param>
    public void StoreRepository<T>(T repository, bool isReadOnly) where T : class
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        var key = NamingEngine.GetRepositoryKey(typeof(T), isReadOnly);
        Store(key, repository);
    }

    /// <summary>
    /// Retrieves a repository by type and access mode.
    /// </summary>
    /// <typeparam name="T">The repository interface type</typeparam>
    /// <param name="isReadOnly">Whether to retrieve the read-only version</param>
    /// <returns>The repository instance</returns>
    /// <exception cref="InvalidOperationException">If repository not found</exception>
    public T GetRepository<T>(bool isReadOnly = true) where T : class
    {
        var key = NamingEngine.GetRepositoryKey(typeof(T), isReadOnly);
        return Get<T>(key);
    }

    /// <summary>
    /// Attempts to retrieve a repository by type and access mode.
    /// </summary>
    /// <typeparam name="T">The repository interface type</typeparam>
    /// <param name="isReadOnly">Whether to retrieve the read-only version</param>
    /// <param name="repository">The retrieved repository if successful</param>
    /// <returns>True if the repository was found</returns>
    public bool TryGetRepository<T>(bool isReadOnly, out T? repository) where T : class
    {
        repository = default;
        var key = NamingEngine.GetRepositoryKey(typeof(T), isReadOnly);
        return TryGet(key, out repository);
    }

    /// <summary>
    /// Removes an item from the context.
    /// </summary>
    /// <param name="key">The key of the item to remove</param>
    /// <returns>True if the item was removed</returns>
    public bool Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        if (_storage.TryGetValue(key, out var value))
        {
            _storage.Remove(key);

            // Remove from disposables if applicable
            if (value is IDisposable disposable)
                _disposables.Remove(disposable);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets all stored keys for debugging purposes.
    /// </summary>
    public IEnumerable<string> GetKeys() => _storage.Keys;

    /// <summary>
    /// Clears all stored items and disposes resources.
    /// </summary>
    public void Clear()
    {
        // Dispose all tracked disposables
        foreach (var disposable in _disposables)
        {
            try
            {
                disposable.Dispose();
            }
            catch
            {
                // Swallow disposal exceptions to ensure all items are attempted
            }
        }

        _disposables.Clear();
        _storage.Clear();
    }

    /// <summary>
    /// Disposes the context and all tracked resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        Clear();
        _disposed = true;
    }
}