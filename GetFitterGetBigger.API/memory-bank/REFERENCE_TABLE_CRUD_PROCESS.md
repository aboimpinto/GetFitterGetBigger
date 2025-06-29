# Reference Table CRUD Conversion Process

## Overview

This document defines the standard process for converting read-only reference tables into fully CRUD-enabled entities. This process ensures consistency, maintains backward compatibility, and follows established patterns in the codebase.

## When to Use This Process

Use this process when:
- A reference table needs to support dynamic data management
- Personal Trainers or administrators need to add/modify reference data
- The data is no longer truly static (e.g., MuscleGroups, MovementPatterns, Equipment)

Do NOT use this process for:
- Truly immutable reference data (e.g., months of the year)
- System-critical enumerations that affect code logic

## Process Steps

### Phase 1: Planning & Analysis

#### 1.1 Identify the Reference Table
- **Table Name**: [e.g., MuscleGroups]
- **Entity Type**: [ReferenceDataBase-inherited or Simple entity]
- **Current Cache Duration**: [24 hours (static) or 1 hour (dynamic)]
- **Relationships**: [List any foreign keys or related entities]

#### 1.2 Define CRUD Requirements
- **Create**: Who can create? What validation is needed?
- **Update**: What fields are updatable? Any business rules?
- **Delete**: Soft delete or hard delete? Impact on related data?
- **Authorization**: Which claims/roles can perform each operation?

#### 1.3 Impact Analysis
- **Related Entities**: List all entities that reference this table
- **Caching Impact**: Current cache strategy and invalidation needs
- **Breaking Changes**: Any potential API breaking changes
- **Data Migration**: Existing data that needs modification

### Phase 2: Design Documentation

#### 2.1 Create Feature Description
Create a feature description file in `/memory-bank/features/1-READY_TO_DEVELOP/[table-name]-crud/feature-description.md`:

```markdown
# Feature: [Table Name] CRUD Operations

## Overview
Convert the [Table Name] reference table from read-only to full CRUD operations.

## Business Requirements
- [List specific business needs]
- [Who requested this change]
- [Expected usage patterns]

## Technical Requirements
- Maintain backward compatibility with existing GET endpoints
- Implement proper authorization (future claim: "ReferenceData-Management")
- Support soft delete to preserve data integrity
- Update caching strategy from [current] to immediate invalidation on mutations

## Acceptance Criteria
- [ ] All existing GET endpoints continue to work
- [ ] POST endpoint creates new records with validation
- [ ] PUT endpoint updates existing records
- [ ] DELETE endpoint soft-deletes records (IsActive = false)
- [ ] Cache invalidation works correctly
- [ ] All operations have proper authorization
- [ ] Unit tests achieve 90%+ coverage
- [ ] Integration tests verify end-to-end flows
- [ ] API documentation is updated
```

#### 2.2 Create Task Breakdown
Create a comprehensive task list in `/memory-bank/features/1-READY_TO_DEVELOP/[table-name]-crud/feature-tasks.md`:

```markdown
# [Table Name] CRUD Implementation Tasks

## 1. Entity & Database Updates
- [ ] Update entity model if needed (add audit fields, soft delete support)
- [ ] Create/update database migration
- [ ] Update DbContext configuration if needed

## 2. DTOs & Validation
- [ ] Create CreateDto with validation attributes
- [ ] Create UpdateDto with validation attributes
- [ ] Update existing ReadDto if needed
- [ ] Create validation rules for business logic

## 3. Repository Layer
- [ ] Add Create method to repository interface
- [ ] Add Update method to repository interface
- [ ] Add Delete/Deactivate method to repository interface
- [ ] Implement new methods in repository
- [ ] Add unit tests for repository methods

## 4. Controller Updates
- [ ] Add POST endpoint with proper routing
- [ ] Add PUT endpoint with proper routing
- [ ] Add DELETE endpoint with proper routing
- [ ] Update controller to use authorization (comment out until claim exists)
- [ ] Update cache invalidation for mutations
- [ ] Add unit tests for controller methods

## 5. Integration Tests
- [ ] Test GET endpoints still work (backward compatibility)
- [ ] Test POST endpoint with valid/invalid data
- [ ] Test PUT endpoint with valid/invalid data
- [ ] Test DELETE endpoint and soft delete behavior
- [ ] Test cache invalidation
- [ ] Test authorization when enabled

## 6. Documentation
- [ ] Update Swagger documentation
- [ ] Update API documentation in memory-bank
- [ ] Add examples of CRUD operations
- [ ] Document any breaking changes
```

### Phase 3: Implementation Pattern

#### 3.1 Entity Updates (if needed)

For entities NOT inheriting from ReferenceDataBase:
```csharp
public record MuscleGroup
{
    // Existing properties
    public MuscleGroupId Id { get; init; }
    public string Name { get; init; }
    
    // Add for CRUD support
    public bool IsActive { get; init; } = true;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    // Update Handler for soft delete support
    public static class Handler
    {
        public static MuscleGroup Create(string name, BodyPartId bodyPartId)
        {
            return new MuscleGroup
            {
                Id = MuscleGroupId.New(),
                Name = name,
                BodyPartId = bodyPartId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }
        
        public static MuscleGroup Update(MuscleGroup current, string name, BodyPartId bodyPartId)
        {
            return current with
            {
                Name = name,
                BodyPartId = bodyPartId,
                UpdatedAt = DateTime.UtcNow
            };
        }
        
        public static MuscleGroup Deactivate(MuscleGroup current)
        {
            return current with
            {
                IsActive = false,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
```

#### 3.2 DTO Structure

