# BUG-006: Entity Framework Core Design Package Version Mismatch Warning

## Bug ID: BUG-006
## Reported: 2025-01-28
## Status: FIXED
## Severity: Low
## Affected Version: Current
## Fixed Version: 2025-01-29

## Description
There is a version mismatch warning between Entity Framework Core Design package and other EF Core packages in the API project. This warning appears when running EF Core commands like adding migrations or updating the database.

## Error Message
```
The Entity Framework tools version '9.0.2' is older than that of the runtime '9.0.6'. Update the tools for the latest features and bug fixes.
```

## Reproduction Steps
1. Run `dotnet ef migrations add <MigrationName>`
2. Or run `dotnet ef database update`
3. A warning about package version mismatch appears in the console output

## Root Cause
The actual issue was twofold:
1. The `Olimpo.EntityFramework.Persistency` project was using EF Core version 9.0.1 while the API project was using 9.0.6
2. The global dotnet-ef tools were at version 9.0.2 while the runtime was 9.0.6

## Impact
- Users affected: Developers running EF Core commands
- Features affected: Database migrations
- Business impact: None (warning only, functionality still works)

## Workaround
The commands still execute successfully despite the warning.

## Fix Summary
1. Updated `Olimpo.EntityFramework.Persistency.csproj` to use EF Core version 9.0.6 (was 9.0.1)
2. Updated global dotnet-ef tools to version 9.0.6 using `dotnet tool update --global dotnet-ef`

## Fix Details
### Changed Files:
- `/Olimpo.EntityFramework.Persistency/Olimpo.EntityFramework.Persistency.csproj`:
  - Updated `Microsoft.EntityFrameworkCore` from 9.0.1 to 9.0.6
  - Updated `Microsoft.EntityFrameworkCore.Design` from 9.0.1 to 9.0.6

### Commands Run:
```bash
dotnet tool update --global dotnet-ef
```

## Verification
After the fix, running `dotnet ef migrations list` shows no version mismatch warnings.

## Notes
- This was a non-critical issue that only showed warnings
- Commands still executed successfully even with the warning
- Fixed to maintain clean build output and avoid confusion