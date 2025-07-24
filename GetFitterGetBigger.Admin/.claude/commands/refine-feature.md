Help me refine feature $ARGUMENT from SUBMITTED to READY_TO_DEVELOP state by:

1. **Reading ALL feature documentation** in `memory-bank/features/0-SUBMITTED/$ARGUMENT/`:
   - Start with `feature-description.md` as the primary source
   - Read ALL other documents in the folder for detailed context and specifications
   - Note any API endpoints, data models, or specific requirements mentioned

2. **Creating a comprehensive task breakdown** (`feature-tasks.md`) following @memory-bank/FEATURE_IMPLEMENTATION_PROCESS.md that includes:
   - Feature branch name
   - Task PHASES (not categories) organized by implementation order
   - Specific tasks with time estimates in the format: `**Task X.Y:** [Description] [ReadyToDevelop] (Est: Xh Ym)`
   - **Testing Rules**:
     - Unit test task IMMEDIATELY after each production code task (services, components, state management)
     - NO unit tests for DTOs/data models
     - Integration test task after each complete workflow implementation
     - Use format: `**Task X.Y:** Create unit tests for [component/service name] [ReadyToDevelop] (Est: Xh Ym)`
   - **CHECKPOINT after each Phase** following @memory-bank/FEATURE_CHECKPOINT_TEMPLATE.md
   - Manual testing phase as the final task
   - Baseline health check section at the beginning

3. **Consulting @memory-bank/CODE_QUALITY_STANDARDS.md** for each task:
   - Look for similar implementation examples for the task at hand
   - Reference specific patterns or standards in the task description
   - If no example is found, highlight: "⚠️ No existing example found for [concept] - possible missing pattern documentation"

4. **Following the task organization pattern by PHASES**:
   - Phase 1: API Service Layer (if needed)
     - Implementation tasks
     - Unit test tasks for each service
     - Integration test for complete API workflow
   - Phase 2: Data Models and DTOs
     - Implementation tasks only (NO unit tests for DTOs)
   - Phase 3: State Management (if needed)
     - Implementation tasks
     - Unit test tasks for state services
   - Phase 4: Shared/Base Components
     - Implementation tasks
     - Unit test tasks for each component
   - Phase 5-N: Feature-specific Components
     - Implementation tasks
     - Unit test tasks for each component
     - Integration test for complete user workflows
   - Final Phases: Navigation/Integration, UI/UX Polish
     - Implementation tasks
     - E2E test tasks for critical user paths

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

9. **Testing Task Guidelines**:
   - **Unit Tests**: Required for ALL production code (services, components, state management, utilities)
   - **NO Unit Tests**: DTOs, data models, POCOs, configuration objects
   - **Integration Tests**: Required after each complete workflow (e.g., full CRUD cycle, user journey)
   - **Test Timing**: Unit test task MUST immediately follow its implementation task
   - **Test Naming**: `**Task X.Y:** Create unit tests for [specific component/service] [ReadyToDevelop] (Est: Xh Ym)`
   - **Coverage Expectation**: Aim for 80%+ coverage on business logic, 100% on critical paths

Ensure all tasks are actionable, testable, and follow the Admin project's Blazor patterns and conventions. Highlight any areas where clarification is needed before implementation can begin.