# FEAT-030: Exercise Link Enhancements - Four-Way Linking System Implementation Tasks

## Feature Branch: `feature/exercise-link-enhancements`
## Estimated Total Time: 12h

## 📚 Pre-Implementation Checklist
- [ ] Read `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md` - Critical implementation mistakes to avoid
- [ ] Read `/memory-bank/PracticalGuides/ServiceImplementationChecklist.md` - Quick reference during coding
- [ ] Read `/memory-bank/CodeQualityGuidelines/ServiceResultPattern.md` - Error handling patterns
- [ ] Read `/memory-bank/PracticalGuides/TestingQuickReference.md` - Test implementation guidance
- [ ] Review existing ExerciseLink implementation in `/GetFitterGetBigger.API/Services/Exercise/Features/Links/`
- [ ] Run baseline health check (`dotnet build` and `dotnet test`)

## Baseline Health Check Report
**Date/Time**: 2025-08-20 (Updated after system issue resolution)
**Branch**: feature/exercise-link-enhancements

### Build Status
- **Build Result**: SUCCESS
- **Warning Count**: 0
- **Warning Details**: None

### Test Status
- **Total Tests**: 400
- **Passed**: 400
- **Failed**: 0 (System issue resolved - all tests now passing)
- **Skipped/Ignored**: 0
- **Test Execution Time**: ~45 seconds

### Decision to Proceed
- [x] All tests passing
- [x] Build successful  
- [x] No warnings

**Approval to Proceed**: APPROVED - Ready for implementation

## Test Structure

### Global Acceptance Tests
**Location**: `/memory-bank/features/1-READY_TO_DEVELOP/FEAT-030-exercise-link-enhancements/acceptance-tests/`

#### End-to-End Workflow Tests
- **Given** PT has access to Exercise Link Management in Admin interface
- **When** PT creates bidirectional exercise links (WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE)
- **Then** Both API and Clients can retrieve and display the complete link relationships
- **And** REST exercise linking restrictions are enforced across all interfaces

#### Cross-Project Integration Tests
- **Given** API has established four-way linking system
- **When** Clients request exercise suggestions based on link types
- **Then** All interfaces show consistent bidirectional relationships and suggestions

### Project-Specific Acceptance Tests
**Location**: `/Tests/Features/ExerciseLinks/ExerciseLinkEnhancementsAcceptanceTests.cs`

#### Bidirectional Link Creation Scenarios
- **Given** an existing WORKOUT type exercise "Burpee"
- **When** I link "Jumping-Jacks" as WARMUP to "Burpee"
- **Then** the system automatically creates reverse WORKOUT link from "Burpee" to "Jumping-Jacks"
- **And** both directions can be queried independently

#### Four-Way Link Type Scenarios
- **Given** exercises of different types (WORKOUT, WARMUP, COOLDOWN)
- **When** I create links between compatible types
- **Then** all four link types (WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE) function correctly
- **And** link type compatibility matrix is enforced

#### REST Exercise Constraint Scenarios
- **Given** a REST type exercise
- **When** I attempt to create any type of link to/from the REST exercise
- **Then** the system rejects the operation with appropriate error message
- **And** no links are created in either direction

## Phase 1: Planning & Analysis (Est: 1h)

### Task 1.1: Study codebase for similar implementations and patterns
`[Pending]` (Est: 45m)

**Objective:**
- Search for similar entities/services/controllers in the codebase
- Identify patterns to follow and code that can be reused
- Document findings with specific file references
- Note any lessons learned from completed features

**Implementation Steps:**
- Use Grep/Glob tools to find similar implementations
- Analyze existing patterns in Services/, Controllers/, and Models/
- Review `/memory-bank/features/3-COMPLETED/FEAT-022-exercise-linking/` for existing patterns
- Document reusable code patterns with file paths

**Critical Patterns Found:**
- **ExerciseLink Entity**: `/Models/Entities/ExerciseLink.cs` - Current string-based LinkType
- **ExerciseLinkService**: `/Services/Exercise/Features/Links/ExerciseLinkService.cs` - Current validation patterns
- **ServiceResult Pattern**: Must return `ServiceResult<T>` from all service methods
- **UnitOfWork Pattern**: ReadOnly for validation, Writable for modifications ONLY
- **Entity Handler Pattern**: Use Handler.CreateNew() and Handler.Create() methods
- **Empty Pattern**: All entities implement IEmptyEntity<T> with Empty property

**Deliverables:**
- List of similar implementations with file paths
- Patterns to follow (ServiceResult, Empty pattern, etc.)
- Code examples that can be adapted
- Critical warnings from lessons learned

### Task 1.2: Create Test Builders for TDD approach
`[Pending]` (Est: 15m)

**Objective:**
- Create test builders for all new DTOs and scenarios
- Enable TDD development approach
- Ensure consistent test data creation

