# Code Review Template - Category Implementation

## Instructions for Use
1. Create a copy of this template for each category review
2. Save as: `Code-Review-Category-{X}-{YYYY-MM-DD}-{HH-MM}-{STATUS}.md`
3. Place in: `/2-IN_PROGRESS/FEAT-XXX/code-reviews/Category_{X}/`
4. Update STATUS in filename after review: APPROVED, APPROVED_WITH_NOTES, or REQUIRES_CHANGES

## Review Information
- **Feature**: FEAT-XXX - [Feature Name]
- **Category**: Category X - [Category Description]
- **Review Date**: YYYY-MM-DD HH:MM
- **Reviewer**: [Name/AI Assistant]
- **Commit Hash**: [Last commit hash reviewed]

## Review Objective
Perform a comprehensive code review of Category X implementation to ensure:
1. Adherence to CODE_QUALITY_STANDARDS.md
2. Consistency with established patterns
3. No technical debt accumulation
4. Ready for next category implementation

## Files Reviewed
List all files created or modified in this category:
```
- [ ] /path/to/file1.cs
- [ ] /path/to/file2.cs
- [ ] /path/to/test1.cs
```

## Critical Review Checklist

### 1. Architecture & Design Patterns ⚠️ CRITICAL
- [ ] **Layer Separation**: No cross-layer dependencies
- [ ] **Service Pattern**: All service methods return ServiceResult<T>
- [ ] **Repository Pattern**: Correct UnitOfWork usage (ReadOnly vs Writable)
- [ ] **Controller Pattern**: Clean pass-through, no business logic
- [ ] **DDD Compliance**: Domain logic in correct layer

**Issues Found**: [None/List issues with file:line references]

### 2. Empty/Null Object Pattern ⚠️ CRITICAL
- [ ] No methods return null (except legacy/obsolete)
- [ ] No null checks (use IsEmpty instead)
- [ ] No null propagation operators (?.) except in DTOs
- [ ] All entities have Empty static property
- [ ] Pattern matching for empty checks

**Issues Found**: [None/List issues with file:line references]

### 3. Exception Handling ⚠️ CRITICAL
- [ ] No exceptions thrown for control flow
- [ ] ServiceResult pattern used for errors
- [ ] Only try-catch for external resources
- [ ] Proper error codes (ServiceErrorCode enum)

**Issues Found**: [None/List issues with file:line references]

### 4. Pattern Matching & Modern C#
- [ ] Switch expressions used where applicable
- [ ] No if-else chains that could be pattern matches
- [ ] Target-typed new expressions
- [ ] Record types for DTOs where applicable

**Issues Found**: [None/List issues with file:line references]

### 5. Method Quality
- [ ] Methods < 20 lines
- [ ] Single responsibility per method
- [ ] No fake async
- [ ] Clear, descriptive names
- [ ] Cyclomatic complexity < 10

**Issues Found**: [None/List issues with file:line references]

### 6. Testing Standards
- [ ] Unit tests: Everything mocked
- [ ] Integration tests: BDD format only
- [ ] No magic strings (use constants/builders)
- [ ] Correct test project (Unit vs Integration)
- [ ] All new code has tests

**Issues Found**: [None/List issues with file:line references]

### 7. Performance & Security
- [ ] Caching implemented for reference data
- [ ] No blocking async calls (.Result, .Wait())
- [ ] Input validation at service layer
- [ ] No SQL injection risks
- [ ] Authorization checks in controllers

**Issues Found**: [None/List issues with file:line references]

### 8. Documentation & Code Quality
- [ ] XML comments on public methods
- [ ] No commented-out code
- [ ] Clear variable names
- [ ] Consistent formatting
- [ ] No TODOs left behind

**Issues Found**: [None/List issues with file:line references]

## Code Flow Verification

### Scenario Testing
Verify the implementation handles these scenarios correctly:

#### Scenario A: Happy Path
- [ ] Feature works as expected
- [ ] Correct HTTP status codes
- [ ] Proper response format

#### Scenario B: Invalid Input
- [ ] Validation errors returned properly
- [ ] 400 Bad Request status
- [ ] Clear error messages

#### Scenario C: Not Found
- [ ] 404 returned appropriately
- [ ] No exceptions thrown
- [ ] Empty pattern used correctly

## Specific Pattern Compliance

### If implementing reference data (Empty Pattern):
- [ ] Entity implements IEmptyEntity<T>
- [ ] ID type has ParseOrEmpty method
- [ ] Service extends appropriate base class
- [ ] Controller uses pattern matching for ServiceResult

### If implementing business logic:
- [ ] All business rules in service layer
- [ ] Proper validation before operations
- [ ] Transaction boundaries correct
- [ ] Audit trail if required

## Review Summary

### Critical Issues (Must Fix)
List any issues that MUST be fixed before proceeding:
1. [Issue description with file:line]
2. [Issue description with file:line]

### Minor Issues (Should Fix)
List any issues that should be fixed but don't block progress:
1. [Issue description with file:line]
2. [Issue description with file:line]

### Suggestions (Nice to Have)
List any improvements that could be made:
1. [Suggestion with rationale]
2. [Suggestion with rationale]

## Metrics
- **Files Reviewed**: X
- **Total Lines of Code**: X
- **Test Coverage**: X%
- **Build Warnings**: X (should be 0)
- **Code Duplication**: [None/Minimal/Concerning]

## Decision

### Review Status: [APPROVED / APPROVED_WITH_NOTES / REQUIRES_CHANGES]

### If APPROVED:
✅ All critical checks passed
✅ No blocking issues found
✅ Ready to proceed to next category

### If APPROVED_WITH_NOTES:
⚠️ Minor issues found but not blocking
⚠️ Document issues to fix in next iteration
⚠️ Can proceed with caution

### If REQUIRES_CHANGES:
❌ Critical issues found
❌ Must fix before proceeding
❌ New review required after fixes

## Action Items
1. [Specific action required]
2. [Specific action required]
3. [Specific action required]

## Next Steps
- [ ] Update task status in feature-tasks.md
- [ ] Fix any REQUIRES_CHANGES items
- [ ] Create new review if changes required
- [ ] Proceed to next category if APPROVED

---

**Review Completed**: YYYY-MM-DD HH:MM
**Next Review Due**: [If REQUIRES_CHANGES]