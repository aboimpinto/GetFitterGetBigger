# Layered Architecture Rules

**🎯 PURPOSE**: Define and enforce the mandatory architectural rules for proper separation of concerns in the GetFitterGetBigger API using a strict layered architecture.

## Architectural Rules (MANDATORY)

### Controller Layer Rules
1. **Controllers MUST NOT directly access repositories** - This is FORBIDDEN
2. **Controllers MUST NOT directly access UnitOfWork (ReadOnly or Writable)** - This is FORBIDDEN
3. **Controllers MUST ONLY communicate with Service layer**
4. **Controllers are responsible for:**
   - HTTP request/response handling
   - Input validation via attributes
   - Authorization checks
   - Calling appropriate service methods
   - Mapping service results to HTTP responses

### Service Layer Rules
1. **Single Repository Rule**: Each service MUST only access its own repository directly
   - Example: MuscleGroupService can only access IMuscleGroupRepository directly
   
2. **Service-to-Service Communication**: When a service needs data from another entity, it MUST call the corresponding service
   - Example: If MuscleGroupService needs to validate a BodyPart exists, it must call IBodyPartService.ExistsAsync()
   
3. **Transactional Pattern**: For operations across multiple repositories:
   - The same WritableUnitOfWork MUST be passed between services
   - Services accepting UnitOfWork parameters MUST use the provided instance
   - Example: AuthService passes its UnitOfWork to ClaimService.CreateUserClaimAsync()
   
4. **Read-Only Operations**: For read-only cross-service operations:
   - Services should NOT pass UnitOfWork between them
   - Each service creates its own ReadOnlyUnitOfWork as needed

5. **Services are responsible for:**
   - Business logic implementation
   - Transaction management via UnitOfWork
   - Calling multiple repositories within a single transaction
   - Data validation beyond basic input validation
   - Business rule enforcement
   - Create and manage UnitOfWork instances
   - Access repositories
   - Handle transactions (CommitAsync)
   - Manage caching

### Repository Layer Rules
1. **Repositories MUST be accessed through UnitOfWork**
2. **Repositories handle ONLY data access logic**
3. **No business logic in repositories**
4. **Repositories are responsible for:**
   - CRUD operations
   - Query operations
   - Data persistence logic

### Transaction Management
1. **UnitOfWork manages database transactions**
2. **Services MUST call CommitAsync() on Writable UnitOfWork**
3. **Multiple operations can be wrapped in a single UnitOfWork transaction**

## ⚠️ CRITICAL: ReadOnly vs Writable UnitOfWork Usage

**This is a MANDATORY architectural rule that MUST be followed:**

### Use ReadOnlyUnitOfWork for ALL validation and query operations
- Checking if related entities exist
- Validating data before updates
- Any operation that doesn't modify data

### Use WritableUnitOfWork ONLY for actual data modifications
- Creating new entities
- Updating existing entities
- Deleting entities

**Why this matters:**
- Using WritableUnitOfWork for queries causes Entity Framework to track entities
- Tracked entities can lead to unwanted database updates
- Example: Validating a BodyPart exists before updating a MuscleGroup can cause BOTH to be updated if using WritableUnitOfWork

### Correct Pattern
```csharp
public async Task<ResultDto> UpdateEntityAsync(string id, UpdateDto request)
{
    // STEP 1: Validation with ReadOnlyUnitOfWork
    using (var readOnlyUow = _unitOfWorkProvider.CreateReadOnly())
    {
        var validationRepo = readOnlyUow.GetRepository<IValidationRepository>();
        var relatedEntity = await validationRepo.GetByIdAsync(request.RelatedId);
        if (relatedEntity == null)
            throw new ArgumentException("Related entity not found");
    }
    
    // STEP 2: Update with WritableUnitOfWork
    using var writableUow = _unitOfWorkProvider.CreateWritable();
    var repository = writableUow.GetRepository<IMainRepository>();
    var entity = await repository.GetByIdAsync(id);
    var updated = Entity.Handler.Update(entity, request.NewValue);
    await repository.UpdateAsync(updated);
    await writableUow.CommitAsync();
    
    return MapToDto(updated);
}
```

## Service-to-Service Communication Patterns

### Validation Pattern
Services expose validation methods for cross-service validation:
```csharp
// MuscleGroupService needs to validate a BodyPart exists
public async Task<MuscleGroupDto> CreateAsync(CreateMuscleGroupRequest request)
{
    // Use IBodyPartService instead of IBodyPartRepository
    if (!await _bodyPartService.ExistsAsync(request.BodyPartId))
    {
        throw new InvalidOperationException($"Body part with ID {request.BodyPartId} not found");
    }
    
    // Continue with creation...
}
```

### Transactional Pattern
Services accept IWritableUnitOfWork parameters for participating in transactions:
```csharp
// AuthService creates user and claim in same transaction
public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    
    // Create user
    var user = new User { Id = UserId.New(), Email = request.Email };
    await userRepository.AddUserAsync(user);
    
    // Pass unitOfWork to ClaimService for transactional consistency
    await _claimService.CreateUserClaimAsync(user.Id, "Free-Tier", unitOfWork);
    
    await unitOfWork.CommitAsync();
}
```

## Pattern to Follow

Use the existing compliant controllers as examples:
- **AuthController** → uses IAuthService
- **ExercisesController** → uses IExerciseService

## Success Metrics
- 100% of controllers compliant with architectural rules
- All tests passing after refactoring
- No direct repository access in any controller
- No UnitOfWork usage in any controller
- Consistent service layer pattern across all controllers

## Long-term Benefits
1. **Maintainability**: Clear separation of concerns
2. **Testability**: Easier to mock services than repositories
3. **Flexibility**: Can change data access strategy without touching controllers
4. **Consistency**: All controllers follow the same pattern
5. **Transaction Management**: Centralized in service layer
6. **Business Logic**: Properly encapsulated in services

## Notes
- This refactoring does not change any external API contracts
- All existing endpoints remain the same
- Only the internal architecture is being improved
- This aligns with industry best practices for layered architecture