# Code Quality Standards - GetFitterGetBigger

**🎯 PURPOSE**: Mandatory code quality standards that MUST be followed for ALL implementations. These standards were derived from the CacheService refactoring and represent our textbook implementation approach.

## 🚨 MANDATORY: Read This Before Starting ANY Implementation

This document defines the non-negotiable code quality standards for all classes, methods, and implementations in the GetFitterGetBigger ecosystem.

---

## 📋 Core Principles

### 1. **Pattern Matching Over If-Else**
Always prefer pattern matching when dealing with conditional logic:

```csharp
// ❌ BAD - Traditional if-else
if (result.IsHit)
    return result.Value;
else
    return T.Empty;

// ✅ GOOD - Pattern matching
return result switch
{
    { IsHit: true } => result.Value,
    _ => T.Empty
};
```

### 2. **Empty/Null Object Pattern**
**NEVER** return or handle nulls directly. Always use the Empty pattern:

```csharp
// ❌ BAD - Returning null
if (entity == null)
    return null;

// ✅ GOOD - Returning Empty
if (entity.IsEmpty)
    return EntityDto.Empty;
```

### 3. **No Defensive Programming Without Justification**
Trust the framework and validate at appropriate levels:

```csharp
// ❌ BAD - Over-defensive
public void SetValue(string key, object value)
{
    if (key == null) throw new ArgumentNullException(nameof(key));
    if (value == null) throw new ArgumentNullException(nameof(value));
    if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be empty");
    // ... actual logic
}

// ✅ GOOD - Trust the framework
public void SetValue(string key, object value)
{
    _cache.Set(key, value); // Let MemoryCache handle null validation
}
```

### 4. **Method Length and Readability**
- Methods should be **short and focused** (ideally < 20 lines)
- Extract complex logic into helper methods
- Use descriptive names that explain the "what" not the "how"

---

## 🛠️ Implementation Standards

### 1. **Async/Await Guidelines**

#### No Fake Async
```csharp
// ❌ BAD - Fake async
public async Task DoSomething()
{
    DoWork();
    await Task.CompletedTask;
}

// ✅ GOOD - Return Task directly
public Task DoSomething()
{
    DoWork();
    return Task.CompletedTask;
}
```

#### Document Synchronous Task Returns
```csharp
/// <remarks>
/// This method returns a Task for interface compatibility, but the operation is synchronous
/// since we're using in-memory cache (IMemoryCache). If we migrate to a distributed cache
/// (Redis, SQL, etc.) in the future, the async signature will be necessary.
/// </remarks>
public Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
```

### 2. **Exception Handling**

#### Only Catch What You Can Handle
```csharp
// ❌ BAD - Catching everything
try
{
    return _cache.Get<T>(key);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error getting from cache");
    return null;
}

// ✅ GOOD - Let it fail naturally
return _cache.Get<T>(key);
```

#### Resource Access Requires Try-Catch
Only use try-catch for operations that access external resources:
- File System operations
- Network/HTTP calls
- Database operations
- NOT for in-memory operations

### 3. **Null Handling During Migration**

#### Mark Nullable Methods as Obsolete
```csharp
[Obsolete("This method returns null and will be removed after Empty pattern migration. " +
          "Use GetAsync<T>(key) for CacheResult<T>, or GetOrEmptyAsync<T>(key) for types implementing IEmptyEntity<T>.")]
Task<T?> GetAsync<T>(string key) where T : class;
```

#### Create Parallel Interfaces for Migration
```csharp
// Temporary interface for gradual migration
public interface IEmptyEnabledCacheService : ICacheService
{
    new Task<CacheResult<T>> GetAsync<T>(string key) where T : class;
    Task<T> GetOrEmptyAsync<T>(string key) where T : class, IEmptyEntity<T>;
}
```

---

## 📊 Code Review Checklist

Before submitting ANY code, verify:

### ✅ Pattern Matching
- [ ] All conditional returns use switch expressions where applicable
- [ ] No if-else chains that could be pattern matches
- [ ] Tuple deconstruction used where appropriate

### ✅ Empty Pattern
- [ ] No methods return null (except legacy/obsolete)
- [ ] All entities implement IEmptyEntity<T>
- [ ] IsEmpty checks instead of null checks

### ✅ Method Quality
- [ ] Methods are < 20 lines
- [ ] Single responsibility per method
- [ ] Clear, descriptive names
- [ ] No fake async

### ✅ Error Handling
- [ ] No unnecessary try-catch blocks
- [ ] Only catch exceptions for external resources
- [ ] Let framework handle validation

### ✅ Documentation
- [ ] XML comments on all public methods
- [ ] Remarks for non-obvious design decisions
- [ ] Obsolete attributes with migration guidance

---

## 🚀 Migration Strategy

When refactoring existing code:

1. **Create Parallel Structure**
   - Don't break existing code
   - Add new Empty-aware methods alongside old ones

2. **Mark Old Methods Obsolete**
   - Clear migration messages
   - Point to new methods

3. **Update Incrementally**
   - Fix warnings as you work on each area
   - Don't do massive refactors

4. **Document the Journey**
   - Update EMPTY-PATTERN-MIGRATION-GUIDE.md
   - Add to LESSONS-LEARNED.md

---

## 📚 Reference Implementation

The `CacheService` class serves as our reference implementation. Study these methods:

```csharp
// Pattern matching example
public async Task<T> GetOrEmptyAsync<T>(string key) where T : class, IEmptyEntity<T>
{
    var result = await GetAsync<T>(key);
    return result switch
    {
        { IsHit: true } => result.Value,
        _ => T.Empty
    };
}

// Clean, simple implementation
public Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
{
    var cacheEntryOptions = new MemoryCacheEntryOptions()
        .SetSlidingExpiration(expiration)
        .RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
        {
            _cacheKeys.TryRemove(evictedKey.ToString()!, out _);
        });

    _memoryCache.Set(key, value, cacheEntryOptions);
    _cacheKeys.TryAdd(key, true);
    
    return Task.CompletedTask;
}
```

---

## 🔗 Related Documents

- `EMPTY-PATTERN-MIGRATION-GUIDE.md` - How to migrate to Empty pattern
- `DEVELOPMENT_PROCESS.md` - Overall development workflow
- `TESTING-QUICK-REFERENCE.md` - Testing patterns and standards

---

## 💡 Remember

> "The best code is code that doesn't need to exist. The second best is code that's so clear it doesn't need comments."

Follow these standards to create maintainable, readable, and robust code that serves as an example for others.