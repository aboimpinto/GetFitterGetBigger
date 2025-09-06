---
Command: refine-feature
Description: Refines a Blazor feature from 0-SUBMITTED to 1-READY_TO_DEVELOP state with comprehensive task breakdown
---

# Refine Feature Command - Admin Blazor Project

This command triggers the `blazor-feature-refiner` agent to analyze a Blazor feature and generate detailed implementation tasks.

## Usage

`/refine-feature [FEAT-ID]` - Refine a feature in the 0-SUBMITTED state

Example: `/refine-feature FEAT-022`

## Process

When you use this command with a feature ID (e.g., FEAT-022), I will:

1. **Launch the blazor-feature-refiner agent** to process the feature
2. The agent will autonomously:
   - Analyze ALL files in the feature folder
   - Study existing Blazor components for patterns
   - Generate comprehensive feature-tasks.md
   - Move the feature to 1-READY_TO_DEVELOP

## Agent Details

The specialized `blazor-feature-refiner` agent will handle:
- **Complete Analysis**: Read all documents including UX research and wireframes
- **Blazor Study**: Identify similar components and patterns
- **Task Generation**: Create detailed tasks with bUnit tests
- **UI Standards**: Reference design standards and patterns
- **State Management**: Include proper state service patterns
- **Workflow Movement**: Automatically move to next state

## Output

The agent will create a comprehensive `feature-tasks.md` with:
- Blazor component study task (mandatory first)
- Tasks organized by phases with checkpoints
- bUnit tests integrated with each component
- UI standards and accessibility requirements
- References to existing components

## Requirements

- Feature must exist in `/memory-bank/features/0-SUBMITTED/`
- Feature must have `feature-description.md`
- May include UX research, wireframes, implementation guides

## Agent Location

`.claude/agents/blazor-feature-refiner.md`

## Related Documents

- `/memory-bank/Templates/FeatureCheckpointTemplate.md`
- `/memory-bank/UI_LIST_PAGE_DESIGN_STANDARDS.md`
- `/memory-bank/UI_FORM_PAGE_DESIGN_STANDARDS.md`
- `/memory-bank/COMPREHENSIVE-TESTING-GUIDE.md`

---

**Implementation**: I will now launch the blazor-feature-refiner agent with the feature ID: $ARGUMENT

Using Task tool to launch blazor-feature-refiner agent...