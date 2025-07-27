# Code Review Standards - GetFitterGetBigger

**CRITICAL**: This document defines the non-negotiable quality standards for code reviews. ANY violation of these standards results in automatic NEEDS_CHANGES status.

## 🚨 MAJOR ISSUES - Automatic NEEDS_CHANGES

The following are considered **MAJOR ISSUES** and will result in immediate rejection:

### 1. Build Warnings
- **Standard**: ZERO build warnings allowed
- **Rationale**: Warnings often indicate potential bugs, deprecated usage, or poor practices
- **Action**: ALL warnings must be resolved before approval
- **Example**: xUnit1031, CS0618, or any other warning = NEEDS_CHANGES

### 2. Failing Tests
- **Standard**: 100% test pass rate required
- **Rationale**: Even "unrelated" failing tests can indicate environment issues or side effects
- **Action**: ALL tests must pass, regardless of perceived relation to changes
- **Example**: 1 failing test out of 1000 = NEEDS_CHANGES

### 3. Build Errors
- **Standard**: Build must succeed without errors
- **Rationale**: Code that doesn't compile cannot be merged
- **Action**: Fix all compilation errors
- **Example**: Any CS error = NEEDS_CHANGES

## 📋 Code Review Process

### Pre-Review Checklist
Before submitting for code review:
1. Run `dotnet clean && dotnet build` - Must show **0 warnings, 0 errors**
2. Run `dotnet test` - Must show **100% pass rate**
3. Check for any skipped/ignored tests - Document why if present
4. For Blazor list pages - Verify compliance with `UI_LIST_PAGE_DESIGN_STANDARDS.md`
5. Verify using statements are used instead of fully qualified type names per `CODE_QUALITY_STANDARDS.md`

### Review Outcomes

#### ✅ APPROVED
Can only be granted when:
- Zero build warnings
- Zero build errors
- All tests passing
- All review checklist items satisfactory
- No major architectural concerns

#### ❌ NEEDS_CHANGES
Must be set when:
- One or more build warnings exist
- One or more tests failing
- Build errors present
- Major issues found in review

#### 💬 NEEDS_DISCUSSION
For cases requiring team input:
- Architectural concerns
- Performance implications
- Security considerations

## 🔧 Common Warning Fixes

### xUnit1031 - Async/Await in Tests
**Problem**: Test methods using `.Wait()` or `.Result`
**Fix**: Convert test to async and use await
```csharp
// ❌ Bad
[Fact]
public void TestMethod()
{
    _service.LoadDataAsync().Wait();
}

// ✅ Good
[Fact]
public async Task TestMethod()
{
    await _service.LoadDataAsync();
}
```

### CS0618 - Obsolete Member Usage
**Problem**: Using deprecated APIs
**Fix**: Update to recommended alternative

### CS8602 - Possible Null Reference
**Problem**: Potential null reference exception
**Fix**: Add null checks or use null-conditional operators

### Fully Qualified Type Names
**Problem**: Using fully qualified type names instead of using statements
**Fix**: Add using statements and simplify type references
```csharp
// ❌ Bad
builder.Services.AddScoped<MyApp.Services.IMyService, MyApp.Services.MyService>();

// ✅ Good
using MyApp.Services;
builder.Services.AddScoped<IMyService, MyService>();
```

## 🎨 UI/UX Standards

### Blazor Component Reviews
When reviewing Blazor components, especially list pages:
1. **Layout Consistency**: Verify container spacing and structure per `UI_LIST_PAGE_DESIGN_STANDARDS.md`
2. **Navigation**: Ensure breadcrumb implementation
3. **Visual Design**: Check card styling, shadows, and hover effects
4. **Empty States**: Verify proper empty state design with icons and CTAs
5. **Responsive Design**: Test breakpoints and mobile layouts

### Common UI Issues
- Missing breadcrumb navigation = NEEDS_CHANGES
- Inconsistent spacing or container layout = NEEDS_CHANGES
- Poor empty state design = NEEDS_CHANGES
- Missing hover states on interactive elements = NEEDS_CHANGES

## 🏕️ Boy Scout Rule - Leave Code Better Than You Found It

### When Reviewing Modified Files
**Standard**: Review ALL code in modified files, not just the changed lines
**Rationale**: Ensures consistency and identifies technical debt
**Process**: 
1. Review all methods in files with status `M` (modified) or `A` (added)
2. Document any pre-existing issues found
3. Classify issues by severity and effort
4. Make informed decisions about fixes

### Boy Scout Rule Documentation
When issues are found in unchanged code within modified files:
```markdown
## 🏕️ Boy Scout Rule - Additional Issues Found

### [FileName.cs]
1. **[Method/Issue Name]** - Brief description
   - ❌ Current issue (e.g., Multiple exit points, inconsistent patterns)
   - **Impact**: How this affects code quality/consistency
   - **Effort**: Estimated time to fix (e.g., ~30 minutes)
   - **Recommendation**: Fix now | Create tech debt ticket | Document only

**Team Decision**: 
- [ ] Fix in this PR (recommended for small fixes)
- [ ] Create separate tech debt ticket
- [ ] Leave as-is (must document why)
```

### Boy Scout Categories
1. **Critical Issues** - MUST fix if found:
   - Security vulnerabilities
   - Data corruption risks
   - Breaking changes to public APIs

2. **Consistency Issues** - SHOULD fix:
   - Methods not following established patterns
   - Inconsistent return types within same service
   - Missing null checks in similar methods

3. **Style Issues** - NICE to fix:
   - Naming conventions
   - Code formatting
   - Missing documentation

## 📊 Quality Metrics Tracking

Each code review must document:
- Build warnings count (must be 0)
- Test results (total/passed/failed)
- Code coverage impact
- Performance considerations
- UI consistency compliance (for Blazor components)
- Boy Scout Rule findings (if any)

## 🚀 Enforcement

- AI assistants MUST follow these standards
- Developers MUST resolve all issues before requesting re-review
- NO exceptions without explicit project lead approval

## 📝 Review Report Template

When creating code review reports, always include:
1. Build status with warning/error counts
2. Test execution results
3. Clear APPROVED/NEEDS_CHANGES status
4. Specific issues that need fixing

---

**Remember**: Quality over speed. A warning-free, fully tested codebase prevents future bugs and maintains project health.