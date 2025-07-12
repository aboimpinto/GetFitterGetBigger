# Manual Testing Instructions - FEAT-024 BDD Integration Tests

## Prerequisites
- Docker Desktop installed and running
- .NET 9.0 SDK installed
- API project cloned and ready

## Testing Steps

### 1. Verify Docker is Running
```bash
docker --version
docker ps
```
Expected: Docker version displayed and daemon running

### 2. Build the Solution
```bash
cd /home/esqueleto/myWork/GetFitterGetBigger/GetFitterGetBigger.API
dotnet clean && dotnet build
```
Expected: Build succeeds with 0 errors, minimal warnings

### 3. Run BDD Integration Tests
```bash
cd GetFitterGetBigger.API.IntegrationTests
dotnet test --logger "console;verbosity=normal"
```
Expected: 
- All 226 tests pass
- PostgreSQL container starts automatically
- Tests complete in reasonable time (<60 seconds)

### 4. Verify Test Reports
```bash
dotnet test --logger html
```
Check the test results HTML file for:
- All tests green
- No skipped tests
- Execution time per test

### 5. Run Specific Feature Files
```bash
# Test authentication
dotnet test --filter "Category=Authentication"

# Test reference tables
dotnet test --filter "Category=ReferenceData"

# Test exercise management
dotnet test --filter "Category=Exercise"
```
Expected: Each category runs successfully

### 6. Verify Parallel Execution
```bash
# Run both test projects in parallel
cd ..
dotnet test
```
Expected:
- Both API.Tests and API.IntegrationTests run
- No conflicts or port issues
- Total test count = API.Tests + BDD tests

### 7. Check Container Cleanup
```bash
# Before tests
docker ps -a | grep postgres

# Run tests
dotnet test GetFitterGetBigger.API.IntegrationTests

# After tests
docker ps -a | grep postgres
```
Expected: No orphaned PostgreSQL containers

### 8. Verify Coverage Report
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```
Expected: Coverage report generated showing current coverage metrics

## Acceptance Checklist
- [ ] Docker is running
- [ ] Solution builds without errors
- [ ] All 226 BDD tests pass
- [ ] Test execution time < 60 seconds
- [ ] Both test projects run in parallel
- [ ] No orphaned Docker containers
- [ ] Coverage report generated
- [ ] Feature files are readable and understandable

## Known Issues
- Coverage dropped from 89.99% to 83.27% (under investigation)
- 12 edge case tests not yet migrated (5.5% - optional)

## Notes
- The 2 build warnings have been fixed in a separate commit
- All critical functionality is covered by BDD tests
- Migration is 94.5% complete which exceeds the 80% requirement