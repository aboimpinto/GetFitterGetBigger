# Optional Coach Notes - Implementation Tasks

## Feature Branch: `feature/optional-coach-notes`

## MANDATORY RULES:
- **After completing each phase/category below:**
  - ✅ Run `dotnet build` - BUILD MUST BE SUCCESSFUL (no errors)
  - ✅ Run `dotnet test` - ALL TESTS MUST BE GREEN (no failures)
  - ✅ Resolve all compiler warnings (nullable references, unused variables, etc.)
  - ❌ DO NOT proceed to next phase if build fails or tests are red
  - ❌ Fix all issues before moving forward (unless user explicitly asks to skip)

### Phase 1: Remove Coach Notes Required Validation
- **Task 1.1:** Remove coach notes required validation from ExerciseForm.razor `[Completed - commit e6031296]`
- **Task 1.2:** Update form submission logic to filter out empty coach notes `[Completed - commit e6031296]`
- **Task 1.3:** Update CoachNotesEditor to handle completely empty state gracefully `[Already completed]`
- **Task 1.4:** Test form submission with zero coach notes `[Completed - commit e6031296]`

**CHECKPOINT 1:** ✅ `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 2: Update Tests and Validation
- **Task 2.1:** Update ExerciseForm validation tests to reflect optional coach notes `[Completed - commit c5226112]`
- **Task 2.2:** Add test cases for exercises with zero coach notes `[Completed - commit c5226112]`
- **Task 2.3:** Update existing tests that assume coach notes are required `[Completed - commit c5226112]`
- **Task 2.4:** Verify API integration works with empty coach notes array `[Completed - commit c5226112]`

**CHECKPOINT 2:** ✅ `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 3: UI/UX Improvements
- **Task 3.1:** Update form labels to remove required indicators for coach notes `[Completed - commit d3dabab5]`
- **Task 3.2:** Ensure empty state messaging is appropriate for optional field `[Completed - commit d3dabab5]`
- **Task 3.3:** Add helpful placeholder text indicating coach notes are optional `[Completed - commit d3dabab5]`

**CHECKPOINT 3:** ✅ `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 4: Integration Testing
- **Task 4.1:** Test complete exercise CRUD flow with zero coach notes `[Completed - commit 604e42ef]`
- **Task 4.2:** Test exercise creation, editing, and deletion without coach notes `[Completed - commit 604e42ef]`
- **Task 4.3:** Verify coach notes functionality still works when notes are provided `[Completed - commit 604e42ef]`
- **Task 4.4:** Test edge cases (all empty notes, mixed empty/filled notes) `[Completed - commit 604e42ef]`

**FINAL CHECKPOINT:** 🛑 `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

## Notes
- Coach notes should be completely optional - no validation errors if empty
- Individual coach notes still have 1000 character limit when provided
- Empty/whitespace-only coach notes should be filtered out before API submission
- All existing coach notes functionality (add, delete, reorder) must continue working
- Follow existing Blazor patterns and Tailwind CSS styling
- Each implementation task must be immediately followed by its test task