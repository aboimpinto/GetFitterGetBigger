# Add Feature Phase Command

## Description
Adds a new phase to an existing feature's task file when additional work is discovered during testing or implementation. This command ensures proper phase insertion, renumbering, and checkpoint creation.

## Usage
```
/add-feature-phase [FEAT-ID] [brief description]
```

## Examples
```
/add-feature-phase FEAT-022 "Need to add exercise link type restrictions"
/add-feature-phase FEAT-030 "Add validation for bidirectional relationships"
/add-feature-phase "Current feature" "Missing accessibility improvements"
```

## Parameters
- `FEAT-ID` (optional): The feature ID to modify. If omitted or "current", uses the feature currently in progress
- `brief description`: Short description of what needs to be added

## Process
1. **Invokes the feature-phase-adder agent** which will:
   - Analyze the current feature structure
   - Ask clarifying questions about the new phase
   - Determine optimal insertion point
   - Create the phase with proper structure
   - Update all document sections

2. **Interactive questioning** to gather:
   - Detailed problem description
   - Required tasks and components
   - Time estimates
   - Testing requirements
   - Dependencies

3. **Preview and confirmation** before applying changes

## What the Command Does
1. Reads the current feature-tasks.md file
2. Analyzes existing phases and their status
3. Asks detailed questions about the new phase
4. Creates tasks with proper structure:
   - Task descriptions and estimates
   - Implementation notes
   - Test scenarios
   - Deliverables
5. Adds a complete checkpoint section
6. Renumbers all subsequent phases
7. Updates time tracking summaries
8. Updates all cross-references

## Required Information
The agent will ask for:
- **Discovery Context**: What issue was found and when?
- **Business Requirements**: What needs to be implemented?
- **Technical Scope**: Which components are affected?
- **Task Breakdown**: What are the specific tasks?
- **Time Estimates**: How long for each task?
- **Testing Needs**: What tests are required?
- **Insertion Point**: After which current phase?

## File Changes
- Updates: `/memory-bank/features/2-IN_PROGRESS/FEAT-XXX-*/feature-tasks.md`
- References: `/memory-bank/DEVELOPMENT_PROCESS.md`
- Follows patterns from: `/memory-bank/features/3-COMPLETED/`

## Example Interaction
```
User: /add-feature-phase FEAT-022 "Exercise link type restrictions needed"

Agent: I'll help you add a new phase to FEAT-022. Let me ask some questions:

1. What specific issue was discovered during testing?
   > "Warmup exercises can add other warmup exercises, which doesn't make sense"

2. After which phase should this be inserted? (Currently completing Phase 5)
   > "After Phase 5, before Testing & Polish"

3. What are the main tasks needed? (I'll ask for details on each)
   > "1. Implement restrictions, 2. Update validation, 3. Add tests, 4. Update UI"

4. What components need modification?
   > "FourWayLinkedExercisesList.razor, ExerciseLinkValidationService.cs"

5. Estimated time for each task?
   > "Task 1: 1h45m, Task 2: 1h15m, Task 3: 1h, Task 4: 30m"

[Agent shows preview of new Phase 6 structure]

Shall I proceed with adding this phase and renumbering subsequent phases? (y/n)
```

## Benefits
- Maintains consistent phase structure
- Prevents manual renumbering errors
- Ensures proper checkpoint creation
- Updates all document sections automatically
- Follows project standards

## Notes
- Always creates a git-friendly diff by preserving existing content
- Maintains all existing git commit references
- Updates total time estimates automatically
- Preserves completed phase statuses
- Follows the same patterns as the feature-refiner agent