**Implementation Steps:**
- Create `ExerciseLinkTestBuilder` in test infrastructure
- Add methods for each link type scenario
- Include bidirectional link test scenarios
- Reference existing test builders in `/Tests/TestInfrastructure/`

**Test Builder Methods:**
- `WithLinkType(ExerciseLinkType linkType)`
- `WithBidirectionalLink()`
- `WithRestExerciseValidation()`
- `WithCircularReferenceValidation()`

## CHECKPOINT: Planning & Analysis
`[Status]` - Date: YYYY-MM-DD

**Requirements for GREEN status:**
- Build Report: 0 errors, 0 warnings
- Test Report: All passed, 0 failed
- Code Review: APPROVED status (see `/memory-bank/DevelopmentGuidelines/UnifiedDevelopmentProcess.md`)

**Verification Steps:**
1. Run `dotnet build` - must show 0 errors, 0 warnings
2. Run `dotnet test` - all tests must pass
3. Create code review file following naming convention
4. Ensure code review status is APPROVED before proceeding

## Phase 2: Models & Database (Est: 2h)

### Task 2.1: Create ExerciseLinkType enum to replace string LinkType
`[Pending]` (Est: 30m)

**Implementation Steps:**
1. Create new file `/Models/Enums/ExerciseLinkType.cs`
2. Define enum with explicit integer values for EF Core mapping
3. Add XML documentation for each enum value

**Exact Implementation:**
```csharp
// File: /Models/Enums/ExerciseLinkType.cs
namespace GetFitterGetBigger.API.Models.Enums;

/// <summary>
/// Defines the types of relationships between exercises
/// </summary>
public enum ExerciseLinkType
{
    /// <summary>
    /// Warmup exercise to prepare for a workout
    /// </summary>
    WARMUP = 0,
    
    /// <summary>
    /// Cooldown exercise for recovery after a workout
    /// </summary>
    COOLDOWN = 1,
    
    /// <summary>
    /// Main workout exercise
    /// </summary>
    WORKOUT = 2,
    
    /// <summary>
    /// Alternative exercise that can replace another
    /// </summary>
    ALTERNATIVE = 3
}
```

**Unit Tests (included):**
- Test enum values are correctly defined (0, 1, 2, 3)
- Test enum parsing from strings
- Validate all four types are present

**Reference**: Follow pattern from existing enums in `/Models/Enums/`

### Task 2.2: Update ExerciseLink entity to use enum and add bidirectional support
`[Pending]` (Est: 45m)

**Implementation (30m):**
1. Update LinkType property from `string` to `ExerciseLinkType` enum
2. Update Handler.CreateNew() validation for enum values
3. Update Handler.Create() method to accept enum
4. Update Empty pattern to use enum default

**Exact Changes to `/Models/Entities/ExerciseLink.cs`:**
```csharp
// Add using statement
using GetFitterGetBigger.API.Models.Enums;

public record ExerciseLink : IEmptyEntity<ExerciseLink>
{
    // Change property type
    public ExerciseLinkType LinkType { get; init; } // Was: string
    
    // Update Empty pattern
    public static ExerciseLink Empty => new()
    {
        // ... other properties ...
        LinkType = ExerciseLinkType.WARMUP, // Was: string.Empty
        // ... other properties ...
    };
    
    // Update Handler.CreateNew
    public static ExerciseLink CreateNew(
        ExerciseId sourceExerciseId,
        ExerciseId targetExerciseId,
        ExerciseLinkType linkType, // Was: string
        int displayOrder)
    {
        // Remove string validation, enum is always valid
        // Remove: if (string.IsNullOrWhiteSpace(linkType))
        // Remove: if (linkType != "Warmup" && linkType != "Cooldown")
        
        // Rest of validation remains the same
        return new ExerciseLink
        {
            Id = ExerciseLinkId.New(),
            SourceExerciseId = sourceExerciseId,
            TargetExerciseId = targetExerciseId,
            LinkType = linkType, // Now enum
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
```

**Unit Tests (15m):**
- Test entity creation with all four enum values
- Test Handler validation with enum
- Test Empty pattern returns WARMUP as default

**CRITICAL WARNING:** Migration will handle data conversion automatically via EF Core

### Task 2.3: Create database migration for enum conversion
`[Pending]` (Est: 45m)

**Implementation Steps:**
1. Run EF Core migration command after entity changes
2. Review generated migration file
3. Add data conversion logic in Up() method
4. Add indexes for performance

**Migration Generation Command:**
```bash
dotnet ef migrations add UpdateExerciseLinksForFourWaySystem
```

