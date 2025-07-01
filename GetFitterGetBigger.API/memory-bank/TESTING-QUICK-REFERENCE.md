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

## 🔄 Integration Test Types - CRITICAL KNOWLEDGE

### SharedDatabaseTestFixture vs PostgreSqlApiTestFixture

We have TWO types of integration test fixtures, each with specific use cases:

#### 1. SharedDatabaseTestFixture (In-Memory Database)
**When to use:**
- Testing CRUD operations where data needs to persist between HTTP requests
- Testing workflows that create data in one API call and use it in subsequent calls
- When you need fast test execution without database overhead
- When testing business logic that doesn't depend on database-specific features

**Characteristics:**
- Uses EF Core In-Memory database
- Single database instance shared across all HTTP requests in a test
- Data persists between API calls within the same test
- Fast execution
- Some EF Core features don't work (complex queries, transactions, etc.)

**Example:**
```csharp
[Collection("SharedDatabase")]
public class EquipmentCrudSimpleTests : IClassFixture<SharedDatabaseTestFixture>
{
    // Test creates equipment via POST, then retrieves it via GET
    // Data persists between these calls
}
```

#### 2. PostgreSqlApiTestFixture (Real PostgreSQL)
**When to use:**
- Testing database-specific features (migrations, complex queries, transactions)
- Testing concurrent operations
- When you need exact production database behavior
- Testing with pre-seeded data that gets reset before each test

**Characteristics:**
- Uses real PostgreSQL via TestContainers
- Inherits from PostgreSqlTestBase which calls CleanupDatabaseAsync() before each test
- Each test starts with a clean database + seeded reference data
- Slower but more accurate
- Supports all PostgreSQL features

**Example:**
```csharp
[Collection("PostgreSQL Integration Tests")]
public class ExercisesControllerPostgreSqlTests : PostgreSqlTestBase
{
    // Test uses pre-seeded data or creates all test data upfront
    // Database is cleaned before each test
}
```

### ⚡ Decision Guide: Which to Use?

**Use SharedDatabaseTestFixture when:**
- ✅ Testing Create → Read → Update → Delete workflows
- ✅ Data created via API needs to persist between calls
- ✅ Testing simple business logic
- ✅ You see "Skip: In-memory database limitation" - try this first!

**Use PostgreSqlApiTestFixture when:**
- ✅ Testing database migrations
- ✅ Testing complex queries or database-specific features
- ✅ Testing concurrent operations
- ✅ Need exact production database behavior
- ✅ Tests work with pre-seeded reference data only

### 🚨 Common Pitfall: "In-memory database limitation"

Before skipping a test with "In-memory database limitation":
1. **First try SharedDatabaseTestFixture** - it solves most data persistence issues
2. If that fails due to missing EF Core features, then use PostgreSqlApiTestFixture
3. Only skip if the test truly cannot work with either approach

## 📊 Success Story
Using these patterns, we fixed:
- BUG-004: 81 tests (inotify limit)
- BUG-005: 6 tests (ID format + mock issues)
- Total: 87 tests fixed → 100% pass rate!

**Remember**: When tests fail, it's often the test setup, not the application code!