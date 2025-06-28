# BUG-005 - Next Steps for Remaining 3 Tests

## Summary of Progress
- **Fixed**: Navigation property loading in ExerciseRepository.AddAsync
- **Result**: 3 of 6 tests now pass (exercise type values correctly populated)
- **Remaining**: 3 tests failing due to Rest type exclusivity business logic

## Remaining Failing Tests

### 1. CreateAsync_WithRestTypeAndOtherTypes_ThrowsInvalidOperationException
**Issue**: Expected InvalidOperationException but got ArgumentNullException
**Root Cause**: The validation might be preventing exercise creation, returning null instead of throwing the expected exception

### 2. UpdateAsync_WithRestTypeAndOtherTypes_ThrowsInvalidOperationException
**Issue**: Same as above for update operations
**Root Cause**: Same validation issue in update flow

### 3. CreateAsync_WithMultipleNonRestTypes_DoesNotThrow
**Issue**: Expected 3 exercise types but got 0
**Root Cause**: The validation logic might be incorrectly rejecting ALL type combinations, not just Rest + Others

## Investigation Plan

### Step 1: Locate Rest Type Validation Logic
- Find where in ExerciseService the Rest type exclusivity is validated
- Check both CreateAsync and UpdateAsync methods
- Look for any code that checks for "Rest" type combined with other types

### Step 2: Analyze Current Validation Behavior
- Determine why ArgumentNullException is thrown instead of InvalidOperationException
- Check if the validation is preventing exercise creation entirely
- Verify if the validation logic is too broad (rejecting all multi-type combinations)

### Step 3: Implement Correct Business Logic
The business rule should be:
- ✅ If exercise has ONLY "Rest" type → Allow
- ✅ If exercise has multiple types but NO "Rest" type → Allow
- ❌ If exercise has "Rest" type + any other type → Throw InvalidOperationException
- The exception message should clearly state the business rule violation

### Step 4: Fix Implementation
Based on findings, either:
- Add missing validation logic
- Fix existing validation to throw correct exception type
- Ensure validation only applies to Rest + Other combinations

## Expected Behavior After Fix

1. **Rest + Other Types**: Should throw InvalidOperationException with message like "Exercise cannot have Rest type combined with other exercise types"

2. **Multiple Non-Rest Types**: Should successfully create/update exercise with all specified types

3. **Only Rest Type**: Should successfully create/update exercise with just Rest type

## Success Criteria
- All 3 remaining tests pass
- Total test count: 349 tests, 340 passed, 0 failed, 9 skipped
- No regression in the 3 tests we already fixed
- Business logic correctly enforces Rest type exclusivity rule