**Manual Adjustments to Migration File:**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Add temporary column for new enum values
    migrationBuilder.AddColumn<int>(
        name: "LinkTypeTemp",
        table: "ExerciseLinks",
        nullable: false,
        defaultValue: 0);
    
    // 2. Convert existing string data to enum integers
    migrationBuilder.Sql(@"
        UPDATE ""ExerciseLinks"" 
        SET ""LinkTypeTemp"" = CASE 
            WHEN ""LinkType"" = 'Warmup' THEN 0
            WHEN ""LinkType"" = 'Cooldown' THEN 1
            ELSE 0
        END
    ");
    
    // 3. Drop old string column
    migrationBuilder.DropColumn(
        name: "LinkType",
        table: "ExerciseLinks");
    
    // 4. Rename temp column to LinkType
    migrationBuilder.RenameColumn(
        name: "LinkTypeTemp",
        table: "ExerciseLinks",
        newName: "LinkType");
    
    // 5. Add indexes for bidirectional queries
    migrationBuilder.CreateIndex(
        name: "IX_ExerciseLinks_TargetExerciseId_LinkType",
        table: "ExerciseLinks",
        columns: new[] { "TargetExerciseId", "LinkType" });
    
    migrationBuilder.CreateIndex(
        name: "IX_ExerciseLinks_SourceTarget",
        table: "ExerciseLinks",
        columns: new[] { "SourceExerciseId", "TargetExerciseId" });
}
```

**Important Notes:**
- **NO reverse links created during migration** - handled by application
- Data conversion only: "Warmup" → 0, "Cooldown" → 1
- Rollback via `dotnet ef migrations remove` if needed

**CRITICAL:** Review generated migration before applying!

## CHECKPOINT: Models & Database  
`[Status]` - Date: YYYY-MM-DD

Build Report: X errors, Y warnings
Test Report: A passed, B failed (Total: C)
Code Review: [filename] - [STATUS]

Notes: [Any relevant observations]

## Phase 3: Repository Layer (Est: 1h 30m)

### Task 3.1: Update IExerciseLinkRepository interface for bidirectional queries
`[Pending]` (Est: 30m)

**Implementation (20m):**
- Add methods for reverse link queries
- Update existing methods to support enum LinkType
- Reference: `/Repositories/Interfaces/IExerciseLinkRepository.cs`

**New Methods:**
```csharp
Task<IEnumerable<ExerciseLink>> GetReverseLinksByTargetExerciseAsync(ExerciseId targetId, ExerciseLinkType? linkType = null);
Task<bool> ExistsBidirectionalAsync(ExerciseId sourceId, ExerciseId targetId, ExerciseLinkType linkType);
```

**Unit Tests (10m):**
- Test interface contract changes
- Verify method signatures

### Task 3.2: Update ExerciseLinkRepository implementation for bidirectional support
`[Pending]` (Est: 1h)

**Implementation (45m):**
- Update all existing methods to use enum instead of string
- Implement new bidirectional query methods
- Add optimized queries for reverse link lookups
- Reference: `/Repositories/Implementations/ExerciseLinkRepository.cs`

**Integration Tests (15m):**
- Test bidirectional query methods
- Test enum filtering works correctly
- Test performance with larger datasets
- Reference: `/Tests/IntegrationTests/Repositories/`

**CRITICAL PATTERNS:**
- Use proper EF Core Include() for navigation properties
- Optimize queries to prevent N+1 problems
- Follow existing repository patterns in codebase

## CHECKPOINT: Repository Layer
`[Status]` - Date: YYYY-MM-DD

Build Report: X errors, Y warnings  
Test Report: A passed, B failed (Total: C)
Code Review: [filename] - [STATUS]

Notes: [Any relevant observations]

## Phase 4: Service Layer (Est: 4h)

### Task 4.1: Create enhanced validation service for link type compatibility
`[Pending]` (Est: 1h)

**Implementation (45m):**
- Create validation methods in ExerciseLinkService
- Implement compatibility matrix logic
- Add basic edge case validation (API protection)

**Validation Priority Order:**
1. REST exercise check (fastest, most common rejection)
2. Duplicate link check
3. Self-linking check (no ALTERNATIVE to self)
4. Compatibility matrix validation
5. Circular reference check (if needed)

**Exact Validation Implementation:**
```csharp
// In ExerciseLinkService.cs

private async Task<bool> IsLinkValidAsync(
    Exercise sourceExercise, 
    Exercise targetExercise, 
    ExerciseLinkType linkType)
{
    // 1. REST exercise check
    if (sourceExercise.ExerciseType == ExerciseType.REST || 
        targetExercise.ExerciseType == ExerciseType.REST)
        return false; // "REST exercises cannot have links"
    
    // 2. Self-linking ALTERNATIVE check
    if (sourceExercise.Id == targetExercise.Id && 
        linkType == ExerciseLinkType.ALTERNATIVE)
        return false; // "An exercise cannot be its own alternative"
    
    // 3. Compatibility matrix
    return IsCompatibleLinkType(
        sourceExercise.ExerciseType, 
        targetExercise.ExerciseType, 
        linkType);
}

