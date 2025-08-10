# Code Quality Standards - GetFitterGetBigger Ecosystem

**🎯 PURPOSE**: Universal code quality standards that MUST be followed across ALL GetFitterGetBigger projects (API, Admin, Clients). These are the non-negotiable quality standards for maintainable, readable, and robust code.

## 🚨 MANDATORY: Read This Before Starting ANY Implementation

This document defines the core quality principles that apply regardless of technology stack or project type. Project-specific standards extend these universal principles.

---

## 🚨 GOLDEN RULES - NON-NEGOTIABLE

```
┌─────────────────────────────────────────────────────────────────┐
│ 🔴 CRITICAL: These rules MUST be followed - NO EXCEPTIONS      │
├─────────────────────────────────────────────────────────────────┤
│ 1. Single Exit Point per method - USE PATTERN MATCHING         │
│ 2. No null returns - USE EMPTY PATTERN                         │
│ 3. Methods < 20 lines (achieved by following rule #1)          │
│ 4. Cyclomatic complexity < 10 (pattern matching helps)         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📋 Universal Core Principles

### 1. **Pattern Matching Over If-Else** 🔑 KEY TO SINGLE EXIT POINT
Modern languages support pattern matching - use it for cleaner, more readable code AND to naturally enforce single exit points:

```csharp
// ❌ BAD - Traditional if-else chains
if (result.IsSuccess)
    return result.Value;
else if (result.IsNotFound)
    return NotFound();
else
    return BadRequest(result.Error);

// ✅ GOOD - Pattern matching (C# example)
return result switch
{
    { IsSuccess: true } => result.Value,
    { IsNotFound: true } => NotFound(),
    _ => BadRequest(result.Error)
};
```

### 2. **Null Safety First - Understanding the Null Object Pattern**
Minimize null usage through language features and patterns:
- Use nullable reference types where available
- Prefer Option/Maybe types or **Empty Object Pattern**
- Validate at boundaries, not throughout the code
- Document when null is intentionally allowed

⚠️ **CRITICAL**: When using the Null Object Pattern, understand that **Empty IS valid** and doesn't need constant validation:

```typescript
// ❌ BAD - Null checks everywhere
function processUser(user: User | null) {
    if (user === null) return;
    if (user.profile === null) return;
    if (user.profile.settings === null) return;
    // actual logic
}

// ❌ BAD - Over-validating with Null Object Pattern
function processUser(user: User) {
    if (user.isEmpty || !user.isActive) return; // Mixing concerns!
    // Defeats the purpose of Null Object Pattern
}

// ✅ GOOD - Trust the Null Object Pattern
function processUser(user: User) {
    // User is never null - might be User.Empty which is valid
    // Check business logic (isActive) separately from null safety
    if (!user.isActive) return UserNotActive;
    // Process user - Empty users will behave correctly
}
```

**Key Principle**: The Null Object Pattern eliminates null checks. Don't add `isEmpty` checks everywhere - that defeats the pattern's purpose. See [NULL_OBJECT_PATTERN_GUIDELINES.md](./GetFitterGetBigger.API/memory-bank/NULL_OBJECT_PATTERN_GUIDELINES.md) for detailed guidelines.

### 3. **Method Length and Complexity**
- **Target**: Methods < 20 lines
- **Maximum cyclomatic complexity**: 10
- Extract complex logic into well-named helper methods
- One level of abstraction per method

### 4. **Single Exit Point Principle** 🚨 CRITICAL - USE PATTERN MATCHING
**🔴 NON-NEGOTIABLE**: Every method MUST have ONE exit point at the end. Pattern matching is your PRIMARY TOOL to achieve this:

```javascript
// ❌ BAD - Multiple returns scattered throughout
function calculateDiscount(customer, amount) {
    if (!customer.isActive) return 0;
    
    if (customer.tier === 'gold') {
        if (amount > 100) return amount * 0.2;
        return amount * 0.1;
    }
    
    if (customer.tier === 'silver') return amount * 0.05;
    
    return 0;
}

// ✅ GOOD - Single exit point
function calculateDiscount(customer, amount) {
    let discount = 0;
    
    if (customer.isActive) {
        discount = customer.tier === 'gold' 
            ? (amount > 100 ? amount * 0.2 : amount * 0.1)
            : customer.tier === 'silver' 
                ? amount * 0.05 
                : 0;
    }
    
    return discount;
}

// ✅ BETTER - Pattern matching with single exit (C# example)
public ServiceResult<DiscountDto> CalculateDiscount(Customer customer, decimal amount) =>
    customer switch {
        { IsActive: false } => ServiceResult<DiscountDto>.Success(new DiscountDto { Amount = 0 }),
        { Tier: "gold" } => ServiceResult<DiscountDto>.Success(new DiscountDto { 
            Amount = amount > 100 ? amount * 0.2m : amount * 0.1m 
        }),
        { Tier: "silver" } => ServiceResult<DiscountDto>.Success(new DiscountDto { Amount = amount * 0.05m }),
        _ => ServiceResult<DiscountDto>.Success(new DiscountDto { Amount = 0 })
    };
```

**🔑 KEY INSIGHT**: Pattern matching naturally enforces single exit point because:
- The entire expression evaluates to ONE value
- No scattered returns throughout the method
- Cleaner, more readable code
- Lower cyclomatic complexity

### 5. **Defensive Programming Balance**
- Validate at system boundaries (API inputs, user inputs)
- Trust internal components and frameworks
- Don't duplicate validation at every layer
- Let infrastructure failures fail fast

---

## 🛠️ Universal Implementation Standards

### 1. **Async/Await Best Practices**

#### Avoid Fake Async
Don't create async methods that don't actually perform async operations:

```typescript
// ❌ BAD - Fake async
async function getName(): Promise<string> {
    return "John"; // No actual async operation
}

