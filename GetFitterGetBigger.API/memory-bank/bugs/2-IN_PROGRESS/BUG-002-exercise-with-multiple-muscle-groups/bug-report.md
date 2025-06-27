# BUG-002: Exercise with Multiple Muscle Groups Not Showing in List

## Bug ID: BUG-002
## Reported: 2025-01-27
## Status: OPEN
## Severity: High
## Affected Version: Current
## Fixed Version: [TBD]

## Description
When an exercise has two or more muscle groups associated with it, the exercise does not appear in the list returned by the GET /api/Exercises endpoint. This is a critical issue as many exercises target multiple muscle groups.

## Error Message
No error message - the exercise is silently excluded from the results.

## Reproduction Steps
1. Create an exercise in the database
2. Associate two or more muscle groups with the exercise
3. Call GET /api/Exercises
4. Expected: Exercise should appear in the list
5. Actual: Exercise does not appear in the list

## Root Cause
[To be determined after investigation]

## Impact
- Users affected: All users trying to view exercises
- Features affected: Exercise listing, workout planning
- Business impact: Exercises with multiple muscle groups are invisible to users, making the system unusable for realistic exercise data

## Workaround
None available - exercises with multiple muscle groups cannot be retrieved through the API

## Test Data
Exercise with:
- Name: "Bench Press"
- Associated with 2 muscle groups: Chest (Primary), Triceps (Secondary)

## Fix Summary
[To be added after fix is implemented]