private bool IsCompatibleLinkType(
    ExerciseType sourceType, 
    ExerciseType targetType, 
    ExerciseLinkType linkType)
{
    return linkType switch
    {
        ExerciseLinkType.WARMUP => targetType == ExerciseType.WORKOUT,
        ExerciseLinkType.COOLDOWN => targetType == ExerciseType.WORKOUT,
        ExerciseLinkType.WORKOUT => sourceType == ExerciseType.WARMUP || 
                                    sourceType == ExerciseType.COOLDOWN,
        ExerciseLinkType.ALTERNATIVE => true, // Any non-REST can be alternative
        _ => false
    };
}
```

**Error Messages (Constants):**
```csharp
public static class ExerciseLinkErrorMessages
{
    public const string RestExerciseLink = "REST exercises cannot have any links";
    public const string SelfAlternative = "An exercise cannot be its own alternative";
    public const string IncompatibleLinkType = "Invalid link type for these exercise types";
    public const string DuplicateLink = "This link already exists";
}
```

**Unit Tests (15m):**
- Test REST exercise rejection
- Test self-ALTERNATIVE rejection
- Test compatibility matrix
- Test duplicate detection

### Task 4.2: Update IExerciseLinkService interface for bidirectional operations
`[Pending]` (Est: 30m)

**Implementation (20m):**
- Add new methods for bidirectional operations
- Add method for getting suggested exercises
- Update existing method signatures to return ServiceResult<T>
- Reference: `/Services/Exercise/Features/Links/IExerciseLinkService.cs`

**New Methods:**
```csharp
// Bidirectional link creation
Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(
    ExerciseId sourceId, 
    CreateExerciseLinkDto dto);

// Get suggested exercises for linking
Task<ServiceResult<List<ExerciseDto>>> GetSuggestedExercisesAsync(
    ExerciseId exerciseId, 
    ExerciseLinkType linkType);

// Validation
Task<ServiceResult<bool>> ValidateLinkTypeCompatibilityAsync(
    ExerciseId sourceId, 
    ExerciseId targetId, 
    ExerciseLinkType linkType);

// Get links with bidirectional info
Task<ServiceResult<List<ExerciseLinkDto>>> GetBidirectionalLinksAsync(
    string exerciseId, 
    ExerciseLinkType? linkType = null);
```

**GetSuggestedExercisesAsync Logic:**
- Returns exercises that CAN be linked but AREN'T linked yet
- Excludes the source exercise itself
- Filters by compatible exercise types
- Example: For WARMUP suggestions, return all WARMUP-type exercises not already linked

**Unit Tests (10m):**
- Test updated interface contracts
- Verify ServiceResult<T> usage

### Task 4.3: Implement bidirectional link creation in ExerciseLinkService
`[Pending]` (Est: 1h 30m)

**Implementation (1h):**

**Step 1: Create Error Messages Constants**
```csharp
// File: /Constants/ExerciseLinkErrorMessages.cs
public static class ExerciseLinkErrorMessages
{
    public const string RestExerciseCannotHaveLinks = "REST exercises cannot have any links";
    public const string ExerciseCannotBeItsOwnAlternative = "An exercise cannot be its own alternative";
    public const string IncompatibleLinkTypes = "Invalid link type for these exercise types";
    public const string LinkAlreadyExists = "This link already exists";
    public const string SourceExerciseNotFound = "Source exercise not found";
    public const string TargetExerciseNotFound = "Target exercise not found";
}
```

**Step 2: Create ServiceValidate Extensions (local to service)**
```csharp
// In ExerciseLinkService.cs - local extension methods
private static class ServiceValidateExtensions
{
    public static ServiceValidate<T> EnsureNotRestExercise<T>(
        this ServiceValidate<T> validate,
        Exercise exercise,
        string exerciseName)
    {
        return validate.Ensure(
            () => exercise.ExerciseType != ExerciseType.REST,
            $"{exerciseName} is a REST exercise and cannot have links");
    }
    
    public static ServiceValidate<T> EnsureValidAlternativeLink<T>(
        this ServiceValidate<T> validate,
        ExerciseId sourceId,
        ExerciseId targetId,
        ExerciseLinkType linkType)
    {
        return validate.Ensure(
            () => linkType != ExerciseLinkType.ALTERNATIVE || sourceId != targetId,
            ExerciseLinkErrorMessages.ExerciseCannotBeItsOwnAlternative);
    }
    