// ✅ GOOD - Synchronous when appropriate
function getName(): string {
    return "John";
}
```

#### Document Sync-in-Async
When interface requires async but implementation is sync:

```typescript
/**
 * Returns user preference from in-memory cache.
 * @remarks Method is synchronous but returns Promise for interface compatibility.
 * Will become truly async if we switch to distributed cache.
 */
async getPreference(key: string): Promise<string> {
    return Promise.resolve(this.cache.get(key));
}
```

### 2. **Error Handling Philosophy**

#### Only Catch What You Can Handle
```javascript
// ❌ BAD - Catching and hiding errors
try {
    const data = await fetchUserData(userId);
    return processData(data);
} catch (error) {
    console.error('Error:', error);
    return null; // Hiding the actual problem
}

// ✅ GOOD - Let unhandleable errors propagate
const data = await fetchUserData(userId); // Let network errors bubble up
return processData(data);
```

#### Validate Preconditions
Check conditions before operations rather than catching failures:

```python
# ❌ BAD - Waiting for exception
try:
    user = users[user_id]
    user.update(data)
except KeyError:
    return Error("User not found")

# ✅ GOOD - Check precondition
if user_id not in users:
    return Error("User not found")

user = users[user_id]
user.update(data)
```

### 3. **Naming Conventions**

#### Universal Rules
- **Classes/Types**: PascalCase (`UserAccount`, `OrderService`)
- **Methods/Functions**: camelCase (`getUserById`, `calculateTotal`)
- **Constants**: UPPER_SNAKE_CASE (`MAX_RETRY_COUNT`, `DEFAULT_TIMEOUT`)
- **Private members**: Leading underscore (`_internalCache`, `_processQueue`)

#### Descriptive Names
- Name should convey intent, not implementation
- Avoid abbreviations except well-known ones
- Boolean names should be questions (`isActive`, `hasPermission`)
- Async methods should indicate async nature (`getUserAsync`, `loadData`)

---

## 🏗️ Architecture Standards

### 1. **Separation of Concerns**
- **Presentation**: UI logic only, no business rules
- **Business Logic**: Core domain logic, framework-agnostic
- **Data Access**: Database/API interactions isolated
- **Cross-Cutting**: Logging, caching, security in separate modules

### 2. **Dependency Direction**
- Dependencies flow inward (outer layers depend on inner)
- Business logic never depends on UI or infrastructure
- Use interfaces/abstractions at boundaries
- Inject dependencies, don't create them

### 3. **Component Size**
- **Single Responsibility**: Each class/module has one reason to change
- **High Cohesion**: Related functionality stays together
- **Low Coupling**: Minimize dependencies between components
- **File Length**: Prefer multiple small files over large ones

---

## 📊 Code Review Checklist

### ✅ Universal Checks

#### Code Quality
- [ ] Methods are focused and < 20 lines
- [ ] Single exit point per method (with rare justified exceptions)
- [ ] No code duplication (DRY principle)
- [ ] Clear, descriptive naming
- [ ] Appropriate comments for complex logic

#### Error Handling
- [ ] Only catching handleable exceptions
- [ ] Preconditions validated
- [ ] Errors properly propagated
- [ ] No empty catch blocks
- [ ] No suppressed errors

#### Architecture
- [ ] Follows project architecture patterns
- [ ] No cross-layer violations
- [ ] Dependencies injected properly
- [ ] Single responsibility maintained
- [ ] Proper abstraction levels

#### Testing
- [ ] New code has tests
- [ ] Tests are readable and maintainable
- [ ] Edge cases covered
- [ ] No magic values in tests
- [ ] Tests follow project conventions

#### Performance
- [ ] No obvious performance issues
- [ ] Appropriate algorithm complexity
- [ ] Resources properly disposed
- [ ] No memory leaks
- [ ] Async used appropriately

---

## 📚 Language-Agnostic Best Practices

### 1. **Immutability Where Possible**
- Prefer immutable data structures
- Use const/final/readonly appropriately
- Return new objects rather than modifying
- Document when mutation is intentional

### 2. **Composition Over Inheritance**
- Favor composition for code reuse
- Use inheritance for true "is-a" relationships
- Keep inheritance hierarchies shallow
- Consider interfaces/protocols over base classes

### 3. **Fail Fast**
- Validate inputs early
- Throw meaningful errors immediately
- Don't continue with invalid state
- Make illegal states unrepresentable

### 4. **Configuration Management**
- Externalize configuration
- Use environment-appropriate settings
- Validate configuration at startup
- Document all configuration options

---

## 🔗 Related Documents

- `CODE_REVIEW_PROCESS.md` - How to conduct code reviews
- `UNIFIED_DEVELOPMENT_PROCESS.md` - Overall development workflow
- Project-specific standards:
  - `API-CODE_QUALITY_STANDARDS.md` - API-specific standards
  - `ADMIN-CODE_QUALITY_STANDARDS.md` - Admin-specific standards
  - `CLIENTS-CODE_QUALITY_STANDARDS.md` - Clients-specific standards

---

## 💡 Remember

> "Any fool can write code that a computer can understand. Good programmers write code that humans can understand." - Martin Fowler

These standards exist to create maintainable, understandable code that serves as a positive example for the entire team.