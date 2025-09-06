# Commit to Checkpoint

Creates a git commit and automatically adds the commit hash to the current checkpoint of the active IN_PROGRESS feature.

## Usage

```bash
/commit-to-checkpoint
```

The command will:
1. Prompt you for an optional highlight message
2. Analyze uncommitted changes and create a descriptive commit
3. Find the current checkpoint based on completed tasks
4. Add the commit hash to the checkpoint's "Git Commits" section

## Process Flow

### 1. Check Prerequisites
- Verifies an IN_PROGRESS feature exists
- Identifies uncommitted changes to commit
- Locates the appropriate checkpoint

### 2. Gather User Input
- Prompts for optional highlight message
- If no highlight provided, uses auto-generated summary

### 3. Create Git Commit
- Analyzes changes to create descriptive message
- Creates commit with standard Claude signature:
  ```
  🤖 Generated with [Claude Code](https://claude.ai/code)
  
  Authored-By: Paulo Aboim Pinto <aboimpinto@gmail.com>
  ```

### 4. Update Checkpoint
- Finds current checkpoint (last completed task's phase)
- Adds/updates "Git Commits" section
- Format: `- [HASH] - [Highlight + summary or just summary]`

## Checkpoint Detection Logic

The command determines the current checkpoint by:
1. Finding the last completed task in feature-tasks.md
2. Identifying which phase that task belongs to
3. Using that phase's checkpoint (or previous if first task incomplete)

Example:
- If Phase 7 Task 7.3 is complete but 7.4 is not → Phase 7 checkpoint
- If Phase 8 Task 8.1 is not complete → Phase 7 checkpoint (previous)

## Checkpoint Update Format

### If "Git Commits" section exists:
```markdown
Git Commits:
- `existing_hash` - Existing commit message
- `new_hash` - [Your highlight + auto summary]
```

### If "Git Commits" section doesn't exist:
Adds the section following the checkpoint template:
```markdown
Code Review: [existing review info]

Git Commits:
- `new_hash` - [Your highlight + auto summary]

Status: [existing status]
```

## Error Handling

### No IN_PROGRESS Feature
- Informs user no active feature found
- Suggests using standard git commit command

### No Uncommitted Changes
- Informs user there's nothing to commit
- Exits gracefully

### Checkpoint Issues
- If checkpoint doesn't exist: Creates one following template
- If format is non-standard: Adapts to match template
- Never commits to completed checkpoints (violation prevention)

## Example Workflow

```bash
/commit-to-checkpoint

> Optional highlight message (press Enter to skip): Fixed performance issues

# Command analyzes changes, creates commit:
# "fix(admin): resolve test failures and optimize component rendering"

# Updates Phase 7 checkpoint:
Git Commits:
- `4ed4583c` - Fixed performance issues - resolved test failures and optimized rendering
```

## Benefits

- **Automated**: No manual checkpoint editing required
- **Traceable**: Every commit linked to feature progress
- **Compliant**: Follows checkpoint template standards
- **Safe**: Prevents violations (no updates to past checkpoints)
- **Intelligent**: Auto-detects current working checkpoint

## Implementation Details

The command:
1. Uses `git status` to check for changes
2. Uses `git diff` to understand what changed
3. Creates meaningful commit messages
4. Uses `git log -1 --format=%H` to get commit hash
5. Updates feature-tasks.md with proper formatting
6. Validates checkpoint structure against template

## Notes

- Only updates the current active checkpoint
- Preserves existing checkpoint content
- Maintains chronological commit order
- Follows project's git commit standards
- Safe against concurrent modifications