    public static async Task<ServiceValidate<T>> EnsureNoDuplicateLinkAsync<T>(
        this ServiceValidate<T> validate,
        Func<Task<bool>> checkDuplicate)
    {
        return await validate.EnsureAsync(
            async () => !await checkDuplicate(),
            ExerciseLinkErrorMessages.LinkAlreadyExists);
    }
    
    public static ServiceValidate<T> EnsureCompatibleLinkTypes<T>(
        this ServiceValidate<T> validate,
        ExerciseType sourceType,
        ExerciseType targetType,
        ExerciseLinkType linkType)
    {
        return validate.Ensure(
            () => IsCompatibleLinkType(sourceType, targetType, linkType),
            ExerciseLinkErrorMessages.IncompatibleLinkTypes);
    }
}
```

**Step 3: Implement CreateLinkAsync with Transaction**
```csharp
public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(
    ExerciseId sourceId, 
    CreateExerciseLinkDto dto)
{
    // Validation with ReadOnlyUnitOfWork
    using (var readUnitOfWork = _unitOfWorkProvider.CreateReadOnly())
    {
        var exerciseRepo = readUnitOfWork.GetRepository<IExerciseRepository>();
        var linkRepo = readUnitOfWork.GetRepository<IExerciseLinkRepository>();
        
        var sourceExercise = await exerciseRepo.GetByIdAsync(sourceId);
        var targetExercise = await exerciseRepo.GetByIdAsync(dto.TargetExerciseId);
        
        // ServiceValidate chain with positive assertions
        var validation = await ServiceValidate
            .Create()
            .EnsureNotNull(sourceExercise, ExerciseLinkErrorMessages.SourceExerciseNotFound)
            .EnsureNotNull(targetExercise, ExerciseLinkErrorMessages.TargetExerciseNotFound)
            .EnsureNotRestExercise(sourceExercise, "Source exercise")
            .EnsureNotRestExercise(targetExercise, "Target exercise")
            .EnsureValidAlternativeLink(sourceId, dto.TargetExerciseId, dto.LinkType)
            .EnsureNoDuplicateLinkAsync(
                async () => await linkRepo.ExistsAsync(sourceId, dto.TargetExerciseId, dto.LinkType))
            .EnsureCompatibleLinkTypes(
                sourceExercise.ExerciseType, 
                targetExercise.ExerciseType, 
                dto.LinkType)
            .ValidateAsync();
            
        if (!validation.IsValid)
            return ServiceResult<ExerciseLinkDto>.Failure(
                ExerciseLinkDto.Empty, 
                ServiceError.ValidationFailed(validation.ErrorMessage));
    }
    
    // Create both links in transaction
    using (var writeUnitOfWork = _unitOfWorkProvider.CreateWritable())
    {
        var linkRepo = writeUnitOfWork.GetRepository<IExerciseLinkRepository>();
        
        // Create primary link
        var primaryLink = ExerciseLink.Handler.CreateNew(
            sourceId,
            dto.TargetExerciseId,
            dto.LinkType,
            dto.DisplayOrder);
            
        // Determine reverse link type and create it
        var reverseType = GetReverseLinkType(dto.LinkType);
        var reverseDisplayOrder = await CalculateDisplayOrderAsync(
            linkRepo, dto.TargetExerciseId, reverseType);
            
        var reverseLink = ExerciseLink.Handler.CreateNew(
            dto.TargetExerciseId,
            sourceId,
            reverseType,
            reverseDisplayOrder);
        
        // Add both links
        await linkRepo.AddAsync(primaryLink);
        await linkRepo.AddAsync(reverseLink);
        
        // Commit transaction
        await writeUnitOfWork.CommitAsync();
        
        return ServiceResult<ExerciseLinkDto>.Success(primaryLink.ToDto());
    }
}

private ExerciseLinkType GetReverseLinkType(ExerciseLinkType linkType)
{
    return linkType switch
    {
        ExerciseLinkType.WARMUP => ExerciseLinkType.WORKOUT,
        ExerciseLinkType.COOLDOWN => ExerciseLinkType.WORKOUT,
        ExerciseLinkType.WORKOUT => ExerciseLinkType.WARMUP, // or COOLDOWN based on source
        ExerciseLinkType.ALTERNATIVE => ExerciseLinkType.ALTERNATIVE,
        _ => throw new InvalidOperationException($"Unknown link type: {linkType}")
    };
}

