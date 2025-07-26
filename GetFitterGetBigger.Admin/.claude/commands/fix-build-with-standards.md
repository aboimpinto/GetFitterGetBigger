# Fix Build with Standards

Use the csharp-build-test-fixer agent to fix build errors and test failures while strictly adhering to the project's code quality standards.

## Instructions for the Agent

/csharp-build-test-fixer You MUST follow these standards while fixing issues:

### 1. Code Quality Standards (@memory-bank/API-CODE_QUALITY_STANDARDS.md)
- **ServiceResult Pattern**: Use ServiceResult<T> for all service methods
- **Single Exit Point**: NEVER return in the middle of methods - use pattern matching
- **Empty/Null Object Pattern**: Return empty objects, never null
- **Service Architecture Boundaries**: Each service MUST only access its own repository
- **No Fake Async**: Don't use Task.FromResult unless truly needed

### 2. Development Process (@memory-bank/DEVELOPMENT_PROCESS.md)
- **Zero Warnings Policy**: Maintain exactly 0 warnings (Boy Scout Rule)
- **Quality Gates**: All tests must pass (100% pass rate)
- **Clean Build First**: Always run `dotnet clean && dotnet build`
- **Architecture Validation**: Verify service repository boundaries

### 3. Testing Standards (@memory-bank/TESTING-QUICK-REFERENCE.md)
- Check common test failures first
- Follow established testing patterns
- Ensure proper mock setups

### Critical Reminders
- Pattern matching is the PRIMARY tool to avoid multiple returns
- Cross-domain data access MUST use service dependencies, not direct repository access
- Always clean before build to catch all warnings

Fix all issues systematically while maintaining these standards.