Help me refine feature $ARGUMENT from SUBMITTED to READY_TO_DEVELOP state by:

1. **Reading ALL feature documentation** in `memory-bank/features/0-SUBMITTED/$ARGUMENT/`:
   - Start with `feature-description.md` as the primary source
   - Read ALL other documents in the folder for detailed context and specifications
   - Note any API endpoints, data models, or specific requirements mentioned

2. **Creating a comprehensive task breakdown** (`feature-tasks.md`) following @memory-bank/FEATURE_IMPLEMENTATION_PROCESS.md that includes:
   - Feature branch name
   - Task PHASES (not categories) organized by implementation order
   - Specific tasks with time estimates in the format: `**Task X.Y:** [Description] [ReadyToDevelop] (Est: Xh Ym)`
   - **Testing Rules** (per @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md):
     - Unit test task IMMEDIATELY after each production code task (services, components, state management)
     - NO unit tests for DTOs/data models
     - Integration test task after each complete workflow implementation
     - Use format: `**Task X.Y:** Create unit tests for [component/service name] [ReadyToDevelop] (Est: Xh Ym)`
     - Include test type in description: "Create unit tests (UI interaction + logic) for..." or "Create service tests (happy path, errors, caching) for..."
   - **CHECKPOINT after each Phase** following @memory-bank/FEATURE_CHECKPOINT_TEMPLATE.md
   - Manual testing phase as the final task (create manual testing guide per existing patterns)
   - Baseline health check section at the beginning

3. **Consulting @memory-bank/CODE_QUALITY_STANDARDS.md** for each task:
   - Look for similar implementation examples for the task at hand
   - Reference specific patterns or standards in the task description
   - If no example is found, highlight: "⚠️ No existing example found for [concept] - possible missing pattern documentation"

4. **Following the task organization pattern by PHASES** (with testing requirements from @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md):
   - Phase 1: API Service Layer (if needed)
     - Implementation tasks
     - Unit test tasks for each service (test patterns: happy path, error responses, caching, retry logic)
     - Integration test for complete API workflow
   - Phase 2: Data Models and DTOs
     - Implementation tasks only (NO unit tests for DTOs per testing guidelines)
   - Phase 3: State Management (if needed)
     - Implementation tasks
     - Unit test tasks for state services (test patterns: state changes, error persistence, optimistic updates, rollback)
   - Phase 4: Shared/Base Components
     - Implementation tasks (ensure `data-testid` attributes and `internal` visibility)
     - Unit test tasks for each component (both UI interaction and direct logic tests)
   - Phase 5-N: Feature-specific Components
     - Implementation tasks (follow component testability checklist)
     - Unit test tasks for each component (use bUnit patterns)
     - Integration test for complete user workflows
   - Final Phases: Navigation/Integration, UI/UX Polish
     - Implementation tasks
     - E2E test tasks for critical user paths
     - Manual testing guide creation task

5. **CRITICAL: After each Phase, add a CHECKPOINT section**:
   - Use the format from @memory-bank/FEATURE_CHECKPOINT_TEMPLATE.md
   - Include placeholders for Build Report, Code Review path, Git Commit hash
   - Mark as `[PENDING]` initially
   - Note: "⚠️ Cannot proceed to Phase [X+1] until this checkpoint is APPROVED"

6. **NO ASSUMPTIONS policy**:
   - If any requirement is unclear or ambiguous, create a task: "**Task X.Y:** Clarify [specific requirement] with stakeholder [BLOCKED] (Est: 15m)"
   - List all questions that need clarification in a "## Questions for Clarification" section
   - Do not make assumptions about UI behavior, data formats, or business logic

7. **Moving the feature folder** from `0-SUBMITTED` to `1-READY_TO_DEVELOP` as per @memory-bank/FEATURE_WORKFLOW_PROCESS.md

8. **Updating** `feature-status.md` to reflect the status change with today's date

9. **Testing Task Guidelines** (per @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md and @memory-bank/TEST-COVERAGE-IMPROVEMENT-STRATEGY.md):
   - **Unit Tests**: Required for ALL production code (services, components, state management, utilities)
   - **NO Unit Tests**: DTOs, data models, POCOs, configuration objects
   - **Integration Tests**: Required after each complete workflow (e.g., full CRUD cycle, user journey)
   - **Test Timing**: Unit test task MUST immediately follow its implementation task
   - **Test Naming**: `**Task X.Y:** Create unit tests for [specific component/service] [ReadyToDevelop] (Est: Xh Ym)`
   - **Coverage Expectation**: Aim for 80%+ coverage on business logic, 100% on critical paths
   - **Component Testing Requirements** (from COMPREHENSIVE-TESTING-GUIDE.md):
     - Add `data-testid` attributes to all interactive elements
     - Make key methods and fields `internal` (not private) for test accessibility
     - Plan for both UI interaction tests AND direct logic tests
     - Use bUnit patterns for Blazor component testing
   - **Service Testing Requirements**:
     - Test happy path, error cases, and edge cases
     - Mock HTTP calls and external dependencies
     - Test caching behavior where applicable
     - Verify error message persistence and state rollback
   - **Boy Scout Rule** (from TEST-COVERAGE-IMPROVEMENT-STRATEGY.md):
     - When touching existing components with <70% coverage, add improvement tasks
     - Prioritize components with 0% coverage when they're part of the feature

Ensure all tasks are actionable, testable, and follow the Admin project's Blazor patterns and conventions. Highlight any areas where clarification is needed before implementation can begin.

## Testing Task Summary Requirements
When creating test tasks, ensure they follow these guidelines from the testing documentation:
- **Component Tests**: Reference @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md Section "Blazor Component Testing (bUnit)"
- **Service Tests**: Reference @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md Section "API Service Testing (xUnit)"
- **Coverage Goals**: Follow @memory-bank/TEST-COVERAGE-IMPROVEMENT-STRATEGY.md - aim for 80%+ on new code
- **Test Patterns**: Use established patterns from well-tested components (>80% coverage) as examples
- **Time Estimates**: Generally allocate 30-50% of implementation time for testing tasks