private async Task<int> CalculateDisplayOrderAsync(
    IExerciseLinkRepository repo,
    ExerciseId exerciseId,
    ExerciseLinkType linkType)
{
    var existingLinks = await repo.GetBySourceAndTypeAsync(exerciseId, linkType);
    return existingLinks.Count() + 1;
}
```

**Unit Tests (30m):**
- Test all validation scenarios
- Test transaction rollback on failure
- Test display order calculation
- Mock all dependencies

### Task 4.4: Update existing service methods for enhanced validation
`[Pending]` (Est: 1h)

**Implementation (45m):**
- Update GetLinksAsync to support new query options
- Update DeleteLinkAsync to handle bidirectional deletion
- Implement enhanced REST exercise validation
- Convert all methods to return ServiceResult<T>

**Enhanced Validation Rules:**
- No links to/from REST exercises
- Enforce compatibility matrix
- Validate self-linking is allowed for specific scenarios
- Check circular references including new link types

**Unit Tests (15m):**
- Test enhanced validation scenarios
- Test bidirectional deletion options
- Test REST exercise constraints

**CRITICAL PATTERN**: Every service method must return ServiceResult<T> - no exceptions!

## CHECKPOINT: Service Layer
`[Status]` - Date: YYYY-MM-DD

Build Report: X errors, Y warnings
Test Report: A passed, B failed (Total: C)  
Code Review: [filename] - [STATUS]

Notes: [Any relevant observations]

## Phase 5: API Controllers (Est: 2h)

### Task 5.1: Update CreateExerciseLinkDto for new link types
`[Pending]` (Est: 30m)

**Implementation (20m):**
- Update RegularExpression validation to include all four types
- Add XML documentation for new types
- Reference: `/DTOs/CreateExerciseLinkDto.cs`

**Updated Validation:**
```csharp
[RegularExpression("^(WARMUP|COOLDOWN|WORKOUT|ALTERNATIVE)$", ErrorMessage = "Link type must be WARMUP, COOLDOWN, WORKOUT, or ALTERNATIVE")]
```

**Unit Tests (10m):**
- Test DTO validation with all four types
- Test validation error messages

### Task 5.2: Update ExerciseLinksController for enhanced operations
`[Pending]` (Est: 1h 15m)

**Implementation (1h):**

**Step 1: Update CreateLink Endpoint**
```csharp
[HttpPost]
[Authorize(Policy = "PT-Tier")]
public async Task<IActionResult> CreateLink(
    [FromRoute] string exerciseId,
    [FromBody] CreateExerciseLinkDto dto)
{
    // Validate exerciseId format
    if (!ExerciseId.TryParse(exerciseId, out var sourceId))
        return BadRequest(new { error = "Invalid exercise ID format" });
    
    var result = await _linkService.CreateLinkAsync(sourceId, dto);
    
    return result.IsSuccess 
        ? Created($"/api/exercises/{exerciseId}/links/{result.Data.Id}", result.Data)
        : MapServiceErrorToActionResult(result.Error);
}

// Returns only the primary link (Option B)
// Response: 201 Created
// Body: { "id": "...", "sourceExerciseId": "...", "targetExerciseId": "...", "linkType": "WARMUP", ... }
```

**Step 2: Error Mapping Helper**
```csharp
private IActionResult MapServiceErrorToActionResult(ServiceError error)
{
    // Map specific error messages to HTTP status codes
    if (error.Message.Contains("REST exercises"))
        return BadRequest(new { error = error.Message }); // 400
        
    if (error.Message.Contains("Invalid link type") || 
        error.Message.Contains("Incompatible"))
        return BadRequest(new { error = error.Message }); // 400
        
    if (error.Message.Contains("already exists") || 
        error.Message.Contains("Duplicate"))
        return Conflict(new { error = error.Message }); // 409
        
    if (error.Message.Contains("not found"))
        return NotFound(new { error = error.Message }); // 404
        
    // Default to 400 for validation errors
    return BadRequest(new { error = error.Message });
}
```

**Step 3: Update Get Links Endpoint**
```csharp
[HttpGet]
public async Task<IActionResult> GetLinks(
    [FromRoute] string exerciseId,
    [FromQuery] ExerciseLinkType? linkType = null,
    [FromQuery] bool includeExerciseDetails = false,
    [FromQuery] bool includeReverse = false)
{
    if (!ExerciseId.TryParse(exerciseId, out var id))
        return BadRequest(new { error = "Invalid exercise ID format" });
    
    var result = await _linkService.GetLinksAsync(
        id, linkType, includeExerciseDetails, includeReverse);
    
    return result.IsSuccess 
        ? Ok(result.Data)
        : MapServiceErrorToActionResult(result.Error);
}
```

**Step 4: Update Delete Endpoint**
```csharp
[HttpDelete("{linkId}")]
[Authorize(Policy = "PT-Tier")]
public async Task<IActionResult> DeleteLink(
    [FromRoute] string exerciseId,
    [FromRoute] string linkId,
    [FromQuery] bool deleteReverse = true)
{
    if (!ExerciseLinkId.TryParse(linkId, out var parsedLinkId))
        return BadRequest(new { error = "Invalid link ID format" });
    
    var result = await _linkService.DeleteLinkAsync(parsedLinkId, deleteReverse);
    
    return result.IsSuccess 
        ? NoContent() // 204
        : MapServiceErrorToActionResult(result.Error);
}
```

**HTTP Status Code Mapping:**
- `400 Bad Request`: REST exercise links, incompatible link types, validation errors
- `404 Not Found`: Exercise or link not found
- `409 Conflict`: Duplicate link attempts
- `201 Created`: Successful link creation
- `204 No Content`: Successful deletion

**Unit Tests (15m):**
- Test each error scenario returns correct status code
- Test successful operations
- Mock service to return different ServiceResult scenarios

### Task 5.3: Update ExerciseLinkDto and response DTOs
`[Pending]` (Est: 15m)

**Implementation (10m):**
- Ensure DTO supports all four link types
- Add bidirectional link information if needed
- Reference: `/DTOs/ExerciseLinkDto.cs`

**Unit Tests (5m):**
- Test DTO serialization with new types

## CHECKPOINT: API Controllers
`[Status]` - Date: YYYY-MM-DD

Build Report: X errors, Y warnings
Test Report: A passed, B failed (Total: C)
Code Review: [filename] - [STATUS]

Notes: [Any relevant observations]

## Phase 6: Integration & Testing (Est: 1h 30m)

### Task 6.1: Create comprehensive BDD integration tests
`[Pending]` (Est: 1h)

**Implementation (45m):**
- Create `ExerciseLinkEnhancements.feature` file
- Use real-world exercise names for clarity
- Test all four link types with bidirectional behavior

**BDD Test Scenarios with Real Exercise Names:**

```gherkin
Feature: Exercise Link Enhancements - Four-Way Linking System

