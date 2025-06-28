# Testing Quick Reference - MUST READ

**IMPORTANT**: This document contains critical patterns learned from fixing 87 test failures. Future Claude instances should reference this when debugging test issues.

## 🚨 Most Common Test Failures (Check These First!)

### 1. Wrong ID Format in Tests
```csharp
// ❌ NEVER DO THIS
"exercisetype-rest-11111111-1111-1111-1111-111111111111"  // WRONG!

// ✅ ALWAYS USE THIS FORMAT
"exercisetype-11111111-1111-1111-1111-111111111111"       // CORRECT!
```
**Pattern**: `{entitytype}-{guid}` (NO additional descriptors)

### 2. Missing Mock Setup in Unit Tests
```csharp
// If test expects data but gets null/empty, YOU FORGOT TO MOCK!
_exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(typeId))
    .ReturnsAsync(exerciseType);  // <-- Don't forget this!
```

### 3. Navigation Properties Not Loaded
```csharp
// After insert, navigation properties are NULL by default
// You MUST explicitly load them:
await Context.Entry(entity).Reference(e => e.NavigationProperty).LoadAsync();
```

## 🔍 Quick Debugging Checklist

When tests fail, check IN THIS ORDER:
1. **ID Format** - Is it `{type}-{guid}` exactly?
2. **Mock Setup** - Did you mock ALL repository calls?
3. **Test Type** - Unit test (needs mocks) or Integration test (needs seed data)?
4. **Navigation Properties** - Are they loaded after insert/update?

## 🎯 Key Patterns to Remember

### Repository Mock Pattern
```csharp
_repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<TId>()))
    .ReturnsAsync(entity);
```

### Navigation Loading Pattern
```csharp
// For single navigation property
await Context.Entry(entity).Reference(e => e.Property).LoadAsync();

// For collection navigation property
await Context.Entry(entity).Collection(e => e.Collection).LoadAsync();
```

## ⚠️ Common Gotchas

1. **SharedDatabaseTestFixture** = Integration test (real seeded data)
2. **Mock<IRepository>** = Unit test (only mocked data exists)
3. **EF Core In-Memory** = Some features don't work (see skipped tests)
4. **ID Parsing** = Will fail silently with wrong format

## 📊 Success Story
Using these patterns, we fixed:
- BUG-004: 81 tests (inotify limit)
- BUG-005: 6 tests (ID format + mock issues)
- Total: 87 tests fixed → 100% pass rate!

**Remember**: When tests fail, it's often the test setup, not the application code!