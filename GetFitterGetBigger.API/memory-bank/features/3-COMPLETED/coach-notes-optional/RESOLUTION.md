# Resolution: Coach Notes Are Already Optional

## Investigation Results (854fed7a)

After thorough investigation of the codebase, we found that **coach notes are already completely optional** for exercises.

### Evidence:

1. **No Validation Requirements**:
   - DTOs have no `[Required]` or `[MinLength]` attributes on CoachNotes
   - Service layer properly handles null/empty coach notes collections
   - Entity handlers don't enforce coach notes

2. **Test Evidence**:
   - Existing test `CreateAsync_WithEmptyCoachNotes_CreatesExerciseWithoutNotes` confirms exercises can be created without coach notes
   - Added comprehensive tests verifying null, empty, and missing coach notes work correctly

3. **Service Implementation**:
   - ExerciseService checks `if (request.CoachNotes != null)` before processing
   - Empty or null collections are handled gracefully

## Conclusion

The perceived requirement for coach notes was likely due to:
- Sending invalid coach note objects (e.g., with empty text) instead of an empty array
- UI/client-side validation that might be enforcing requirements not present in the API

## Correct Usage

✅ **Valid requests without coach notes:**
```json
{
  "coachNotes": []  // Empty array
}
```
```json
{
  "coachNotes": null  // Explicitly null
}
```
```json
{
  // Property omitted entirely
}
```

❌ **Invalid request:**
```json
{
  "coachNotes": [
    {
      "text": "",  // Empty text fails validation
      "order": 0
    }
  ]
}
```

## Action Items

No code changes required - coach notes are working as intended. The feature is complete.