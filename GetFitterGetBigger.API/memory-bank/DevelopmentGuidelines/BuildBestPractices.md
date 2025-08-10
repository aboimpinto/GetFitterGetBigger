# Build Best Practices - Quick Reference

## 🎯 The Golden Rule: Always Clean Before Build

```bash
dotnet clean && dotnet build
```

## Why Clean Before Build?

### The Problem with Incremental Builds
- .NET's incremental build system can **hide warnings**
- Files that haven't changed might not be recompiled
- Warnings from unchanged files won't appear
- You might miss critical issues before committing

### Real Example
```bash
# First build - shows warnings
$ dotnet build
Build succeeded with 2 warning(s)

# Make unrelated change, build again - warnings gone!
$ dotnet build  
Build succeeded.

# Clean and build - warnings reappear!
$ dotnet clean && dotnet build
Build succeeded with 2 warning(s)
```

## When to Clean Build

### MANDATORY Clean Build Points
1. **Before ANY commit** - Ensure code is warning-free
2. **After each task completion** - Verify quality gates
3. **Before merging to main** - Final quality check
4. **Before releases** - Production readiness
5. **After pulling changes** - Verify integration

### During Development
- After refactoring
- When switching branches
- If build behaves unexpectedly
- When adding/removing packages

## Build Commands Reference

### Basic Commands
```bash
# Clean all build artifacts
dotnet clean

# Build with detailed output
dotnet build -v normal

# Build specific project
dotnet build MyProject.csproj

# Build release configuration
dotnet build -c Release
```

### Combined Commands (Recommended)
```bash
# Standard development build
dotnet clean && dotnet build

# Full verification
dotnet clean && dotnet build && dotnet test

# Release verification
dotnet clean && dotnet build -c Release && dotnet test -c Release
```

## Common Warning Types to Watch For

### Critical Warnings (Fix Immediately)
- CS8618: Non-nullable property not initialized
- CS8602: Possible null reference
- CS0168: Variable declared but never used
- CS0219: Variable assigned but never used

### Code Quality Warnings
- CS1998: Async method lacks await operators
- CS8603: Possible null reference return
- CS8604: Possible null reference argument

## Build Performance Tips

### When Clean Build is Slow
1. Use parallel builds: `dotnet build -m`
2. Consider selective cleaning:
   ```bash
   dotnet clean MyProject.csproj && dotnet build MyProject.csproj
   ```
3. Use build caching tools for CI/CD

### Incremental Builds During Active Development
- OK to use `dotnet build` for quick iterations
- BUT always clean build before:
  - Committing
  - Testing
  - Sharing code

## Integration with Our Processes

### Feature Implementation
From `FEATURE_IMPLEMENTATION_PROCESS.md`:
```markdown
5. **MANDATORY: Run `dotnet clean && dotnet build`**
   - Always clean first to catch all warnings
```

### Bug Fixes
From `BUG_IMPLEMENTATION_PROCESS.md`:
```markdown
4. **MANDATORY: Run `dotnet clean && dotnet build`**
   - Clean build reveals all warnings
```

### Quality Gates
From `UNIFIED_DEVELOPMENT_PROCESS.md`:
```markdown
✅ `dotnet clean && dotnet build` - zero errors
   - Clean build ensures all warnings are visible
```

## Quick Checklist

Before committing, ask yourself:
- [ ] Did I run `dotnet clean` first?
- [ ] Are there zero build errors?
- [ ] Are warnings at baseline level or better? (BOY SCOUT RULE: Leave code better than you found it)
- [ ] Do all tests pass?

## Pro Tips

1. **Alias for convenience**:
   ```bash
   alias dcb='dotnet clean && dotnet build'
   alias dcbt='dotnet clean && dotnet build && dotnet test'
   ```

2. **Watch for patterns**:
   - Warnings appearing/disappearing? You need clean builds
   - Build faster than expected? Might be skipping files

3. **CI/CD always does clean builds**:
   - Your local build should match CI/CD
   - Catch issues before pushing

## Remember

> "A warning hidden is a bug waiting to happen" 

Always clean before you build! 🧹🏗️