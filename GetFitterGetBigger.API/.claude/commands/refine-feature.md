# Refine Feature Command

This command triggers the `feature-task-refiner` agent to analyze a feature description and break it down into detailed implementation tasks following GetFitterGetBigger's established patterns and best practices.

## Usage

`/refine-feature [FEAT-ID]` - Refine a feature in the 0-SUBMITTED state

Example: `/refine-feature FEAT-026`

**Prerequisites**:
- Feature must exist in `/memory-bank/features/0-SUBMITTED/FEAT-XXX-feature-name/`
- Feature must have a `feature-description.md` file

## How It Works

This command delegates the feature refinement process to the specialized `feature-task-refiner` agent, which will:

1. **Analyze** all files in the feature's `0-SUBMITTED` folder
2. **Study** the codebase for similar implementations and patterns
3. **Generate** a comprehensive `feature-tasks.md` with proper task ordering
4. **Include** references to existing code examples and patterns
5. **Incorporate** lessons learned from completed features
6. **Move** the feature folder from `0-SUBMITTED` to `1-READY_TO_DEVELOP`

## Agent Capabilities

The `feature-task-refiner` agent handles all aspects of feature refinement:

- **Comprehensive Analysis**: Reads ALL files in the feature folder (not just feature-description.md)
- **Codebase Study**: Includes mandatory initial task to study similar implementations
- **TDD Approach**: Integrates tests with each implementation task (no separate testing phase)
- **Quality Checkpoints**: Adds mandatory checkpoints between major categories
- **Pattern Recognition**: References existing code examples and established patterns
- **Automated Workflow**: Moves features through the proper workflow states

## Output

The agent generates a `feature-tasks.md` file containing:
- Pre-implementation checklist with key document references
- Codebase study task (mandatory first task)
- Tasks organized by category with time estimates
- Integrated unit/integration tests with each task
- Checkpoints between categories with specific report formats
- References to similar code and critical patterns
- BOY SCOUT RULE section for improvements

## Implementation Details

When invoked, this command:
1. Validates the feature ID format and existence
2. Launches the `feature-task-refiner` agent with the feature ID
3. The agent autonomously completes the entire refinement process
4. Returns a summary of the work completed

## Agent Location

The agent definition is located at: `.claude/agents/feature-task-refiner.md`

For detailed requirements and process documentation that the agent follows, see:
- `/memory-bank/FEATURE_IMPLEMENTATION_PROCESS.md`
- `/memory-bank/DEVELOPMENT_PROCESS.md`
- `/memory-bank/DevelopmentGuidelines/FeatureWorkflowProcess.md`