# BUG-006: Entity Framework Core Design Package Version Mismatch Warning

## Bug ID: BUG-006
## Reported: 2025-01-28
## Status: TODO
## Severity: Low
## Affected Version: Current
## Fixed Version: [TBD]

## Description
There is a version mismatch warning between Entity Framework Core Design package and other EF Core packages in the API project. This warning appears when running EF Core commands like adding migrations or updating the database.

## Error Message
[To be captured next time the warning appears during migration commands]

## Reproduction Steps
1. Run `dotnet ef migrations add <MigrationName>`
2. Or run `dotnet ef database update`
3. A warning about package version mismatch appears in the console output

## Root Cause
The `Microsoft.EntityFrameworkCore.Design` package version likely doesn't match the versions of other EF Core packages like:
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Relational
- Npgsql.EntityFrameworkCore.PostgreSQL

## Impact
- Users affected: Developers running EF Core commands
- Features affected: Database migrations
- Business impact: None (warning only, functionality still works)

## Workaround
The commands still execute successfully despite the warning.

## Fix Summary
1. Check all EF Core package versions in the .csproj file
2. Ensure all EF Core packages use the same version
3. Update any mismatched packages to align versions

## Notes
- This is a non-critical issue that only shows warnings
- Commands still execute successfully
- Should be fixed to maintain clean build output