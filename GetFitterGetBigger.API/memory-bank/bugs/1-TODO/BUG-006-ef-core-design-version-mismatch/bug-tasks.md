# BUG-006: Entity Framework Core Design Package Version Mismatch - Tasks

## Investigation Phase
- [ ] Run `dotnet ef migrations add TestMigration` to capture the exact warning message
- [ ] Document the complete warning text in bug-report.md
- [ ] Check current package versions in GetFitterGetBigger.API.csproj
- [ ] List all EF Core related packages and their versions
- [ ] Identify which packages have version mismatches

## Fix Implementation
- [ ] Update all EF Core packages to the same version in .csproj
- [ ] Run `dotnet restore` to update packages
- [ ] Test by running `dotnet ef migrations add TestMigration` again
- [ ] Verify warning is resolved
- [ ] Remove the test migration if created

## Verification
- [ ] Run `dotnet build` - should complete without warnings
- [ ] Run `dotnet ef database update` - should complete without version warnings
- [ ] Document the final package versions used

## Documentation
- [ ] Update bug report with actual error message
- [ ] Document the package versions that were mismatched
- [ ] Document the final aligned versions
- [ ] Add a note to CLAUDE.md about checking EF Core package versions