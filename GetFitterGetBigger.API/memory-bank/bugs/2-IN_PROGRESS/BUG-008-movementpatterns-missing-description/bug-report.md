# BUG-008: MovementPatterns API endpoints not returning Description field

## Bug ID: BUG-008
## Reported: 2025-01-08
## Status: FIXED
## Severity: Medium
## Affected Version: Current
## Fixed Version: Fixed in bugfix/movementpatterns-description-mapping

## Description
The MovementPatterns API endpoints are not returning the `Description` field in their responses, even though the MovementPattern entity has a Description property and it was recently populated with data through FEAT-021. The endpoints are only returning `Id` and `Value` fields through the ReferenceDataDto.

## Error Message
No error message - the issue is missing data in the API response. The endpoints return:
```json
{
  "id": "movementpattern-{guid}",
  "value": "Movement Pattern Name"
}
```

Instead of:
```json
{
  "id": "movementpattern-{guid}",
  "value": "Movement Pattern Name",
  "description": "Detailed description of the movement pattern"
}
```

## Reproduction Steps
1. Start the API server
2. Make a GET request to `/api/movementpatterns`
3. Expected: Response includes id, value, and description fields for each movement pattern
4. Actual: Response only includes id and value fields; description is missing

## Root Cause
The MovementPatternsController was not mapping the Description field from the MovementPattern entity to the ReferenceDataDto, even though the DTO already had a Description property available.

## Impact
- Users affected: Personal Trainers and Clients using the API
- Features affected: Movement pattern display and selection in UI
- Business impact: Recently added movement pattern descriptions (from FEAT-021) are not visible to users, reducing the educational value of the movement pattern taxonomy

## Workaround
None available through the API. The data exists in the database but is not exposed through the endpoints.

## Test Data
Use any of the movement patterns added/updated in FEAT-021:
- Squat
- Hinge
- Lunge
- Horizontal Push
- Vertical Push
- Horizontal Pull
- Vertical Pull
- Carry
- Rotation/Anti-Rotation

## Fix Summary
The fix was straightforward - updated all four endpoints in MovementPatternsController to include the Description field in the DTO mapping:
- GetAll: Added `Description = mp.Description` to the Select projection
- GetById: Added `Description = movementPattern.Description` to the DTO initialization
- GetByName: Added `Description = movementPattern.Description` to the DTO initialization  
- GetByValue: Added `Description = movementPattern.Description` to the DTO initialization

Added 4 integration tests to verify the Description field is properly returned by all endpoints.

## Related Feature
FEAT-021 - Movement Patterns Data Update (added the description data that is not being returned)