```csharp
// Create DTO
public class CreateMuscleGroupDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string BodyPartId { get; set; } = string.Empty;
}

// Update DTO
public class UpdateMuscleGroupDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string BodyPartId { get; set; } = string.Empty;
}

// Read DTO (existing, ensure it includes all needed fields)
public class MuscleGroupDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BodyPartId { get; set; } = string.Empty;
    public string BodyPartName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
```

#### 3.3 Repository Interface Updates

```csharp
public interface IMuscleGroupRepository : IReferenceDataRepository<MuscleGroup, MuscleGroupId>
{
    // Existing methods...
    
    // Add CRUD methods
    Task<MuscleGroup> CreateAsync(MuscleGroup entity);
    Task<MuscleGroup> UpdateAsync(MuscleGroup entity);
    Task<bool> DeactivateAsync(MuscleGroupId id);
    Task<bool> ExistsByNameAsync(string name, MuscleGroupId? excludeId = null);
}
```

#### 3.4 Controller Implementation

```csharp
[HttpPost]
[ProducesResponseType(typeof(MuscleGroupDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
// [Authorize(Policy = "ReferenceDataManagement")] // Uncomment when claim is implemented
public async Task<ActionResult<MuscleGroupDto>> Create(
    [FromBody] CreateMuscleGroupDto dto)
{
    // Validation
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    // Business rule validation
    var exists = await _repository.ExistsByNameAsync(dto.Name);
    if (exists)
        return BadRequest(new { error = "A muscle group with this name already exists" });
    
    // Create entity
    var bodyPartId = BodyPartId.From(dto.BodyPartId);
    var entity = MuscleGroup.Handler.Create(dto.Name, bodyPartId);
    
    // Save
    var created = await _repository.CreateAsync(entity);
    await _unitOfWork.SaveChangesAsync();
    
    // Invalidate cache
    await InvalidateTableCacheAsync();
    
    // Return result
    var result = MapToDto(created);
    return CreatedAtAction(
        nameof(GetById), 
        new { id = result.Id }, 
        result);
}

[HttpPut("{id}")]
[ProducesResponseType(typeof(MuscleGroupDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
// [Authorize(Policy = "ReferenceDataManagement")] // Uncomment when claim is implemented
public async Task<ActionResult<MuscleGroupDto>> Update(
    string id,
    [FromBody] UpdateMuscleGroupDto dto)
{
    // Similar pattern for update...
}

[HttpDelete("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
// [Authorize(Policy = "ReferenceDataManagement")] // Uncomment when claim is implemented
public async Task<IActionResult> Delete(string id)
{
    // Soft delete implementation...
}
```

### Phase 4: Testing Strategy

#### 4.1 Unit Tests Structure
```
Tests/
├── Controllers/
│   └── ReferenceTables/
│       └── MuscleGroupControllerTests/
│           ├── CreateTests.cs
│           ├── UpdateTests.cs
│           ├── DeleteTests.cs
│           └── GetTests.cs (ensure backward compatibility)
├── Repositories/
│   └── MuscleGroupRepositoryTests/
│       ├── CreateTests.cs
│       ├── UpdateTests.cs
│       ├── DeactivateTests.cs
│       └── QueryTests.cs
└── Entities/
    └── MuscleGroupTests/
        └── HandlerTests.cs
```

#### 4.2 Integration Tests
```
IntegrationTests/
└── ReferenceTables/
    └── MuscleGroupIntegrationTests.cs
        - Should_Create_MuscleGroup_With_Valid_Data
        - Should_Fail_Create_With_Duplicate_Name
        - Should_Update_Existing_MuscleGroup
        - Should_Soft_Delete_MuscleGroup
        - Should_Invalidate_Cache_On_Mutations
        - Should_Maintain_Backward_Compatibility
```

### Phase 5: Documentation Updates

#### 5.1 API Documentation Structure
```
/api-docs/
└── reference-tables/
    └── muscle-groups.md
        - Overview
        - GET endpoints (existing)
        - POST endpoint (new)
        - PUT endpoint (new)
        - DELETE endpoint (new)
        - Examples
        - Error responses
```

#### 5.2 Memory Bank Updates
- Update `systemPatterns.md` with CRUD pattern for reference tables
- Update `cache-invalidation-strategy.md` with specific table strategies
- Create migration guide if breaking changes exist

## Validation Checklist

Before marking the feature as complete:

- [ ] All unit tests pass with 90%+ coverage
- [ ] All integration tests pass
- [ ] No breaking changes to existing endpoints
- [ ] Cache invalidation verified in real scenarios
- [ ] API documentation updated and accurate
- [ ] Performance impact assessed (especially for frequently accessed tables)
- [ ] Error handling covers all edge cases
- [ ] Logging added for audit trail
- [ ] Code follows established patterns
- [ ] PR review completed

## Common Pitfalls to Avoid

1. **Breaking Existing Endpoints**: Always maintain backward compatibility
2. **Cache Invalidation Timing**: Invalidate AFTER successful database operation
3. **Circular Dependencies**: Watch for issues with related entities
4. **Hard Delete Impact**: Always prefer soft delete for reference data
5. **Missing Validation**: Validate both at DTO and business rule level
6. **Incomplete Tests**: Test edge cases, not just happy paths
7. **Authorization Gaps**: Plan for future authorization even if not implemented yet

## Example Timeline

For a typical reference table CRUD conversion:
- Planning & Design: 2-4 hours
- Implementation: 8-12 hours
- Testing: 4-6 hours
- Documentation: 2-3 hours
- Total: 2-3 days

## Related Documents

- `/memory-bank/FEATURE_IMPLEMENTATION_PROCESS.md`
- `/memory-bank/systemPatterns.md`
- `/memory-bank/cache-invalidation-strategy.md`
- `/memory-bank/TESTING-QUICK-REFERENCE.md`