Background:
    Given the following exercises exist:
        | Name                | ExerciseType | Description                    |
        | Burpees            | WORKOUT      | Full body workout exercise     |
        | Jumping Jacks      | WARMUP       | Cardio warmup exercise        |
        | Mountain Climbers  | WARMUP       | Dynamic warmup exercise       |
        | Pigeon Pose        | COOLDOWN     | Hip stretch cooldown          |
        | Child's Pose       | COOLDOWN     | Recovery cooldown position    |
        | Push-ups           | WORKOUT      | Upper body workout            |
        | Wall Push-ups      | WORKOUT      | Modified push-up variation    |
        | Rest Period        | REST         | Recovery break                |

Scenario: Creating a warmup link automatically creates reverse workout link
    Given I am authenticated as a Personal Trainer
    When I link "Jumping Jacks" as WARMUP to "Burpees"
    Then "Jumping Jacks" should appear in the warmup list for "Burpees"
    And "Burpees" should appear in the workout list for "Jumping Jacks"
    And the display order for "Jumping Jacks" in "Burpees" warmups should be 1
    And the display order for "Burpees" in "Jumping Jacks" workouts should be 1

Scenario: Creating a cooldown link automatically creates reverse workout link
    Given I am authenticated as a Personal Trainer
    When I link "Pigeon Pose" as COOLDOWN to "Burpees"
    Then "Pigeon Pose" should appear in the cooldown list for "Burpees"
    And "Burpees" should appear in the workout list for "Pigeon Pose"

Scenario: Creating alternative exercise links
    Given I am authenticated as a Personal Trainer
    When I link "Wall Push-ups" as ALTERNATIVE to "Push-ups"
    Then "Wall Push-ups" should appear as an alternative for "Push-ups"
    And "Push-ups" should appear as an alternative for "Wall Push-ups"

Scenario: REST exercises cannot have any links
    Given I am authenticated as a Personal Trainer
    When I attempt to link "Jumping Jacks" as WARMUP to "Rest Period"
    Then the request should fail with status 400
    And the error message should be "REST exercises cannot have any links"

Scenario: Exercise cannot be its own alternative
    Given I am authenticated as a Personal Trainer
    When I attempt to link "Burpees" as ALTERNATIVE to "Burpees"
    Then the request should fail with status 400
    And the error message should be "An exercise cannot be its own alternative"

Scenario: Multiple warmup exercises with correct display order
    Given I am authenticated as a Personal Trainer
    And "Jumping Jacks" is already linked as WARMUP to "Burpees" with display order 1
    When I link "Mountain Climbers" as WARMUP to "Burpees" with display order 2
    Then both "Jumping Jacks" and "Mountain Climbers" should appear as warmups for "Burpees"
    And "Burpees" should appear in the workout list for "Mountain Climbers" with display order 1

Scenario: Duplicate link prevention
    Given I am authenticated as a Personal Trainer
    And "Jumping Jacks" is already linked as WARMUP to "Burpees"
    When I attempt to link "Jumping Jacks" as WARMUP to "Burpees" again
    Then the request should fail with status 409
    And the error message should be "This link already exists"

