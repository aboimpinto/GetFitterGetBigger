# Service Implementation Checklist

This is a quick reference checklist to use **EVERY TIME** you implement or modify a service method.

## 🔍 Pre-Implementation Review
- [ ] Have I read `/memory-bank/common-implementation-pitfalls.md`?
- [ ] Do I understand the ReadOnly vs Writable UnitOfWork pattern?

## ✅ Implementation Checklist

### 1. Validation Queries
- [ ] Am I checking if related entities exist? → Use `ReadOnlyUnitOfWork`
- [ ] Am I validating foreign keys? → Use `ReadOnlyUnitOfWork`
- [ ] Am I loading data for display/validation only? → Use `ReadOnlyUnitOfWork`

### 2. Data Modifications
- [ ] Am I creating new entities? → Use `WritableUnitOfWork`
- [ ] Am I updating existing entities? → Use `WritableUnitOfWork`
- [ ] Am I deleting entities? → Use `WritableUnitOfWork`

### 3. Correct Pattern Template
```csharp
public async Task<ResultDto> UpdateSomethingAsync(string id, UpdateDto request)
{
    // STEP 1: ALL validation with ReadOnlyUnitOfWork
    using (var readOnlyUow = _unitOfWorkProvider.CreateReadOnly())
    {
        // Check related entities exist
        // Validate business rules
        // Load reference data
    }
    
    // STEP 2: Modifications with WritableUnitOfWork
    using var writableUow = _unitOfWorkProvider.CreateWritable();
    // Perform actual updates
    await writableUow.CommitAsync();
    
    // STEP 3: Invalidate cache if needed
    await InvalidateCacheAsync();
}
```

### 4. Post-Implementation Verification
- [ ] Did I use separate UnitOfWork instances for validation and modification?
- [ ] Did I dispose ReadOnlyUnitOfWork before starting WritableUnitOfWork?
- [ ] Did I call CommitAsync() on WritableUnitOfWork?
- [ ] Did I invalidate relevant caches after modifications?

## 🚨 Red Flags to Watch For
- Using the same UnitOfWork for both validation and updates
- Validating foreign keys inside a WritableUnitOfWork
- Loading reference entities (like BodyPart, Equipment) in WritableUnitOfWork
- Forgetting to invalidate cache after updates

## 💡 Quick Rule
**If you're not saving it, don't track it!** Use ReadOnlyUnitOfWork for everything except the actual save operation.