Scenario: Deleting a link with bidirectional cleanup
    Given I am authenticated as a Personal Trainer
    And "Jumping Jacks" is linked as WARMUP to "Burpees"
    When I delete the link between "Jumping Jacks" and "Burpees" with deleteReverse=true
    Then "Jumping Jacks" should not appear in the warmup list for "Burpees"
    And "Burpees" should not appear in the workout list for "Jumping Jacks"
```

**Test Data Builder Updates:**
```csharp
public class ExerciseLinkTestDataBuilder
{
    public static Exercise CreateBurpees() => 
        Exercise.Handler.CreateNew("Burpees", ExerciseType.WORKOUT, ...);
        
    public static Exercise CreateJumpingJacks() => 
        Exercise.Handler.CreateNew("Jumping Jacks", ExerciseType.WARMUP, ...);
        
    public static Exercise CreatePigeonPose() => 
        Exercise.Handler.CreateNew("Pigeon Pose", ExerciseType.COOLDOWN, ...);
        
    public static Exercise CreateRestPeriod() => 
        Exercise.Handler.CreateNew("Rest Period", ExerciseType.REST, ...);
}
```

**Unit Tests (15m):**
- Implement step definitions for each scenario
- Use real exercise names in assertions
- Clear, readable test output for non-technical stakeholders

### Task 6.2: Performance validation (Optional - Future Enhancement)
`[Deferred]` (Est: 30m)

**Note:** Performance testing deferred to future iteration. Current focus is on functional correctness.

**Future Considerations:**
- Validate link operations complete in < 100ms
- Check for N+1 query problems
- Monitor memory usage with large datasets

## CHECKPOINT: Integration & Testing
`[Status]` - Date: YYYY-MM-DD

Build Report: X errors, Y warnings
Test Report: A passed, B failed (Total: C)
Code Review: [filename] - [STATUS]

Notes: [Any relevant observations]

## Phase 7: Documentation & Deployment (Est: 30m)

### Task 7.1: Update API documentation
`[Pending]` (Est: 15m)

**Implementation:**
- Update Swagger documentation for enhanced endpoints
- Document new query parameters and response formats
- Add examples for bidirectional operations

### Task 7.2: Run full integration test suite
`[Pending]` (Est: 15m)

**Implementation:**
- Execute complete test suite
- Verify no regressions in existing functionality
- Confirm all new functionality works end-to-end

**Final Acceptance Criteria:**
- All tests pass (unit, integration, BDD)
- No build warnings
- Performance requirements met
- Bidirectional linking works correctly
- REST exercise constraints enforced
- Four link types fully functional

## BOY SCOUT RULE - Improvements Found During Implementation

Document any improvements, refactoring opportunities, or technical debt discovered during implementation:

### Improvements Made
- [ ] [To be filled during implementation]

### Future Opportunities  
- [ ] [To be filled during implementation]

### Technical Debt Identified
- [ ] [To be filled during implementation]

## Final Verification Checklist

- [ ] All four link types (WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE) are functional
- [ ] Bidirectional links are created/deleted automatically  
- [ ] REST exercises cannot have any links
- [ ] Existing links are migrated successfully
- [ ] All validation rules are enforced
- [ ] Performance is not degraded (< 100ms for link operations)
- [ ] 100% test coverage for new code
- [ ] ServiceResult<T> pattern used throughout
- [ ] No exceptions thrown for business logic
- [ ] ReadOnly/Writable UnitOfWork used correctly
- [ ] All tests pass (unit, integration, BDD)
- [ ] Build has no errors or warnings
- [ ] Code review completed and approved

## Dependencies Reference

### Existing Code to Study
- **Current ExerciseLink**: `/Models/Entities/ExerciseLink.cs`
- **Current Service**: `/Services/Exercise/Features/Links/ExerciseLinkService.cs`
- **Current Repository**: `/Repositories/Implementations/ExerciseLinkRepository.cs`
- **Current Controller**: `/Controllers/ExerciseLinksController.cs`
- **Current Tests**: `/Tests/Features/Exercise/ExerciseLinks.feature`

### Critical Guidelines  
- **Service Patterns**: `/memory-bank/CodeQualityGuidelines/ServiceResultPattern.md`
- **Common Pitfalls**: `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md`
- **Testing Guide**: `/memory-bank/PracticalGuides/TestingQuickReference.md`
- **Implementation Checklist**: `/memory-bank/PracticalGuides/ServiceImplementationChecklist.md`

### Similar Features for Reference
- **FEAT-022**: `/memory-bank/features/3-COMPLETED/FEAT-022-exercise-linking/` - Original implementation
- **FEAT-026**: `/memory-bank/features/3-COMPLETED/FEAT-026-workout-template-core/` - Complex entity relationships

**REMEMBER**: Always check Common Implementation Pitfalls before implementing service methods!